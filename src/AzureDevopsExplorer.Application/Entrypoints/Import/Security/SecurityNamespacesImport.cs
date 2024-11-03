using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Security;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Security;
public class SecurityNamespacesImport
{
    private readonly AzureDevopsApiOrgQueries queries;
    private readonly ILogger logger;
    private readonly Mappers mapper;
    private readonly AzureDevopsOrganisationDataContext dataContext;

    public SecurityNamespacesImport(AzureDevopsOrganisationDataContext dataContext)
    {
        queries = dataContext.Queries;
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.SecurityNamespaceImport)
        {
            await ImportSecurityNamespaces();
        }
    }

    public async Task ImportSecurityNamespaces()
    {
        var securityNamespacesResult = await queries.Security.GetSecurityNamespaces();
        if (securityNamespacesResult.IsT1)
        {
            logger.LogError(securityNamespacesResult.AsT1.AsError);
            return;
        }

        var securityNamespaces = securityNamespacesResult.AsT0.value;
        foreach (var securityNamespace in securityNamespaces)
        {
            await AddSecurityNamespace(securityNamespace);
        }
    }

    private async Task AddSecurityNamespace(AzureDevopsApi.Security.SecurityNamespace securityNamespace)
    {
        // i don't think that these will change frequently enough to bother to track changes.
        using var db = dataContext.DataContextFactory.Create();

        var namespaceId = Guid.Parse(securityNamespace.namespaceId);

        var namespaceFromApi = mapper.MapSecurityNamespace(securityNamespace);
        var namespaceActionsFromApi = securityNamespace.actions.Select(x =>
            {
                return new SecurityNamespaceAction
                {
                    Bit = x.bit,
                    DisplayName = x.displayName,
                    Name = x.name,
                    NamespaceId = namespaceId,
                };
            })
            .ToList();

        var currentNamespace = db.SecurityNamespace.Where(x => x.NamespaceId == namespaceId).SingleOrDefault();
        var currentNamespaceActions = db.SecurityNamespaceAction.Where(x => x.NamespaceId == namespaceId).ToList();
        if (currentNamespace != null || currentNamespaceActions.Count > 0)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string> {
                        nameof(SecurityNamespaceAction.Id)
                    },
                IgnoreCollectionOrder = true,
                MaxDifferences = 1000
            });

            var namespaceComparison = compareLogic.CompareSameType(currentNamespace, namespaceFromApi);
            var namespaceActionsComparison = compareLogic.CompareSameType(currentNamespaceActions, namespaceActionsFromApi);
            if (namespaceComparison.AreEqual && namespaceActionsComparison.AreEqual)
            {
                return;
            }
            else
            {
                db.SecurityNamespace.Remove(currentNamespace);
                db.SecurityNamespaceAction.RemoveRange(currentNamespaceActions);
            }
        }

        db.SecurityNamespace.Add(namespaceFromApi);
        db.SecurityNamespaceAction.AddRange(namespaceActionsFromApi);
        db.SaveChanges();
    }
}
