using AzureDevopsExplorer.Database.Model.Data;
using Microsoft.VisualStudio.Services.Common;

namespace AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;

public class DescriptorAcls
{
    public DescriptorAcls(string descriptor, List<(AccessControlTokenParseResult, AccessControl Acl)> parsedAcls, List<NamespacePermission> namespacePermissions)
    {
        Descriptor = descriptor;
        NamespacePermissions = namespacePermissions;

        var aclsByDescriptor = parsedAcls.Where(x => x.Acl.Descriptor == descriptor);
        OrganisationLevel = aclsByDescriptor.Where(x => x.Item1.Type == AccessControlTokenParseResultType.OrganisationLevel).ToList();
        ProjectLevel = aclsByDescriptor.Where(x => x.Item1.Type == AccessControlTokenParseResultType.ProjectLevel).ToList();
        ProjectAndObjectLevel = aclsByDescriptor.Where(x => x.Item1.Type == AccessControlTokenParseResultType.ProjectObjectLevel).ToList();
        CollectionObjectLevel = aclsByDescriptor.Where(x => x.Item1.Type == AccessControlTokenParseResultType.CollectionObjectLevel).ToList();
    }

    public string Descriptor { get; }
    public List<NamespacePermission> NamespacePermissions { get; }
    public List<(AccessControlTokenParseResult, AccessControl Acl)> OrganisationLevel { get; }
    public List<(AccessControlTokenParseResult, AccessControl Acl)> ProjectLevel { get; }
    public List<(AccessControlTokenParseResult, AccessControl Acl)> ProjectAndObjectLevel { get; }
    public List<(AccessControlTokenParseResult, AccessControl Acl)> CollectionObjectLevel { get; }

    public (List<NamespacePermissionAction> Allow, List<NamespacePermissionAction> Deny) EvaluateOrganisationLevelResource(OrganisationScopedResource resource)
    {
        var orgAcls = OrganisationLevel.Where(x => x.Acl.InheritPermissions).ToList();
        var collectionAcls = CollectionObjectLevel.Where(x => x.Item1.ObjectId == resource.Id).ToList();

        var allowActionBits = new HashSet<int>();
        var denyActionBits = new HashSet<int>();

        AddAclsToAllowAndDeny(allowActionBits, denyActionBits, orgAcls);
        AddAclsToAllowAndDeny(allowActionBits, denyActionBits, collectionAcls);

        foreach (var denyActionBit in denyActionBits)
        {
            // deny always overrides allow, if any deny then remove allow
            if (allowActionBits.Contains(denyActionBit))
            {
                allowActionBits.Remove(denyActionBit);
            }
        }
        return (ToPermissionActions(allowActionBits), ToPermissionActions(denyActionBits));
    }

    public (List<NamespacePermissionAction> Allow, List<NamespacePermissionAction> Deny) EvaluateProjectLevelResource(ProjectScopedResource resource)
    {
        var orgAcls = OrganisationLevel.Where(x => x.Acl.InheritPermissions).ToList();
        var projectAcls = ProjectLevel.Where(x => x.Acl.InheritPermissions && x.Item1.ProjectId == resource.ProjectId).ToList();
        var collectionAcls = CollectionObjectLevel.Where(x => x.Item1.ObjectId == resource.Id).ToList();
        var objectAcls = ProjectAndObjectLevel.Where(x => x.Item1.ObjectId == resource.Id && x.Item1.ProjectId == resource.ProjectId).ToList();

        var allowActionBits = new HashSet<int>();
        var denyActionBits = new HashSet<int>();

        AddAclsToAllowAndDeny(allowActionBits, denyActionBits, orgAcls);
        AddAclsToAllowAndDeny(allowActionBits, denyActionBits, projectAcls);
        AddAclsToAllowAndDeny(allowActionBits, denyActionBits, collectionAcls);
        AddAclsToAllowAndDeny(allowActionBits, denyActionBits, objectAcls);

        foreach (var denyActionBit in denyActionBits)
        {
            // deny always overrides allow, if any deny then remove allow
            if (allowActionBits.Contains(denyActionBit))
            {
                allowActionBits.Remove(denyActionBit);
            }
        }
        return (ToPermissionActions(allowActionBits), ToPermissionActions(denyActionBits));
    }

    public (List<NamespacePermissionAction> Allow, List<NamespacePermissionAction> Deny) EvaluateProject(ProjectResource resource)
    {
        var orgAcls = OrganisationLevel.Where(x => x.Acl.InheritPermissions).ToList();
        var projectAcls = ProjectLevel.Where(x => x.Item1.ProjectId == resource.ProjectId).ToList();

        var allowActionBits = new HashSet<int>();
        var denyActionBits = new HashSet<int>();

        AddAclsToAllowAndDeny(allowActionBits, denyActionBits, orgAcls);
        AddAclsToAllowAndDeny(allowActionBits, denyActionBits, projectAcls);

        foreach (var denyActionBit in denyActionBits)
        {
            // deny always overrides allow, if any deny then remove allow
            if (allowActionBits.Contains(denyActionBit))
            {
                allowActionBits.Remove(denyActionBit);
            }
        }
        return (ToPermissionActions(allowActionBits), ToPermissionActions(denyActionBits));
    }

    private List<NamespacePermissionAction> ToPermissionActions(HashSet<int> actionBits)
    {
        return actionBits
            .Select(x => NamespacePermissions.First(y => y.Bit == x))
            .Select(x => new NamespacePermissionAction(x.Name, x.DisplayName, x.Bit))
            .ToList();
    }

    private void AddAclsToAllowAndDeny(HashSet<int> allowActionBits, HashSet<int> denyActionBits, List<(AccessControlTokenParseResult, AccessControl Acl)> parsedAcls)
    {
        foreach (var acl in parsedAcls.Select(x => x.Acl))
        {
            if (acl.Allow != 0)
            {
                var actions =
                    NamespacePermissions
                    .Where(x => x.Permission == acl.Allow)
                    .Select(x => x.Bit);
                allowActionBits.AddRange(actions);
            }

            if (acl.Deny != 0)
            {
                var actions =
                    NamespacePermissions
                    .Where(x => x.Permission == acl.Deny)
                    .Select(x => x.Bit);
                denyActionBits.AddRange(actions);
            }
        }
    }
}
