using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Security;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Pipelines;
using AzureDevopsExplorer.Database.Model.Security;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Security;
public class AccessControlListImport
{
    private readonly AzureDevopsApiOrgQueries queries;
    private readonly ILogger logger;
    private readonly AzureDevopsOrganisationDataContext dataContext;

    public AccessControlListImport(AzureDevopsOrganisationDataContext dataContext)
    {
        queries = dataContext.Queries;
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.AccessControlListImport)
        {
            await Import();
        }
    }

    public async Task Import()
    {
        var namespaceIds = new List<Guid>();
        using (var db = dataContext.DataContextFactory.Create())
        {
            namespaceIds = db.SecurityNamespace.Select(x => x.NamespaceId).ToList();
        }
        var importTime = DateTime.UtcNow;
        foreach (var namespaceId in namespaceIds)
        {
            await AddNamespaceAcls(namespaceId, importTime);
        }
    }

    private async Task AddNamespaceAcls(Guid namespaceId, DateTime importTime)
    {
        using var db = dataContext.DataContextFactory.Create();

        var aclsResult = await queries.Security.GetAclsForNamespace(namespaceId);
        if (aclsResult.IsT1)
        {
            logger.LogError(aclsResult.AsT1.AsError);
            return;
        }

        var aclsFromApi = aclsResult.AsT0.value.ToList();

        EnsureAllPermissionsAreInDatabase(namespaceId, aclsFromApi, db);
        EnsureAllIdentitiesAreInDatabase(aclsFromApi, db);

        var accessControlsToAdd = new List<AccessControl>();
        var allAclsForNamespaceFromApi =
            aclsFromApi
            .SelectMany(x => x.acesDictionary.Values.Select(ace =>
            {
                return new AccessControl
                {
                    Allow = ace.allow,
                    Deny = ace.deny,
                    Descriptor = ace.descriptor,
                    InheritPermissions = x.inheritPermissions,
                    NamespaceId = namespaceId,
                    Token = x.token,
                    LastImport = importTime
                };
            }))
            .ToList();

        var currentVals = db.AccessControl.Where(x => x.NamespaceId == namespaceId).ToList();
        if (currentVals.Count > 0)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string> {
                    nameof(AccessControl.Id),
                    nameof(AccessControl.LastImport)
                },
                IgnoreCollectionOrder = true,
                MaxDifferences = 10000
            });

            var comparison = compareLogic.CompareSameType(currentVals, allAclsForNamespaceFromApi);
            if (comparison.AreEqual == false)
            {
                db.AccessControl.RemoveRange(currentVals);

                db.AccessControlChange.Add(new AccessControlChange
                {
                    NamespaceId = namespaceId,
                    PreviousImport = currentVals.FirstOrDefault()?.LastImport,
                    NextImport = importTime,
                    Difference = comparison.DifferencesString
                });

                db.AccessControl.AddRange(allAclsForNamespaceFromApi);
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
            // not seen before, might have been added or is first import
            db.AccessControl.AddRange(allAclsForNamespaceFromApi);

            db.AccessControlChange.Add(new AccessControlChange
            {
                NamespaceId = namespaceId,
                PreviousImport = null,
                NextImport = importTime,
                Difference = $"First time seeing namespace id {namespaceId}"
            });
            db.SaveChanges();
            return;
        }
    }

    private static void EnsureAllPermissionsAreInDatabase(Guid namespaceId, List<AccessControlList> aclList, DataContext db)
    {
        // making a bit of an assumption that the permissions won't ever change
        // seems like a fairly ok assumption
        var actions = db.SecurityNamespaceAction
            .Where(x => x.NamespaceId == namespaceId)
            .OrderBy(x => x.Bit)
            .ToArray();

        if (actions.Length == 0)
        {
            throw new Exception($"No SecurityNamespaceAction exist for namespace {namespaceId}");
        }

        var distinctPermissions =
            aclList
            .SelectMany(x => x.acesDictionary.Values.SelectMany(y => new[] { y.deny, y.allow }))
            .Distinct()
            .Where(x => x != 0)
            .ToList();

        var permissions = new HashSet<int>(distinctPermissions);

        var existingPermissions = db.SecurityNamespacePermission
            .Where(x => x.NamespaceId == namespaceId)
            .Select(x => x.Permission)
            .Distinct()
            .ToList();

        foreach (var existingPermission in existingPermissions)
        {
            if (permissions.Contains(existingPermission))
            {
                permissions.Remove(existingPermission);
            }
        }

        if (permissions.Count == 0)
        {
            // if all permissions already exist in DB return early
            return;
        }

        var permissionsToAdd = new List<SecurityNamespacePermission>();
        foreach (var permutations in CombinationsPicker.GetCombinations(actions.Length))
        {
            var permutationActions = permutations.Select(x => actions[x]);
            var permission = permutationActions.Select(x => x.Bit).Sum();
            if (permissions.Contains(permission))
            {
                foreach (var p in permutations)
                {
                    var action = actions[p];
                    permissionsToAdd.Add(new SecurityNamespacePermission
                    {
                        NamespaceId = namespaceId,
                        Permission = permission,
                        ActionBit = action.Bit
                    });
                }
                permissions.Remove(permission);
                if (permissions.Count == 0)
                {
                    break;
                }
            }
        }
        db.SecurityNamespacePermission.AddRange(permissionsToAdd);
        db.SaveChanges();
    }

    private static void EnsureAllIdentitiesAreInDatabase(List<AccessControlList> aclList, DataContext db)
    {
        var hasChanged = false;
        var descriptors = aclList.SelectMany(x => x.acesDictionary.Keys).Distinct();
        foreach (var descriptor in descriptors)
        {
            if (db.IdentityImport.Any(x => x.Descriptor == descriptor) == false)
            {
                hasChanged = true;
                db.IdentityImport.Add(new IdentityImport
                {
                    Descriptor = descriptor,
                });
            }
        }
        if (hasChanged)
        {
            db.SaveChanges();
        }
    }
}

public class CombinationsPicker
{
    public static Dictionary<int, IEnumerable<int[]>> Values = new Dictionary<int, IEnumerable<int[]>>();

    public static IEnumerable<int[]> GetCombinations(int length)
    {
        if (Values.ContainsKey(length))
        {
            return Values[length];
        }
        var result = Combinations(Enumerable.Range(0, length));
        Values[length] = result;
        return result;
    }
    private static IEnumerable<T[]> Combinations<T>(IEnumerable<T> source)
    {
        // https://stackoverflow.com/a/57058345/4854368
        if (null == source)
            throw new ArgumentNullException(nameof(source));

        T[] data = source.ToArray();

        return Enumerable
          .Range(1, (1 << data.Length) - 1)
          .Select(index => data
             .Where((v, i) => (index & 1 << i) != 0)
             .ToArray());
    }

}