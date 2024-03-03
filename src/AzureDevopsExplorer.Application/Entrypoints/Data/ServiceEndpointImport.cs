using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Extensions;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using KellermanSoftware.CompareNetObjects;
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
            var importTime = DateTime.UtcNow;
            foreach (var se in serviceEndpoints.Value)
            {
                AddOrUpdateServiceEndpoint(se, importTime);
            }
        },
        err =>
        {
            Console.WriteLine(err.AsError);
        });
    }

    private void AddOrUpdateServiceEndpoint(AzureDevopsApi.Dtos.ServiceEndpoint se, DateTime importTime)
    {
        var endPoint = mapper.MapServiceEndpoint(se);
        endPoint.LastImport = importTime;

        var db = new DataContext();
        var currentServiceEndpoint = db.ServiceEndpoint.SingleOrDefault(x => x.Id == se.Id);
        var currentServiceEndpointData = db.ServiceEndpointData.Where(x => x.ServiceEndpointId == se.Id).ToList();
        var currentServiceEndpointProjectReference = db.ServiceEndpointProjectReference.Where(x => x.ServiceEndpointId == se.Id).ToList();
        var currentServiceEndpointAuthorizationParameter = db.ServiceEndpointAuthorizationParameter.Where(x => x.ServiceEndpointId == se.Id).ToList();

        if (se.Authorization == null || se.Authorization.Parameters == null)
        {
            se.Authorization = new AzureDevopsApi.Dtos.Authorization
            {
                Parameters = new Dictionary<string, string>()
            };
        }
        if (se.ServiceEndpointProjectReferences == null)
        {
            se.ServiceEndpointProjectReferences = [];
        }
        if (se.Data == null)
        {
            se.Data = [];
        }

        var apiAuthorizationParameters = se.Authorization.Parameters.Select(x => new ServiceEndpointAuthorizationParameter
        {
            ServiceEndpointId = endPoint.Id,
            Name = x.Key,
            Value = x.Value
        }).ToList();

        var apiProjectReferences = se.ServiceEndpointProjectReferences.Select(x => new ServiceEndpointProjectReference
        {
            ServiceEndpointId = endPoint.Id,
            Name = x.Name,
            Description = x.Description,
            ProjectReferenceId = x.ProjectReference.Id,
            ProjectReferenceName = x.ProjectReference.Name,
        }).ToList();

        var apiData = se.Data.Select(x => new ServiceEndpointData
        {
            ServiceEndpointId = endPoint.Id,
            Name = x.Key,
            Value = x.Value,
        }).ToList();

        if (currentServiceEndpoint != null)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string>
                {
                    nameof(ServiceEndpoint.LastImport)
                },
                IgnoreCollectionOrder = true,
                MaxDifferences = 1000
            });

            var diffs = new List<string>();
            var seComparison = compareLogic.Compare(currentServiceEndpoint, endPoint);
            if (seComparison.AreEqual == false) { diffs.Add(seComparison.DifferencesString); }

            var sePrComparison = compareLogic.Compare(currentServiceEndpointProjectReference, apiProjectReferences);
            if (sePrComparison.AreEqual == false) { diffs.Add(sePrComparison.DifferencesString); }

            var seApComparison = compareLogic.Compare(currentServiceEndpointAuthorizationParameter, apiAuthorizationParameters);
            if (seApComparison.AreEqual == false) { diffs.Add(seApComparison.DifferencesString); }

            var seDataComparison = compareLogic.Compare(currentServiceEndpointData, apiData);
            if (seDataComparison.AreEqual == false) { diffs.Add(seDataComparison.DifferencesString); }

            if (diffs.Count > 0)
            {
                var combinedDiff = string.Join(Environment.NewLine, diffs);

                db.ServiceEndpointAuthorizationParameter.RemoveRange(currentServiceEndpointAuthorizationParameter);
                db.ServiceEndpointData.RemoveRange(currentServiceEndpointData);
                db.ServiceEndpointProjectReference.RemoveRange(currentServiceEndpointProjectReference);
                db.ServiceEndpoint.RemoveRange(currentServiceEndpoint);

                db.ServiceEndpointProjectReference.AddRange(apiProjectReferences);
                db.ServiceEndpointAuthorizationParameter.AddRange(apiAuthorizationParameters);
                db.ServiceEndpointData.AddRange(apiData);
                db.ServiceEndpoint.Add(endPoint);

                db.ServiceEndpointChange.Add(new ServiceEndpointChange
                {
                    ServiceEndpointId = endPoint.Id,
                    PreviousImport = currentServiceEndpoint.LastImport,
                    NextImport = importTime,
                    Difference = combinedDiff
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
            db.ServiceEndpointProjectReference.AddRange(apiProjectReferences);
            db.ServiceEndpointAuthorizationParameter.AddRange(apiAuthorizationParameters);
            db.ServiceEndpointData.AddRange(apiData);
            db.ServiceEndpoint.Add(endPoint);

            db.ServiceEndpointChange.Add(new ServiceEndpointChange
            {
                ServiceEndpointId = endPoint.Id,
                PreviousImport = null,
                NextImport = importTime,
                Difference = $"First time or added service endpoint {endPoint.Id}"
            });

            db.SaveChanges();
            return;
        }
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
