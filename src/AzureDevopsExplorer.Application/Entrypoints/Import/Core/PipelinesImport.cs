using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Pipelines;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Environment;

public class PipelinesImport
{
    private readonly Mappers mapper;
    private readonly ILogger logger;
    private readonly AzureDevopsProjectDataContext dataContext;

    public PipelinesImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.PipelineCurrentImport)
        {
            await AddPipelines();
        }
    }

    public async Task AddPipelines()
    {
        var importTime = DateTime.UtcNow;
        var pipelineRefsResult = await dataContext.Queries.Pipelines.GetPipelines();
        if (pipelineRefsResult.IsT1)
        {
            logger.LogError(pipelineRefsResult.AsT1.AsError);
            return;
        }

        var pipelinesFromApi = pipelineRefsResult.AsT0
            .Select(x =>
            {
                var mapped = mapper.MapPipelineRef(x);
                mapped.LastImport = importTime;
                mapped.ProjectId = dataContext.Project.ProjectId;
                return mapped;
            })
            .ToList();

        List<int> currentIds = new List<int>();
        using (var db = dataContext.DataContextFactory.Create())
        {
            var currentValues = db.PipelineCurrent.Where(x => x.ProjectId == dataContext.Project.ProjectId).ToList();
            currentIds = currentValues.Select(x => x.Id).ToList();

            foreach (var fromApi in pipelinesFromApi)
            {
                await RunForApiValueNoSaveChanges(fromApi, currentValues, importTime, db);
            }
            await db.SaveChangesAsync();
        }

        await RemoveExistingNotPresentInApiResponse(currentIds, pipelineRefsResult.AsT0, importTime);
    }

    private async Task RunForApiValueNoSaveChanges(Database.Model.Pipelines.PipelineCurrent fromApi, List<Database.Model.Pipelines.PipelineCurrent> currentDbValues, DateTime importTime, DataContext db)
    {
        var existingValue = currentDbValues.SingleOrDefault(x => x.Id == fromApi.Id);
        if (existingValue != null)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string>
                {
                    nameof(Database.Model.Pipelines.PipelineCurrent.LastImport)
                },
                MaxDifferences = 1000
            });

            var comparison = compareLogic.CompareSameType(fromApi, existingValue);
            if (comparison.AreEqual == false)
            {
                db.PipelineCurrent.Remove(existingValue);
                db.PipelineCurrent.Add(fromApi);

                db.PipelineCurrentChange.Add(new PipelineCurrentChange
                {
                    PipelineId = fromApi.Id,
                    PreviousImport = existingValue.LastImport,
                    NextImport = importTime,
                    Difference = comparison.DifferencesString
                });
                return;
            }
            else
            {
                // has not changed
                return;
            }
        }
        else
        {
            db.PipelineCurrent.Add(fromApi);

            db.PipelineCurrentChange.Add(new PipelineCurrentChange
            {
                PipelineId = fromApi.Id,
                PreviousImport = null,
                NextImport = importTime,
                Difference = $"First time or added"
            });
            return;
        }
    }

    private async Task RemoveExistingNotPresentInApiResponse(List<int> existingIds, List<AzureDevopsApi.Pipelines.PipelineRef> pipelineRefs, DateTime importTime)
    {
        var idsFromApi = pipelineRefs.Select(x => x.id).ToList();
        var removed = existingIds.Except(idsFromApi);
        if (removed.Any())
        {
            using (var db = dataContext.DataContextFactory.Create())
            {
                var toRemove = db.PipelineCurrent.Where(x => removed.Contains(x.Id)).ToList();
                db.PipelineCurrentChange.AddRange(toRemove.Select(x =>
                {
                    return new PipelineCurrentChange
                    {
                        PipelineId = x.Id,
                        Difference = $"Removed",
                        PreviousImport = x.LastImport,
                        NextImport = importTime
                    };
                }));
                db.PipelineCurrent.RemoveRange(toRemove);
                await db.SaveChangesAsync();
            }
        }
    }
}
