using AzureDevopsExplorer.Database.Model.Security;

namespace AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;

public record AccessControlResourceConfig(
            Guid NamespaceId,
            AccessControlTokenParser Parser,
            AzureDevopsExplorer.Database.Model.Security.SecurityNamespacePermissionResourceType ResourceType
        );
