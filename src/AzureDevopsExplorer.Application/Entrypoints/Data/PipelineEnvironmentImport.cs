using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class PipelineEnvironmentImport
{
    private readonly ILogger logger;
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly Mappers mapper;

    public PipelineEnvironmentImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.GetLogger();
        this.httpClient = dataContext.HttpClient.Value;
        mapper = new Mappers();
    }

    public async Task Run(DataConfig config)
    {
        if (config.PipelineEnvironmentImport)
        {
            await AddPipelineEnvironments();
        }
    }

    public async Task AddPipelineEnvironments()
    {
        var queries = new AzureDevopsApiProjectQueries(httpClient);
        var pipelineEnvironmentResult = await queries.GetPipelineEnvironments();
        if (pipelineEnvironmentResult.IsT1)
        {
            Console.WriteLine(pipelineEnvironmentResult.AsT1.AsError);
            return;
        }

        var existingIds = new List<int>();
        using (var db = new DataContext())
        {
            existingIds = db.PipelineEnvironment.Select(x => x.Id).ToList();
        }

        var pipelineEnvironments = pipelineEnvironmentResult.AsT0;
        var importTime = DateTime.UtcNow;
        foreach (var pe in pipelineEnvironments.Value)
        {
            AddOrUpdatePipelineEnvironment(pe, importTime);
        }

        await RemoveExistingNotPresentInApiResponse(existingIds, pipelineEnvironments, importTime);
    }

    private static async Task RemoveExistingNotPresentInApiResponse(List<int> existingIds, ListResponse<AzureDevopsApi.Dtos.PipelineEnvironment> pipelineEnvironments, DateTime importTime)
    {
        var idsFromApi = pipelineEnvironments.Value.Select(x => x.Id).ToList();
        var removed = existingIds.Except(idsFromApi);
        if (removed.Any())
        {
            using (var db = new DataContext())
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

    private void AddOrUpdatePipelineEnvironment(AzureDevopsApi.Dtos.PipelineEnvironment pe, DateTime importTime)
    {
        var envFromApi = mapper.MapPipelineEnvironment(pe);
        envFromApi.LastImport = importTime;

        var db = new DataContext();
        var currentPipelineEnvironment = db.PipelineEnvironment.SingleOrDefault(x => x.Id == pe.Id);

        if (currentPipelineEnvironment != null)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string>
                {
                    nameof(Database.Model.Data.PipelineEnvironment.LastImport)
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
