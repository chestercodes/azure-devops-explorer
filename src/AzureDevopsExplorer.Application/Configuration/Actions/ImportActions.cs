namespace AzureDevopsExplorer.Application.Configuration.Actions;

public class ImportAction
{
    public string Command { get; set; }
    public string Info { get; set; }
    public ApplicationConfig Config { get; set; }
}

public class ImportActions
{
    public static List<ImportAction> All
    {
        get
        {
            return new List<ImportAction>
            {
                new ImportAction
                {
                    Command = "all-build-and-pipeline",
                    Info = "Add all build and pipeline entities",
                    Config = new ImportConfig
                    {
                        BuildAddArtifacts = true,
                        BuildAddPipelineRun = true,
                        BuildAddTimeline = true,
                        BuildRunYamlAnalysis = true,
                        PipelineCurrentImport = true,
                        PipelineRunImport = true,
                        PipelineRunTemplateImport = true,
                    }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "acl-import",
                    Info = "Import access control lists.",
                    Config = new ImportConfig { AccessControlListImport = true, SecurityNamespaceImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "agent-pool-import",
                    Info = "Import agent pools.",
                    Config = new ImportConfig { AgentPoolImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "builds-add-from-start",
                    Info = "Add all builds in order of queued from start.",
                    Config = new ImportConfig { PipelineCurrentImport = true, BuildsAddFromStart = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "builds-add-latest-default",
                    Info = "Add latest build from each pipeline.",
                    Config = new ImportConfig { PipelineCurrentImport = true, BuildsAddLatestDefaultFromPipeline = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "build-add-artifacts",
                    Info = "Add build artifacts for builds.",
                    Config = new ImportConfig { BuildAddArtifacts = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "build-add-timeline",
                    Info = "Add build timeline for builds.",
                    Config = new ImportConfig { BuildAddTimeline = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "build-add-pipeline-run",
                    Info = "Add pipeline run for builds.",
                    Config = new ImportConfig { BuildAddPipelineRun = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "build-run-yaml-analysis",
                    Info = "Add analysis of expanded yaml file to get the used service connections and variable groups.",
                    Config = new ImportConfig { BuildRunYamlAnalysis = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "code-search-import",
                    Info = "Add code search keywords.",
                    Config = new ImportConfig { CodeSearchImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "check-configuration-import",
                    Info = "Add check configurations.",
                    Config = new ImportConfig { CheckConfigurationImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "git-add-repositories",
                    Info = "Add git repositories.",
                    Config = new ImportConfig { GitAddRepositories = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "git-add-pull-requests",
                    Info = "Add git pull requests.",
                    Config = new ImportConfig { GitAddPullRequests = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "graph-add-entra-applications",
                    Info = "Add Entra applications from graph API.",
                    Config = new ImportConfig { GraphAddEntraApplications = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "graph-add-entra-groups",
                    Info = "Add Entra groups from graph API.",
                    Config = new ImportConfig { GraphAddEntraGroups = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "graph-add-users",
                    Info = "Add graph users from Azure Devops graph API.",
                    Config = new ImportConfig { GraphAddUsers = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "graph-add-sp",
                    Info = "Add graph service principals from Azure Devops graph API.",
                    Config = new ImportConfig { GraphAddServicePrincipals = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "graph-add-groups",
                    Info = "Add graph groups from Azure Devops graph API.",
                    Config = new ImportConfig { GraphAddGroups = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "graph-add-group-memberships",
                    Info = "Add graph group memberships from Azure Devops graph API for each of the graph groups.",
                    Config = new ImportConfig { GraphAddGroupMemberships = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "identity-import",
                    Info = "Add identity.",
                    Config = new ImportConfig { IdentityImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "pipeline-import",
                    Info = "Add pipelines.",
                    Config = new ImportConfig { PipelineCurrentImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "pipeline-environment-import",
                    Info = "Add pipeline environments information.",
                    Config = new ImportConfig { PipelineEnvironmentImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "pipeline-permissions-import",
                    Info = "Add pipeline approval permissions information.",
                    Config = new ImportConfig { PipelinePermissionsImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "pipeline-run-import",
                    Info = "Add pipeline run information.",
                    Config = new ImportConfig { PipelineRunImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "pipeline-run-template-import",
                    Info = "Add pipeline run template information.",
                    Config = new ImportConfig { PipelineRunTemplateImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "policy-configuration-import",
                    Info = "Add policy configuration information.",
                    Config = new ImportConfig { PolicyConfigurationImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "service-endpoint-add-latest",
                    Info = "Add latest values for service endpoints.",
                    Config = new ImportConfig { ServiceEndpointAddLatest = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "secure-file-import",
                    Info = "Add latest values for secure files.",
                    Config = new ImportConfig { SecureFileImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "security-namespace-import",
                    Info = "Add security namespaces and analyse the permissions.",
                    Config = new ImportConfig { SecurityNamespaceImport = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "service-endpoint-add-history",
                    Info = "Add history for service endpoints.",
                    Config = new ImportConfig { ServiceEndpointAddHistory = true }.ToApplicationConfig()
                },
                new ImportAction
                {
                    Command = "variable-group-add-latest",
                    Info = "Add latest variable groups.",
                    Config = new ImportConfig { VariableGroupAddLatest = true }.ToApplicationConfig()
                },
            };
        }
    }
}