using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Security;

public class SecurityNamespacePermissionReport
{
    [Key]
    public long Id { get; set; }
    public Guid NamespaceId { get; set; }
    public string? NamespaceName { get; set; }
    public bool InheritPermissions { get; set; }
    public string Token { get; set; }
    public SecurityNamespacePermissionAllowOrDeny AllowOrDeny { get; set; }
    public int? ActionBit { get; set; }
    public string? ActionName { get; set; }
    public string? ActionDisplayName { get; set; }
    public string IdentityDescriptor { get; set; }
    public Guid? IdentityId { get; set; }
    public string? IdentityName { get; set; }
    public string? IdentityDisplayName { get; set; }
    public bool? IdentityIsGroup { get; set; }
    public int? IdentityMemberCount { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? ResourceId { get; set; }
    public string? ResourceName { get; set; }
    public SecurityNamespacePermissionResourceType? ResourceType { get; set; }
    public AccessControlTokenPermissionScope? PermissionScope { get; set; }

}

public enum AccessControlTokenPermissionScope
{
    Organisation,
    Project,
    ProjectResource,
    CollectionResource
}

