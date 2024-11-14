using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Environment;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Environment;
public class PipelineEnvironmentImport
{
    private readonly ILogger logger;
    private readonly Mappers mapper;
    private readonly AzureDevopsProjectDataContext dataContext;

    public PipelineEnvironmentImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.PipelineEnvironmentImport)
        {
            await AddPipelineEnvironments();
        }
    }

    public async Task AddPipelineEnvironments()
    {
        logger.LogInformation($"Running environments import");

        var pipelineEnvironmentResult = await dataContext.Queries.Environments.GetPipelineEnvironments();
        if (pipelineEnvironmentResult.IsT1)
        {
            logger.LogError(pipelineEnvironmentResult.AsT1.AsError);
            return;
        }

        var existingIds = new List<int>();
        using (var db = dataContext.DataContextFactory.Create())
        {
            existingIds = db.PipelineEnvironment.Where(x => x.ProjectId == dataContext.Project.ProjectId).Select(x => x.Id).ToList();
        }

        var pipelineEnvironments = pipelineEnvironmentResult.AsT0;
        var importTime = DateTime.UtcNow;
        foreach (var pe in pipelineEnvironments.value)
        {
            AddOrUpdatePipelineEnvironment(pe, importTime);
        }

        await RemoveExistingNotPresentInApiResponse(existingIds, pipelineEnvironments, importTime);
    }

    private async Task RemoveExistingNotPresentInApiResponse(List<int> existingIds, ListResponse<AzureDevopsApi.Environments.PipelineEnvironment> pipelineEnvironments, DateTime importTime)
    {
        var idsFromApi = pipelineEnvironments.value.Select(x => x.id).ToList();
        var removed = existingIds.Except(idsFromApi);
        if (removed.Any())
        {
            using (var db = dataContext.DataContextFactory.Create())
            {
                var toRemove = db.PipelineEnvironment.Where(x => removed.Contains(x.Id)).ToList();
                db.PipelineEnvironmentChange.AddRange(toRemove.Select(x =>
                {
                    return new PipelineEnvironmentChange
                    {
                        PipelineEnvironmentId = x.Id,
                        Difference = $"Removed",
                        PreviousImport = x.LastImport,
                        NextImport = importTime
                    };
                }));
                db.PipelineEnvironment.RemoveRange(toRemove);
                await db.SaveChangesAsync();
            }

        }
    }

    private void AddOrUpdatePipelineEnvironment(AzureDevopsApi.Environments.PipelineEnvironment pe, DateTime importTime)
    {
        var envFromApi = mapper.MapPipelineEnvironment(pe);
        envFromApi.LastImport = importTime;

        var db = dataContext.DataContextFactory.Create();
        var currentPipelineEnvironment = db.PipelineEnvironment.SingleOrDefault(x => x.Id == pe.id);

        if (currentPipelineEnvironment != null)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string>
                {
                    nameof(PipelineEnvironment.LastImport)
                },
                MaxDifferences = 1000
            });

            var comparison = compareLogic.CompareSameType(currentPipelineEnvironment, envFromApi);
            if (comparison.AreEqual == false)
            {
                db.PipelineEnvironment.Remove(currentPipelineEnvironment);
                db.PipelineEnvironment.Add(envFromApi);

                db.PipelineEnvironmentChange.Add(new PipelineEnvironmentChange
                {
                    PipelineEnvironmentId = envFromApi.Id,
                    PreviousImport = currentPipelineEnvironment.LastImport,
                    NextImport = importTime,
                    Difference = comparison.DifferencesString
                });

                db.SaveChanges();
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
            db.PipelineEnvironment.AddRange(envFromApi);

            db.PipelineEnvironmentChange.Add(new PipelineEnvironmentChange
            {
                PipelineEnvironmentId = envFromApi.Id,
                PreviousImport = null,
                NextImport = importTime,
                Difference = $"First time or added pipeline environment {envFromApi.Id}"
            });

            db.SaveChanges();
            return;
        }
    }
}
