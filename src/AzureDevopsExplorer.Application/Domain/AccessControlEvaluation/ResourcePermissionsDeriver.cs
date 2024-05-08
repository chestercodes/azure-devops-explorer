using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Data;
using AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;

public class ResourcePermissionsDeriver
{
    public void RunForResourceNamespace(
        DataContext db,
        AccessControlResourceConfig resourceConfig,
        List<ProjectResource> projectResources,
        List<ProjectScopedResource> projectScopedResources,
        List<OrganisationScopedResource> organisationScopedResources)
    {
        var namespaceAcls = GetNamespaceAcls(db, resourceConfig.NamespaceId);
        var namespaceActionsByPermission = GetNamespacePermissions(db, resourceConfig.NamespaceId);

        var parsedAcls =
            namespaceAcls
                .Select(acl => (resourceConfig.Parser.Parse(acl.Token), acl))
                .Where(x => x.Item1 != null)
                .ToList();

        var distinctDescriptors = namespaceAcls.Select(x => x.Descriptor).Distinct();

        List<DerivedPermission> permissions = new();

        foreach (var descriptor in distinctDescriptors)
        {
            var descriptorAcls = new DescriptorAcls(descriptor, parsedAcls, namespaceActionsByPermission);

            foreach (var organisationScopedResource in organisationScopedResources)
            {
                var (allow, deny) = descriptorAcls.EvaluateOrganisationLevelResource(organisationScopedResource);
                foreach (var action in allow)
                {
                    permissions.Add(new DerivedPermission(
                        resourceConfig.NamespaceId,
                        descriptor,
                        organisationScopedResource.Id,
                        null,
                        AllowOrDeny.Allow,
                        action.Name,
                        action.DisplayName,
                        action.Bit)
                    );
                }
                foreach (var action in deny)
                {
                    permissions.Add(new DerivedPermission(
                        resourceConfig.NamespaceId,
                        descriptor,
                        organisationScopedResource.Id,
                        null,
                        AllowOrDeny.Deny,
                        action.Name,
                        action.DisplayName,
                        action.Bit)
                    );
                }
            }

            foreach (var projectScopedResource in projectScopedResources)
            {
                var (allow, deny) = descriptorAcls.EvaluateProjectLevelResource(projectScopedResource);
                foreach (var action in allow)
                {
                    permissions.Add(new DerivedPermission(
                        resourceConfig.NamespaceId,
                        descriptor,
                        projectScopedResource.Id,
                        projectScopedResource.ProjectId,
                        AllowOrDeny.Allow,
                        action.Name,
                        action.DisplayName,
                        action.Bit)
                    );
                }
                foreach (var action in deny)
                {
                    permissions.Add(new DerivedPermission(
                        resourceConfig.NamespaceId,
                        descriptor,
                        projectScopedResource.Id,
                        projectScopedResource.ProjectId,
                        AllowOrDeny.Deny,
                        action.Name,
                        action.DisplayName,
                        action.Bit)
                    );
                }
            }
        }
        AddToDb(db, resourceConfig, permissions);
    }

    private static void AddToDb(DataContext db, AccessControlResourceConfig resourceConfig, List<DerivedPermission> permissions)
    {
        var existing = db.SecurityNamespaceResourcePermission.Where(x => x.NamespaceId == resourceConfig.NamespaceId && x.ResourceType == resourceConfig.ResourceType).ToList();
        db.SecurityNamespaceResourcePermission.RemoveRange(existing);

        var toAdd = permissions.Select(x => new SecurityNamespaceResourcePermission
        {
            AllowOrDeny =
                x.AllowOrDeny == AllowOrDeny.Allow
                    ? SecurityNamespacePermissionAllowOrDeny.Allow
                    : SecurityNamespacePermissionAllowOrDeny.Deny,
            Name = x.Name,
            Bit = x.Bit,
            Descriptor = x.Descriptor,
            DisplayName = x.DisplayName,
            NamespaceId = resourceConfig.NamespaceId,
            ProjectId = x.ProjectId,
            ResourceId = x.ResourceId,
            ResourceType = resourceConfig.ResourceType
        });
        db.SecurityNamespaceResourcePermission.AddRange(toAdd);
        db.SaveChanges();
    }

    private static List<AccessControl> GetNamespaceAcls(DataContext db, Guid namespaceId)
    {
        return db.AccessControl.Where(x => x.NamespaceId == namespaceId).ToList();
    }

    private static List<NamespacePermission> GetNamespacePermissions(DataContext db, Guid namespaceId)
    {
        return (
            from snp in db.SecurityNamespacePermission
            join sna in db.SecurityNamespaceAction
            on new { snp.NamespaceId, snp.ActionBit } equals new { sna.NamespaceId, ActionBit = sna.Bit }
            where snp.NamespaceId == namespaceId
            select new
            {
                snp.NamespaceId,
                snp.Permission,
                sna.Bit,
                sna.Name,
                sna.DisplayName
            })
            .ToList()
            .Select(x => new NamespacePermission(x.Name, x.DisplayName, x.Bit, x.Permission))
            .ToList();
    }
}
