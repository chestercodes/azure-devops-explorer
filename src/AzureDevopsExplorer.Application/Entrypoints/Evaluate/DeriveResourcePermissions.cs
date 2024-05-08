using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;
using AzureDevopsExplorer.Application.Entrypoints.Data;
using AzureDevopsExplorer.Database.Model.Data;
using System.Text.RegularExpressions;

namespace AzureDevopsExplorer.Application.Entrypoints.Evaluate;

public class AccessControlResourceConfigs_Todo
{
    public static readonly AccessControlResourceConfig AccountAdminSecurity = new AccessControlResourceConfig(
        Guid.Parse("11238E09-49F2-40C7-94D0-8F0307204CE4"),
        new AccessControlTokenParser(
                [],
                "/Ownership",
                null,
                null,
                null
            ),
        SecurityNamespacePermissionResourceType.Todo
    );

    public static readonly AccessControlResourceConfig BuildAdministration = new AccessControlResourceConfig(
        Guid.Parse("302ACACA-B667-436D-A946-87133492041C"),
        new AccessControlTokenParser(
                [],
                "BuildPrivileges",
                null,
                null,
                null
            ),
        SecurityNamespacePermissionResourceType.Todo
    );

    public static readonly AccessControlResourceConfig Server = new AccessControlResourceConfig(
        Guid.Parse("1F4179B3-6BAC-4D01-B421-71EA09171400"),
        new AccessControlTokenParser(
                [],
                "FrameworkGlobalSecurity",
                null,
                null,
                null
            ),
        SecurityNamespacePermissionResourceType.Server
    );

}

public class DeriveResourcePermissions
{
    private ResourcePermissionsDeriver deriver;

    public const string ProjectIdGuid = "(?<ProjectId>[0-9A-Fa-f]{8}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{12})";
    public const string ObjectIdGuid = "(?<ObjectId>[0-9A-Fa-f]{8}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{12})";
    public const string ObjectIdInt = "(?<ObjectId>[0-9]+)";

    public DeriveResourcePermissions()
    {
        deriver = new ResourcePermissionsDeriver();
    }
    public async Task Run()
    {
        await DeriveAgentPoolPermissions();
        await DeriveBuildPipelinePermissions();
        await DeriveEnvironmentPermissions();
        await DeriveGitRepositoryPermissions();
        await DeriveIdentityPermissions();
        await DeriveOrganisationPermissions();
        await DeriveProjectPermissions();
        await DeriveSecureFilePermissions();
        await DeriveServiceEndpointPermissions();
        await DeriveVariableGroupPermissions();
    }

    public async Task DeriveAgentPoolPermissions()
    {
        var config = new AccessControlResourceConfig(
            SecurityNamespaceIds.DistributedTask,
            new AccessControlTokenParser(
                    [],
                    "AgentPools",
                    null,
                    null,
                    $"AgentPools/{ObjectIdInt}"
                ),
            SecurityNamespacePermissionResourceType.AgentPool
        );
        // TODO!
    }

    public async Task DeriveBuildPipelinePermissions()
    {
        using var db = new DataContext();
        var resourceConfig = new AccessControlResourceConfig(
        SecurityNamespaceIds.Build,
        new AccessControlTokenParser(
                [],
                null,
                $"{ProjectIdGuid}",
                $"{ProjectIdGuid}/.*{ObjectIdInt}", // .* because folders will have their name in token
                null
            ),
        SecurityNamespacePermissionResourceType.Build
    );
        var pipelines = db.Definition.Select(x => new ProjectScopedResource(x.Id.ToString(), x.ProjectId)).ToList();
        deriver.RunForResourceNamespace(db, resourceConfig, [], pipelines, []);
    }

    public async Task DeriveEnvironmentPermissions()
    {
        var config = new AccessControlResourceConfig(
            SecurityNamespaceIds.Environment,
            new AccessControlTokenParser(
                    [],
                    null,
                    $"Environments/{ProjectIdGuid}",
                    $"Environments/{ProjectIdGuid}/{ObjectIdInt}",
                    $"Environments/{ObjectIdInt}"
                ),
            SecurityNamespacePermissionResourceType.Environment
        );
        // TODO!
    }

