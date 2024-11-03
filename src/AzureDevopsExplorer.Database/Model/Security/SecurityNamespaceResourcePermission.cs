namespace AzureDevopsExplorer.Database.Model.Security;

public enum SecurityNamespacePermissionResourceType
{
    ServiceEndpoint,
    VariableGroup,
    GitRepository,
    Identity,
    Project,
    Build,
    Organisation,
    AgentPool,
    Environment,
    SecureFile,
    Server,
    Todo
}

public enum SecurityNamespacePermissionAllowOrDeny { Allow, Deny }

public class SecurityNamespaceResourcePermission
{
    public long Id { get; set; }
    public Guid NamespaceId { get; set; }
    public Guid? ProjectId { get; set; }
    public string Descriptor { get; set; }
    public string ResourceId { get; set; }
    public SecurityNamespacePermissionResourceType ResourceType { get; set; }
    public SecurityNamespacePermissionAllowOrDeny AllowOrDeny { get; set; }
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public int Bit { get; set; }
}
