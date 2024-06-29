using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class AgentPoolImport
{
    private readonly Mappers mapper;
    private readonly ILogger logger;
    private readonly AzureDevopsApiOrgQueries queries;

    public AgentPoolImport(AzureDevopsOrganisationDataContext dataContext)
    {
        logger = dataContext.GetLogger();
        this.queries = dataContext.Queries.Value;
        mapper = new Mappers();
    }

    public async Task Run(DataConfig config)
    {
        if (config.AgentPoolImport)
        {
            await AddAgentPools();
        }
    }

    public async Task AddAgentPools()
    {
        var importTime = DateTime.UtcNow;
        var agentPoolsResult = await queries.GetAgentPools();
        if (agentPoolsResult.IsT1)
        {
            Console.WriteLine(agentPoolsResult.AsT1.AsError);
            return;
        }

        var agentPools = agentPoolsResult.AsT0;
        var agentPoolsFromApi = agentPools.Value
            .Select(x =>
            {
                var mapped = mapper.MapAgentPool(x);
                mapped.LastImport = importTime;
                return mapped;
            })
            .ToList();

        List<int> currentAgentPoolIds = new List<int>();
        using (var db = new DataContext())
        {
            var currentAgentPools = db.AgentPool.ToList();
            currentAgentPoolIds = currentAgentPools.Select(x => x.Id).ToList();

            foreach (var fromApi in agentPoolsFromApi)
            {
                await RunForAgentPoolNoSaveChanges(fromApi, currentAgentPools, importTime, db);
            }
            await db.SaveChangesAsync();
        }

        await RemoveExistingNotPresentInApiResponse(currentAgentPoolIds, agentPools, importTime);
    }

    private async Task RunForAgentPoolNoSaveChanges(Database.Model.Data.AgentPool fromApi, List<Database.Model.Data.AgentPool> currentAgentPools, DateTime importTime, DataContext db)
    {
        var existingAgentPool = currentAgentPools.SingleOrDefault(x => x.Id == fromApi.Id);
        if (existingAgentPool != null)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string>
                {
                    nameof(Database.Model.Data.AgentPool.LastImport)
                },
                MaxDifferences = 1000
            });

            var comparison = compareLogic.CompareSameType(fromApi, existingAgentPool);
            if (comparison.AreEqual == false)
            {
                db.AgentPool.Remove(existingAgentPool);
                db.AgentPool.Add(fromApi);

                db.AgentPoolChange.Add(new AgentPoolChange
                {
                    AgentPoolId = fromApi.Id,
                    PreviousImport = existingAgentPool.LastImport,
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
            db.AgentPool.Add(fromApi);

            db.AgentPoolChange.Add(new AgentPoolChange
            {
                AgentPoolId = fromApi.Id,
                PreviousImport = null,
                NextImport = importTime,
                Difference = $"First time or added"
            });
            return;
        }
    }

    private static async Task RemoveExistingNotPresentInApiResponse(List<int> existingIds, ListResponse<AzureDevopsApi.Dtos.AgentPool> agentPools, DateTime importTime)
    {
        var idsFromApi = agentPools.Value.Select(x => x.Id).ToList();
        var removed = existingIds.Except(idsFromApi);
        if (removed.Any())
        {
            using (var db = new DataContext())
            {
                var toRemove = db.AgentPool.Where(x => removed.Contains(x.Id)).ToList();
                db.AgentPoolChange.AddRange(toRemove.Select(x =>
                {
                    return new AgentPoolChange
                    {
                        AgentPoolId = x.Id,
                        Difference = $"Removed",
                        PreviousImport = x.LastImport,
                        NextImport = importTime
                    };
                }));
                db.AgentPool.RemoveRange(toRemove);
                await db.SaveChangesAsync();
            }
        }
    }
}