    public async Task DeriveGitRepositoryPermissions()
    {
        using var db = new DataContext();
        var resourceConfig = new AccessControlResourceConfig(
        SecurityNamespaceIds.GitRepositories,
        new AccessControlTokenParser(
                [
                    new Regex(".*/refs/heads/.*") // ignore branch policies
                ],
                null,
                $"repoV2/{ProjectIdGuid}",
                $"repoV2/{ProjectIdGuid}/{ObjectIdGuid}",
                null
            ),
        SecurityNamespacePermissionResourceType.GitRepository
    );

        var projectScopedResources =
            db.GitRepository
            .Select(x => new ProjectScopedResource(x.Id.ToString(), x.ProjectReferenceId))
            .ToList();
        deriver.RunForResourceNamespace(db, resourceConfig, [], projectScopedResources, []);
    }

    public async Task DeriveIdentityPermissions()
    {
        // TODO! theres projects in the ACLs which either don't exist, or i don't have access to
        // check whether the guids are 
        using var db = new DataContext();
        var resourceConfig = new AccessControlResourceConfig(
        SecurityNamespaceIds.Identity,
        new AccessControlTokenParser(
                [],
                null,
                $"{ProjectIdGuid}",
                $"{ProjectIdGuid}\\\\{ObjectIdGuid}",
                null
            ),
        SecurityNamespacePermissionResourceType.Identity
    );

        var projectScopedResources = new List<ProjectScopedResource>();
        var identities = db.Identity.ToList();
        foreach (var project in db.Project.ToList())
        {
            foreach (var identity in identities)
            {
                projectScopedResources.Add(new ProjectScopedResource(identity.Id.ToString(), project.Id));
            }
        }
        deriver.RunForResourceNamespace(db, resourceConfig, [], projectScopedResources, []);
    }

    public async Task DeriveOrganisationPermissions()
    {
        var collectionConfig = new AccessControlResourceConfig(
            SecurityNamespaceIds.Collection,
            new AccessControlTokenParser(
                    [],
                    "NAMESPACE",
                    null,
                    null,
                    null
                ),
            SecurityNamespacePermissionResourceType.Organisation
        );
        // TODO!
    }

    public async Task DeriveProjectPermissions()
    {
        using var db = new DataContext();
        var resourceConfig = new AccessControlResourceConfig(
        SecurityNamespaceIds.Project,
        new AccessControlTokenParser(
                [],
                null,
                $"\\$PROJECT:vstfs:///Classification/TeamProject/{ProjectIdGuid}",
                null,
                null
            ),
        SecurityNamespacePermissionResourceType.Project
    );
        var projects = db.Project.Select(x => new ProjectResource(x.Id)).ToList();
        deriver.RunForResourceNamespace(db, resourceConfig, projects, [], []);
    }

    public async Task DeriveSecureFilePermissions()
    {
        var config = new AccessControlResourceConfig(
            SecurityNamespaceIds.Library,
            new AccessControlTokenParser(
                    [],
                    null,
                    null,
                    $"Library/{ProjectIdGuid}/SecureFile/{ObjectIdGuid}",
                    null
                ),
            SecurityNamespacePermissionResourceType.SecureFile
        );
        // TODO!
    }

    public async Task DeriveServiceEndpointPermissions()
    {
        using var db = new DataContext();
        var resourceConfig = new AccessControlResourceConfig(
        SecurityNamespaceIds.ServiceEndpoints,
        new AccessControlTokenParser(
            [],
            null,
            $"endpoints/{ProjectIdGuid}",
            $"endpoints/{ProjectIdGuid}/{ObjectIdGuid}",
            $"endpoints/Collection/{ProjectIdGuid}"
        ),
        SecurityNamespacePermissionResourceType.ServiceEndpoint
    );

        var projectScopedResources =
            (
                from se in db.ServiceEndpoint
                join sep in db.ServiceEndpointProjectReference
                on se.Id equals sep.ServiceEndpointId
                select new
                {
                    sep.ServiceEndpointId,
                    sep.ProjectReferenceId
                }
            )
            .Select(x => new ProjectScopedResource(x.ServiceEndpointId, Guid.Parse(x.ProjectReferenceId)))
            .ToList();
        deriver.RunForResourceNamespace(db, resourceConfig, [], projectScopedResources, []);
    }

    public async Task DeriveVariableGroupPermissions()
    {
        var config = new AccessControlResourceConfig(
            SecurityNamespaceIds.Library,
            new AccessControlTokenParser(
                    [],
                    null,
                    $"Library/{ProjectIdGuid}",
                    $"Library/{ProjectIdGuid}/VariableGroup/{ObjectIdInt}",
                    $"Library/Collection/VariableGroup/{ObjectIdInt}"
                ),
            SecurityNamespacePermissionResourceType.VariableGroup
        );
        // TODO!
    }
}
