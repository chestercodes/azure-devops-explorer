using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;
using AzureDevopsExplorer.Application.Entrypoints.Import;
using System.Text.RegularExpressions;
using AzureDevopsExplorer.Database.Model.Security;

namespace AzureDevopsExplorer.Application.Entrypoints.Evaluate;

public class DeriveResourcePermissions
{
    public DeriveResourcePermissions(ICreateDataContexts dataContextFactory)
    {
        this.dataContextFactory = dataContextFactory;
    }

    private ResourcePermissionsDeriver deriver;

    public const string ProjectIdGuid = "(?<ProjectId>[0-9A-Fa-f]{8}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{12})";
    public const string ObjectIdGuid = "(?<ObjectId>[0-9A-Fa-f]{8}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{12})";
    public const string ObjectIdInt = "(?<ObjectId>[0-9]+)";
    private readonly ICreateDataContexts dataContextFactory;

    public List<ProjectResource> All
    {
        get
        {
            using var db = dataContextFactory.Create();
            return db.Project.Select(x => new ProjectResource(x.Id)).ToList();
        }
    }

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
                    [
                        new Regex("AgentQueues.*"),
                        new Regex("MachineGroups.*"),
                        new Regex("DeploymentPools.*")
                    ],
                    "AgentPools",
                    null,
                    null,
                    $"AgentPools/{ObjectIdInt}"
                ),
            SecurityNamespacePermissionResourceType.AgentPool
        );

        using var db = dataContextFactory.Create();
        var agentPools = db.AgentPool.Select(x => new OrganisationScopedResource(x.Id.ToString())).ToList();
        deriver.RunForResourceNamespace(db, config, [], [], agentPools);
    }

    public async Task DeriveBuildPipelinePermissions()
    {
        using var db = dataContextFactory.Create();
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
        var pipelines = db.PipelineCurrent.Select(x => new ProjectScopedResource(x.Id.ToString(), x.ProjectId)).ToList();
        deriver.RunForResourceNamespace(db, resourceConfig, All, pipelines, []);
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
        using var db = dataContextFactory.Create();
        var environments = db.PipelineEnvironment.Select(x => new ProjectScopedResource(x.Id.ToString(), x.ProjectId)).ToList();
        deriver.RunForResourceNamespace(db, config, All, environments, []);
    }

    public async Task DeriveGitRepositoryPermissions()
    {
        using var db = dataContextFactory.Create();
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
        deriver.RunForResourceNamespace(db, resourceConfig, All, projectScopedResources, []);
    }

    public async Task DeriveIdentityPermissions()
    {
        // TODO! theres projects in the ACLs which either don't exist, or i don't have access to
        // check whether the guids are 
        using var db = dataContextFactory.Create();
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
                    "NAMESPACE$",
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
        using var db = dataContextFactory.Create();
        deriver.RunForResourceNamespace(db, resourceConfig, All, [], []);
    }

    public async Task DeriveSecureFilePermissions()
    {
        var config = new AccessControlResourceConfig(
            SecurityNamespaceIds.Library,
            new AccessControlTokenParser(
                    [
                        new Regex($"Library/{ProjectIdGuid}$"),
                        new Regex($"Library/{ProjectIdGuid}/VariableGroup.*"),
                        new Regex($"Library/Collection/OAuthConfiguration"),
                        new Regex($"Library/Collection/VariableGroup.*")
                    ],
                    null,
                    null,
                    $"Library/{ProjectIdGuid}/SecureFile/{ObjectIdGuid}",
                    null
                ),
            SecurityNamespacePermissionResourceType.SecureFile
        );
        // TODO!
        using var db = dataContextFactory.Create();
        var projectScopedResources =
            db.SecureFile
            .Select(x => new ProjectScopedResource(x.Id.ToString(), x.ProjectId))
            .ToList();
        deriver.RunForResourceNamespace(db, config, [], projectScopedResources, []);

    }

    public async Task DeriveServiceEndpointPermissions()
    {
        var resourceConfig = new AccessControlResourceConfig(
        SecurityNamespaceIds.ServiceEndpoints,
        new AccessControlTokenParser(
            [

            ],
            null,
            $"endpoints/{ProjectIdGuid}",
            $"endpoints/{ProjectIdGuid}/{ObjectIdGuid}",
            $"endpoints/Collection/{ProjectIdGuid}"
        ),
        SecurityNamespacePermissionResourceType.ServiceEndpoint
    );

        using var db = dataContextFactory.Create();
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
            .Select(x => new ProjectScopedResource(x.ServiceEndpointId.ToString(), x.ProjectReferenceId))
            .ToList();
        deriver.RunForResourceNamespace(db, resourceConfig, All, projectScopedResources, []);
    }

    public async Task DeriveVariableGroupPermissions()
    {
        var config = new AccessControlResourceConfig(
            SecurityNamespaceIds.Library,
            new AccessControlTokenParser(
                    [
                        new Regex($".*/SecureFile/.*"),
                        new Regex($".*OAuth.*"),
                    ],
                    null,
                    $"Library/{ProjectIdGuid}",
                    $"Library/{ProjectIdGuid}/VariableGroup/{ObjectIdInt}",
                    $"Library/Collection/VariableGroup/{ObjectIdInt}"
                ),
            SecurityNamespacePermissionResourceType.VariableGroup
        );

        using var db = dataContextFactory.Create();
        var projectScopedResources =
            (
                from vg in db.VariableGroup
                join vgp in db.VariableGroupProjectReference
                on vg.Id equals vgp.VariableGroupId
                select new
                {
                    vgp.VariableGroupId,
                    vgp.ProjectReferenceId
                }
            )
            .Select(x => new ProjectScopedResource(x.VariableGroupId.ToString(), x.ProjectReferenceId))
            .ToList();
        deriver.RunForResourceNamespace(db, config, All, projectScopedResources, []);
    }
}
