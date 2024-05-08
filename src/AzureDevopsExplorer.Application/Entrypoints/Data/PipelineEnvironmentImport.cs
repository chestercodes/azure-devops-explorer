using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using KellermanSoftware.CompareNetObjects;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class PipelineEnvironmentImport
{
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly Mappers mapper;

    public PipelineEnvironmentImport(AzureDevopsApiProjectClient httpClient)
    {
        this.httpClient = httpClient;
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
        pipelineEnvironmentResult.Switch(pipelineEnvironments =>
        {
            var importTime = DateTime.UtcNow;
            foreach (var pe in pipelineEnvironments.Value)
            {
                AddOrUpdatePipelineEnvironment(pe, importTime);
            }
        },
        err =>
        {
            Console.WriteLine(err.AsError);
        });
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
                MaxDifferences = 100
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
