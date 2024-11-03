using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database.Extensions;
using AzureDevopsExplorer.Database.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Historical;
public class ServiceEndpointExecutionHistoryImport
{
    private readonly ILogger logger;
    private readonly Mappers mapper;
    private readonly AzureDevopsProjectDataContext dataContext;

    public ServiceEndpointExecutionHistoryImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.ServiceEndpointAddHistory)
        {
            await AddExecutionHistory();
        }
    }

    public async Task AddExecutionHistory()
    {
        List<Guid> serviceEndpointIds = new();

        using (var db = dataContext.DataContextFactory.Create())
        {
            serviceEndpointIds = await db.ServiceEndpoint.Select(x => x.Id).ToListAsync();
        }

        foreach (var serviceEndpointId in serviceEndpointIds)
        {
            await RunForServiceEndpoint(serviceEndpointId);
        }
    }

    private async Task RunForServiceEndpoint(Guid serviceEndpointId)
    {
        using var db = dataContext.DataContextFactory.Create();

        int? currentImportState = db.GetServiceConnectionExecutionHistoryLatestIdValue(serviceEndpointId);
        int? latestImportedId = null;

        var carryOn = true;
        var hasErrored = false;
        string continuationToken = null;
        while (carryOn)
        {
            var result = await dataContext.Queries.ServiceEndpoints.GetNext(serviceEndpointId, continuationToken);
            result.Switch(historyResult =>
            {
                var (data, nextToken) = historyResult;
                if (nextToken == null)
                {
                    carryOn = false;
                }
                else
                {
                    continuationToken = nextToken;
                }
                var idsFromApi = data.Select(x => x.data.id).ToList();

                var alreadyPresentIds = db.ServiceEndpointExecutionHistory
                    .Where(x => idsFromApi.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToList();

                var toAddIds = idsFromApi.Except(alreadyPresentIds).ToList();
                foreach (var historyItem in data)
                {
                    if (currentImportState != null)
                    {
                        if (historyItem.data.id == currentImportState)
                        {
                            carryOn = false;
                            break;
                        }
                    }

                    if (historyItem.data.finishTime == null)
                    {
                        latestImportedId = null;
                    }
                    else
                    {
                        if (latestImportedId == null)
                        {
                            latestImportedId = historyItem.data.id;
                        }
                    }

                    if (toAddIds.Contains(historyItem.data.id) == false)
                    {
                        continue;
                    }

                    var history = mapper.MapServiceEndpointExecutionHistory(historyItem.data);
                    history.EndpointId = Guid.Parse(historyItem.endpointId);
                    db.ServiceEndpointExecutionHistory.Add(history);
                }

                db.SaveChanges();
            },
            err =>
            {
                carryOn = false;
                hasErrored = true;
            });
        }

        if (!hasErrored && latestImportedId != null && latestImportedId != currentImportState)
        {
            db.SetServiceConnectionExecutionHistoryLatestIdValue(serviceEndpointId, latestImportedId.Value);
        }
    }
}
