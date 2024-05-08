namespace AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;

public enum AllowOrDeny { Allow, Deny }
public record DerivedPermission(Guid NamespaceId, string Descriptor, string ResourceId, Guid? ProjectId, AllowOrDeny AllowOrDeny, string Name, string DisplayName, int Bit);
public record NamespacePermission(string Name, string DisplayName, int Bit, int Permission);
public record NamespacePermissionAction(string Name, string DisplayName, int Bit);
public record ProjectScopedResource(string Id, Guid ProjectId);
public record ProjectResource(Guid ProjectId);
public record OrganisationScopedResource(string Id);

public enum AccessControlTokenParseResultType
{
    OrganisationLevel,
    ProjectLevel,
    ProjectObjectLevel,
    CollectionObjectLevel
}

public record AccessControlTokenParseResult(
    AccessControlTokenParseResultType Type,
    Guid? ProjectId,
    string? ObjectId);