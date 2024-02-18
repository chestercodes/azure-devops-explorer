using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Extensions;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using Microsoft.EntityFrameworkCore;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class ServiceEndpointImport
{
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly Mappers mapper;

    public ServiceEndpointImport(AzureDevopsApiProjectClient httpClient)
    {
        this.httpClient = httpClient;
        mapper = new Mappers();
    }

    public async Task Run(DataConfig config)
    {
        if (config.ServiceEndpointAddLatest)
        {
            await AddServiceEndpoints();
        }
        if (config.ServiceEndpointAddHistory)
        {
            await AddExecutionHistory();
        }
    }

    public async Task AddServiceEndpoints()
    {
        var queries = new AzureDevopsApiProjectQueries(httpClient);
        var serviceEndpointResult = await queries.GetServiceEndpoints();
        serviceEndpointResult.Switch(serviceEndpoints =>
        {
            using var db = new DataContext();

            foreach (var se in serviceEndpoints.Value)
            {
                var endPoint = mapper.MapServiceEndpoint(se);
                if (db.ServiceEndpoint.Any(x => x.Id == endPoint.Id))
                {
                    db.ServiceEndpointAuthorizationParameter.RemoveRange(db.ServiceEndpointAuthorizationParameter.Where(x => x.ServiceEndpointId == endPoint.Id));
                    db.ServiceEndpointData.RemoveRange(db.ServiceEndpointData.Where(x => x.ServiceEndpointId == endPoint.Id));
                    db.ServiceEndpointProjectReference.RemoveRange(db.ServiceEndpointProjectReference.Where(x => x.ServiceEndpointId == endPoint.Id));
                    db.ServiceEndpoint.RemoveRange(db.ServiceEndpoint.Where(x => x.Id == endPoint.Id));
                }

                if (se.Authorization?.Parameters != null)
                {
                    foreach (var p in se?.Authorization?.Parameters)
                    {
                        db.ServiceEndpointAuthorizationParameter.Add(new ServiceEndpointAuthorizationParameter
                        {
                            ServiceEndpointId = endPoint.Id,
                            Name = p.Key,
                            Value = p.Value
                        });
                    }
                }
                if (se.ServiceEndpointProjectReferences != null)
                {
                    foreach (var pr in se?.ServiceEndpointProjectReferences)
                    {
                        db.ServiceEndpointProjectReference.Add(new ServiceEndpointProjectReference
                        {
                            ServiceEndpointId = endPoint.Id,
                            Name = pr.Name,
                            Description = pr.Description,
                            ProjectReferenceId = pr.ProjectReference.Id,
                            ProjectReferenceName = pr.ProjectReference.Name,
                        });
                    }
                }
                if (se.Data != null)
                {
                    foreach (var d in se?.Data)
                    {
                        db.ServiceEndpointData.Add(new ServiceEndpointData
                        {
                            ServiceEndpointId = endPoint.Id,
                            Name = d.Key,
                            Value = d.Value,
                        });
                    }
                }

                db.ServiceEndpoint.Add(endPoint);
            }

            db.SaveChanges();
        },
        err =>
        {
            Console.WriteLine(err.AsError);
        });
    }

    public async Task AddExecutionHistory()
    {
        List<string> serviceEndpointIds = new();

        using (var db = new DataContext())
        {
            serviceEndpointIds = await db.ServiceEndpoint.Select(x => x.Id).ToListAsync();
        }

        foreach (var serviceEndpointId in serviceEndpointIds)
        {
            await RunForServiceEndpoint(serviceEndpointId);
        }
    }

    private async Task RunForServiceEndpoint(string serviceEndpointId)
    {
        using var db = new DataContext();

        var getHistory = new GetServiceEndpointExecutionHistory(httpClient);

        int? currentImportState = db.GetServiceConnectionExecutionHistoryLatestIdValue(serviceEndpointId);
        int? latestImportedId = null;

        var carryOn = true;
        var hasErrored = false;
        string continuationToken = null;
        while (carryOn)
        {
            var result = await getHistory.GetNext(serviceEndpointId, continuationToken);
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
                var idsFromApi = data.Select(x => x.Data.Id).ToList();

                var alreadyPresentIds = db.ServiceEndpointExecutionHistory
                    .Where(x => idsFromApi.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToList();

                var toAddIds = idsFromApi.Except(alreadyPresentIds).ToList();
                foreach (var historyItem in data)
                {
                    if (currentImportState != null)
                    {
                        if (historyItem.Data.Id == currentImportState)
                        {
                            carryOn = false;
                            break;
                        }
                    }

                    if (historyItem.Data.FinishTime == null)
                    {
                        latestImportedId = null;
                    }
                    else
                    {
                        if (latestImportedId == null)
                        {
                            latestImportedId = historyItem.Data.Id;
                        }
                    }

                    if (toAddIds.Contains(historyItem.Data.Id) == false)
                    {
                        continue;
                    }

                    var history = mapper.MapServiceEndpointExecutionHistory(historyItem.Data);
                    history.EndpointId = Guid.Parse(historyItem.EndpointId);
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
