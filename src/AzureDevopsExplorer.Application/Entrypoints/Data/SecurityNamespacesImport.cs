using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class SecurityNamespacesImport
{
    private readonly AzureDevopsApiOrgQueries queries;
    private readonly ILogger logger;
    private readonly Mappers mapper;

    public SecurityNamespacesImport(AzureDevopsOrganisationDataContext dataContext)
    {
        this.queries = dataContext.Queries.Value;
        logger = dataContext.GetLogger();
        mapper = new Mappers();
    }

    public async Task Run(DataConfig config)
    {
        if (config.SecurityNamespaceImport)
        {
            await ImportSecurityNamespaces();
        }
    }

    public async Task ImportSecurityNamespaces()
    {
        var securityNamespacesResult = await queries.GetSecurityNamespaces();
        if (securityNamespacesResult.IsT1)
        {
            Console.WriteLine(securityNamespacesResult.AsT1.AsError);
            return;
        }

        var securityNamespaces = securityNamespacesResult.AsT0.Value;
        foreach (var securityNamespace in securityNamespaces)
        {
            await AddSecurityNamespace(securityNamespace);
        }
    }

    private async Task AddSecurityNamespace(AzureDevopsApi.Dtos.SecurityNamespace securityNamespace)
    {
        // i don't think that these will change frequently enough to bother to track changes.
        using var db = new DataContext();

        var namespaceId = Guid.Parse(securityNamespace.NamespaceId);

        var namespaceFromApi = mapper.MapSecurityNamespace(securityNamespace);
        var namespaceActionsFromApi = securityNamespace.Actions.Select(x =>
            {
                return new SecurityNamespaceAction
                {
                    Bit = x.Bit,
                    DisplayName = x.DisplayName,
                    Name = x.Name,
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
