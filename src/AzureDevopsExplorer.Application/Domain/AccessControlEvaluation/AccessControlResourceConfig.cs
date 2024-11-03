using AzureDevopsExplorer.Database.Model.Security;

namespace AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;

public record AccessControlResourceConfig(
            Guid NamespaceId,
            AccessControlTokenParser Parser,
            SecurityNamespacePermissionResourceType ResourceType
        );
