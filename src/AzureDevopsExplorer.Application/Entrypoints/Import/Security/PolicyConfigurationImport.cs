using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Security;
using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Security;
public class PolicyConfigurationImport
{
    private readonly Mappers mapper;
    private readonly ILogger logger;
    private readonly AzureDevopsProjectDataContext dataContext;

    public PolicyConfigurationImport(AzureDevopsProjectDataContext dataContext)
    {
        mapper = new Mappers();
        logger = dataContext.LoggerFactory.Create(this);
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.PolicyConfigurationImport)
        {
            await Import();
        }
    }

    public async Task Import()
    {
        logger.LogInformation($"Running policy configuration import");

        var policyConfigsResult = await dataContext.Queries.Policy.GetAllPolicyConfigurations();

        if (policyConfigsResult.IsT1)
        {
            logger.LogError(policyConfigsResult.AsT1.AsError);
            return;
        }

        var existingIds = new List<int>();
        using (var db = dataContext.DataContextFactory.Create())
        {
            existingIds = db.PolicyConfiguration
                .Where(x => x.ProjectId == dataContext.Project.ProjectId)
                .Select(x => x.Id).ToList();
        }

        var policyConfigs = policyConfigsResult.AsT0;
        var importTime = DateTime.UtcNow;

        foreach (var policyConfig in policyConfigs.value)
        {
            await AddPolicyConfiguration(policyConfig, importTime);
        }

        await RemoveExistingNotPresentInApiResponse(existingIds, policyConfigs, importTime);
    }

    private async Task AddPolicyConfiguration(AzureDevopsApi.Policy.PolicyConfiguration policyConfig, DateTime importTime)
    {
        using var db = dataContext.DataContextFactory.Create();

        var policyConfigFromApi = mapper.MapPolicyConfiguration(policyConfig);
        var policyConfigSettingsFromApi = policyConfig.settings.FlattenJsonObject().Select(x =>
        {
            return new PolicyConfigurationSetting
            {
                PolicyConfigurationId = policyConfigFromApi.Id,
                Name = x.Key,
                Value = x.Value,
            };
        })
            .ToList();
        policyConfigFromApi.ProjectId = dataContext.Project.ProjectId;
        policyConfigFromApi.LastImport = importTime;
        policyConfigFromApi.Settings = policyConfigSettingsFromApi;

        var currentValue = db.PolicyConfiguration
            .Include(x => x.Settings)
            .Where(x => x.Id == policyConfig.id)
            .SingleOrDefault();

        if (currentValue != null)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string> {
                        nameof(PolicyConfigurationSetting.Id),
                        nameof(PolicyConfiguration.LastImport),
                    },
                IgnoreCollectionOrder = true,
                MaxDifferences = 1000
            });

            var comparison = compareLogic.CompareSameType(currentValue, policyConfigFromApi);
            if (comparison.AreEqual)
            {
                return;
            }
            else
            {
                db.PolicyConfiguration.Remove(currentValue);
                db.PolicyConfiguration.Add(policyConfigFromApi);

                db.PolicyConfigurationChange.Add(new PolicyConfigurationChange
                {
                    PolicyId = policyConfig.id,
                    PreviousImport = currentValue.LastImport,
                    NextImport = importTime,
                    Difference = comparison.DifferencesString,
                });

                db.SaveChanges();
                return;
            }
        }

        db.PolicyConfiguration.Add(policyConfigFromApi);

        db.PolicyConfigurationChange.Add(new PolicyConfigurationChange
        {
            PolicyId = policyConfig.id,
            PreviousImport = null,
            NextImport = importTime,
            Difference = $"First time or added {policyConfig.id}"
        });

        db.SaveChanges();
        return;
    }

    private async Task RemoveExistingNotPresentInApiResponse(List<int> existingIds, ListResponse<AzureDevopsApi.Policy.PolicyConfiguration> policyConfigs, DateTime importTime)
    {
        var idsFromApi = policyConfigs.value.Select(x => x.id).ToList();
        var removed = existingIds.Except(idsFromApi);
        if (removed.Any())
        {
            using (var db = dataContext.DataContextFactory.Create())
            {
                var toRemove = db.PolicyConfiguration.Where(x => removed.Contains(x.Id)).ToList();
                db.PolicyConfigurationChange.AddRange(toRemove.Select(x =>
                {
                    return new PolicyConfigurationChange
                    {
                        PolicyId = x.Id,
                        Difference = $"Removed",
                        PreviousImport = x.LastImport,
                        NextImport = importTime
                    };
                }));
                db.PolicyConfiguration.RemoveRange(toRemove);
                await db.SaveChangesAsync();
            }

        }
    }
}
