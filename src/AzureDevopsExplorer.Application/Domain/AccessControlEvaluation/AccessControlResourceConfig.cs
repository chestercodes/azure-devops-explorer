using AzureDevopsExplorer.Database.Model.Data;

namespace AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;

public record AccessControlResourceConfig(
            Guid NamespaceId,
            AccessControlTokenParser Parser,
            SecurityNamespacePermissionResourceType ResourceType
        );
