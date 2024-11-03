using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Security;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static AzureDevopsExplorer.AzureDevopsApi.ApprovalsAndChecks.ProjectQueries;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Security;
public class CheckConfigurationImport
{
    private readonly ILogger logger;
    private readonly Mappers mapper;
    private readonly AzureDevopsProjectDataContext dataContext;

    public CheckConfigurationImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.CheckConfigurationImport)
        {
            await AddCheckConfigurations();
        }
    }

    public async Task AddCheckConfigurations()
    {
        CheckConfigurationsQueryResource[] allCheckResources = null;

        using (var db = dataContext.DataContextFactory.Create())
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

        // TODO? probably should chunk this call and response handling
        var result = await dataContext.Queries.ApprovalsAndChecks.CheckConfigurationsQuery(allCheckResources);
        if (result.IsT1)
        {
            logger.LogError(result.AsT1.AsError);
            return;
        }

        var lastImport = DateTime.UtcNow;
        var checkConfigs = result.AsT0;

        var existingIds = new List<int>();
        using (var db = dataContext.DataContextFactory.Create())
        {
            existingIds = db.CheckConfiguration.Select(x => x.Id).ToList();
        }

        if (checkConfigs.value != null)
        {
            foreach (var check in checkConfigs.value ?? [])
            {
                RunAddOrUpdate(check, lastImport);
            }

            await RemoveExistingNotPresentInApiResponse(lastImport, checkConfigs, existingIds);
        }
    }

    private async Task RemoveExistingNotPresentInApiResponse(DateTime lastImport, ListResponse<AzureDevopsApi.ApprovalsAndChecks.CheckConfiguration> checkConfigs, List<int> existingIds)
    {
        var idsFromApi = checkConfigs.value.Select(x => x.id).ToList();
        var removed = existingIds.Except(idsFromApi);
        if (removed.Any())
        {
            using (var db = dataContext.DataContextFactory.Create())
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

    private void RunAddOrUpdate(AzureDevopsApi.ApprovalsAndChecks.CheckConfiguration check, DateTime lastImport)
    {
        var mapped = mapper.MapCheckConfiguration(check);
        mapped.Settings = JsonSerializer.Serialize(check.settings);
        using var db = dataContext.DataContextFactory.Create();
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
                db.CheckConfigurationChange.Add(new CheckConfigurationChange
                {
                    CheckId = mapped.Id,
                    ResourceId = check.resource.id,
                    ResourceType = check.resource.type,
                    ResourceName = check.resource.name,
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
            db.CheckConfigurationChange.Add(new CheckConfigurationChange
            {
                CheckId = check.id,
                ResourceId = check.resource.id,
                ResourceType = check.resource.type,
                ResourceName = check.resource.name,
                PreviousImport = null,
                NextImport = lastImport,
                Difference = $"Added or first time seeing check {check.id}.",
            });
            mapped.LastImport = lastImport;
            db.CheckConfiguration.Add(mapped);
            db.SaveChanges();
        }
    }
}
