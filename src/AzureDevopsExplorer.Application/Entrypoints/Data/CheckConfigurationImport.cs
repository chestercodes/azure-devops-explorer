using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static AzureDevopsExplorer.AzureDevopsApi.AzureDevopsApiProjectQueries;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class CheckConfigurationImport
{
    private readonly ILogger logger;
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly Mappers mapper;

    public CheckConfigurationImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.GetLogger();
        this.httpClient = dataContext.HttpClient.Value;
        mapper = new Mappers();
    }

    public async Task Run(DataConfig config)
    {
        if (config.CheckConfigurationImport)
        {
            await AddCheckConfigurations();
        }
    }

    public async Task AddCheckConfigurations()
    {
        CheckConfigurationsQueryResource[] allCheckResources = null;

        using (var db = new DataContext())
        {
            var serviceConnectionIds = db.ServiceEndpoint
                .Where(x => x.Type == "azurerm")
                .Select(x => new { x.Id, x.Name })
                .ToList()
                .Select(x => new CheckConfigurationsQueryResource(x.Id.ToString(), x.Name, "endpoint"))
                .ToList();
            var variableGroupIds = db.VariableGroup
                .Select(x => new { x.Id, x.Name })
                .ToList()
                .Select(x => new CheckConfigurationsQueryResource(x.Id.ToString(), x.Name, "variablegroup"))
                .ToList();
            var pipelineEnvironmentsIds = db.PipelineEnvironment
                .Select(x => new { x.Id, x.Name })
                .ToList()
                .Select(x => new CheckConfigurationsQueryResource(x.Id.ToString(), x.Name, "environment"))
                .ToList();
            var secureFileIds = db.SecureFile
                .Select(x => new { x.Id, x.Name })
                .ToList()
                .Select(x => new CheckConfigurationsQueryResource(x.Id.ToString(), x.Name, "securefile"))
                .ToList();
            allCheckResources = serviceConnectionIds.Union(variableGroupIds).Union(pipelineEnvironmentsIds).Union(secureFileIds).ToArray();
        }

        var queries = new AzureDevopsApiProjectQueries(httpClient);
        // TODO? probably should chunk this call and response handling
        var result = await queries.CheckConfigurationsQuery(allCheckResources);
        if (result.IsT1)
        {
            Console.WriteLine(result.AsT1.AsError);
            return;
        }

        var lastImport = DateTime.UtcNow;
        var checkConfigs = result.AsT0;

        var existingIds = new List<int>();
        using (var db = new DataContext())
        {
            existingIds = db.CheckConfiguration.Select(x => x.Id).ToList();
        }

        foreach (var check in checkConfigs.Value)
        {
            RunAddOrUpdate(check, lastImport);
        }

        await RemoveExistingNotPresentInApiResponse(lastImport, checkConfigs, existingIds);
    }

    private static async Task RemoveExistingNotPresentInApiResponse(DateTime lastImport, ListResponse<ConfigurationCheck> checkConfigs, List<int> existingIds)
    {
        var idsFromApi = checkConfigs.Value.Select(x => x.Id).ToList();
        var removed = existingIds.Except(idsFromApi);
        if (removed.Any())
        {
            using (var db = new DataContext())
            {
                var toRemove = db.CheckConfiguration.Where(x => removed.Contains(x.Id)).ToList();
                db.CheckConfigurationChange.AddRange(toRemove.Select(x =>
                {
                    return new CheckConfigurationChange
                    {
                        CheckId = x.Id,
                        Difference = $"Removed",
                        PreviousImport = x.LastImport,
                        NextImport = lastImport,
                        ResourceId = x.ResourceId,
                        ResourceName = x.ResourceName,
                        ResourceType = x.ResourceType
                    };
                }));
                db.CheckConfiguration.RemoveRange(toRemove);
                await db.SaveChangesAsync();
            }
        }
    }

    private void RunAddOrUpdate(AzureDevopsApi.Dtos.ConfigurationCheck check, DateTime lastImport)
    {
        var mapped = mapper.MapCheckConfiguration(check);
        mapped.Settings = JsonSerializer.Serialize(check.Settings);
        using var db = new DataContext();
        var currentCheck = db.CheckConfiguration.SingleOrDefault(x => x.Id == mapped.Id);
        if (currentCheck != null)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string>
                {
                    nameof(CheckConfiguration.LastImport)
                },
                MaxDifferences = 1000
            });
            var result = compareLogic.CompareSameType(currentCheck, mapped);
            if (!result.AreEqual)
            {
                db.CheckConfiguration.Remove(currentCheck);
                db.CheckConfigurationChange.Add(new Database.Model.Data.CheckConfigurationChange
                {
                    CheckId = mapped.Id,
                    ResourceId = check.Resource.Id,
                    ResourceType = check.Resource.Type,
                    ResourceName = check.Resource.Name,
                    PreviousImport = currentCheck.LastImport,
                    NextImport = lastImport,
                    Difference = result.DifferencesString,
                });
                mapped.LastImport = lastImport;
                db.CheckConfiguration.Add(mapped);
                db.SaveChanges();
                return;
            }
            else
            {
                // exists and hasn't changed, do not save because it will set last import to null
                return;
            }
        }
        else
        {
            // check doesn't exist in DB, might be added or new
            db.CheckConfigurationChange.Add(new Database.Model.Data.CheckConfigurationChange
            {
                CheckId = check.Id,
                ResourceId = check.Resource.Id,
                ResourceType = check.Resource.Type,
                ResourceName = check.Resource.Name,
                PreviousImport = null,
                NextImport = lastImport,
                Difference = $"Added or first time seeing check {check.Id}.",
            });
            mapped.LastImport = lastImport;
            db.CheckConfiguration.Add(mapped);
            db.SaveChanges();
        }
    }
}
