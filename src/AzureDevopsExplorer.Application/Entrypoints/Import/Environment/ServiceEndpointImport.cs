using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi.ServiceEndpoints;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Environment;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Environment;
public class ServiceEndpointImport
{
    private readonly ILogger logger;
    private readonly Mappers mapper;
    private readonly AzureDevopsProjectDataContext dataContext;

    public ServiceEndpointImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.ServiceEndpointAddLatest)
        {
            await AddServiceEndpoints();
        }
    }

    public async Task AddServiceEndpoints()
    {
        // TODO? figure out whether to remove values which are not in API response
        // or whether we can't be sure that it's not being used by another project
        var serviceEndpointResult = await dataContext.Queries.ServiceEndpoints.GetServiceEndpoints();
        serviceEndpointResult.Switch(serviceEndpoints =>
        {
            var importTime = DateTime.UtcNow;
            foreach (var se in serviceEndpoints.value)
            {
                AddOrUpdateServiceEndpoint(se, importTime);
            }
        },
        err =>
        {
            logger.LogError(err.AsError);
        });
    }

    private void AddOrUpdateServiceEndpoint(AzureDevopsApi.ServiceEndpoints.ServiceEndpoint se, DateTime importTime)
    {
        var endPoint = mapper.MapServiceEndpoint(se);
        endPoint.LastImport = importTime;

        using var db = dataContext.DataContextFactory.Create();
        var currentServiceEndpoint = db.ServiceEndpoint.SingleOrDefault(x => x.Id == Guid.Parse(se.id));
        var currentServiceEndpointData = db.ServiceEndpointData.Where(x => x.ServiceEndpointId == Guid.Parse(se.id)).ToList();
        var currentServiceEndpointProjectReference = db.ServiceEndpointProjectReference.Where(x => x.ServiceEndpointId == Guid.Parse(se.id)).ToList();
        var currentServiceEndpointAuthorizationParameter = db.ServiceEndpointAuthorizationParameter.Where(x => x.ServiceEndpointId == Guid.Parse(se.id)).ToList();

        if (se.authorization == null || se.authorization.parameters == null)
        {
            se.authorization = new Authorization
            {
                parameters = new Dictionary<string, string>()
            };
        }
        if (se.serviceEndpointProjectReferences == null)
        {
            se.serviceEndpointProjectReferences = [];
        }
        if (se.data == null)
        {
            se.data = [];
        }

        var apiAuthorizationParameters = se.authorization.parameters.Select(x => new ServiceEndpointAuthorizationParameter
        {
            ServiceEndpointId = endPoint.Id,
            Name = x.Key,
            Value = x.Value
        }).ToList();

        var apiProjectReferences = se.serviceEndpointProjectReferences.Select(x => new ServiceEndpointProjectReference
        {
            ServiceEndpointId = endPoint.Id,
            Name = x.name,
            Description = x.description,
            ProjectReferenceId = Guid.Parse(x.projectReference.id),
            ProjectReferenceName = x.projectReference.name,
        }).ToList();

        var apiData = se.data.Select(x => new ServiceEndpointData
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
                    nameof(Database.Model.Environment.ServiceEndpoint.LastImport)
                },
                IgnoreCollectionOrder = true,
                MaxDifferences = 1000
            });

            var diffs = new List<string>();
            var seComparison = compareLogic.CompareSameType(currentServiceEndpoint, endPoint);
            if (seComparison.AreEqual == false) { diffs.Add(seComparison.DifferencesString); }

            var sePrComparison = compareLogic.CompareSameType(currentServiceEndpointProjectReference, apiProjectReferences);
            if (sePrComparison.AreEqual == false) { diffs.Add(sePrComparison.DifferencesString); }

            var seApComparison = compareLogic.CompareSameType(currentServiceEndpointAuthorizationParameter, apiAuthorizationParameters);
            if (seApComparison.AreEqual == false) { diffs.Add(seApComparison.DifferencesString); }

            var seDataComparison = compareLogic.CompareSameType(currentServiceEndpointData, apiData);
            if (seDataComparison.AreEqual == false) { diffs.Add(seDataComparison.DifferencesString); }

            if (diffs.Count > 0)
            {
                var combinedDiff = string.Join(System.Environment.NewLine, diffs);

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
}
