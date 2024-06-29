namespace AzureDevopsExplorer.Application.Configuration;

public class DataAction
{
    public string Command { get; set; }
    public string Info { get; set; }
    public ApplicationConfig Config { get; set; }
}

public class ProcessAction
{
    public string Command { get; set; }
    public string Info { get; set; }
    public ApplicationConfig Config { get; set; }
}

public class ApplicationActions
{
    public static List<ProcessAction> GetAllProcessActions
    {
        get
        {
            return new List<ProcessAction>
            {
                new ProcessAction
                {
                    Command = "update-pipeline-and-run",
                    Info = "Update latest pipeline and run tables.",
                    Config = new ProcessConfig { UpdateLatestPipelineAndRun = true }.ToApplicationConfig()
                },

                new ProcessAction
                {
                    Command = "derive-resource-permissions",
                    Info = "Derive resource permissions from ACL tables.",
                    Config = new ProcessConfig { DerivePermissions = true }.ToApplicationConfig()
                },

            };
        }
    }

    public static List<DataAction> GetAllDataActions
    {
        get
        {
            return new List<DataAction>
            {
                new DataAction
                {
                    Command = "all-build-and-pipeline",
                    Info = "Add all build and pipeline entities",
                    Config = new DataConfig
                    {
                        BuildAddArtifacts = true,
                        BuildAddPipelineRun = true,
                        BuildAddTimeline = true,
                        BuildRunYamlAnalysis = true,
                        PipelineRunImport = true,
                        PipelineRunTemplateImport = true,
                    }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "acl-import",
                    Info = "Import access control lists.",
                    Config = new DataConfig { AccessControlListImport = true, SecurityNamespaceImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "agent-pool-import",
                    Info = "Import agent pools.",
                    Config = new DataConfig { AgentPoolImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "builds-add-from-start",
                    Info = "Add all builds in order of queued from start.",
                    Config = new DataConfig { BuildsAddFromStart = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "builds-add-latest-default",
                    Info = "Add latest build from each pipeline.",
                    Config = new DataConfig { BuildsAddLatestDefaultFromPipeline = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "build-add-artifacts",
                    Info = "Add build artifacts for builds.",
                    Config = new DataConfig { BuildAddArtifacts = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "build-add-timeline",
                    Info = "Add build timeline for builds.",
                    Config = new DataConfig { BuildAddTimeline = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "build-add-pipeline-run",
                    Info = "Add pipeline run for builds.",
                    Config = new DataConfig { BuildAddPipelineRun = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "build-run-yaml-analysis",
                    Info = "Add analysis of expanded yaml file to get the used service connections and variable groups.",
                    Config = new DataConfig { BuildRunYamlAnalysis = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "code-search-import",
                    Info = "Add code search keywords.",
                    Config = new DataConfig { CodeSearchImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "check-configuration-import",
                    Info = "Add check configurations.",
                    Config = new DataConfig { CheckConfigurationImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "git-add-repositories",
                    Info = "Add git repositories.",
                    Config = new DataConfig { GitAddRepositories = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "git-add-pull-requests",
                    Info = "Add git pull requests.",
                    Config = new DataConfig { GitAddPullRequests = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "graph-add-applications",
                    Info = "Add Entra applications from graph API.",
                    Config = new DataConfig { GraphAddApplications = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "graph-add-groups",
                    Info = "Add Entra groups from graph API.",
                    Config = new DataConfig { GraphAddGroups = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "identity-import",
                    Info = "Add identity.",
                    Config = new DataConfig { IdentityImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "pipeline-environment-import",
                    Info = "Add pipeline environments information.",
                    Config = new DataConfig { PipelineEnvironmentImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "pipeline-permissions-import",
                    Info = "Add pipeline approval permissions information.",
                    Config = new DataConfig { PipelinePermissionsImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "pipeline-run-import",
                    Info = "Add pipeline run information.",
                    Config = new DataConfig { PipelineRunImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "pipeline-run-template-import",
                    Info = "Add pipeline run template information.",
                    Config = new DataConfig { PipelineRunTemplateImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "service-endpoint-add-latest",
                    Info = "Add latest values for service endpoints.",
                    Config = new DataConfig { ServiceEndpointAddLatest = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "secure-file-import",
                    Info = "Add latest values for secure files.",
                    Config = new DataConfig { SecureFileImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "security-namespace-import",
                    Info = "Add security namespaces and analyse the permissions.",
                    Config = new DataConfig { SecurityNamespaceImport = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "service-endpoint-add-history",
                    Info = "Add history for service endpoints.",
                    Config = new DataConfig { ServiceEndpointAddHistory = true }.ToApplicationConfig()
                },
                new DataAction
                {
                    Command = "variable-group-add-latest",
                    Info = "Add latest variable groups.",
                    Config = new DataConfig { VariableGroupAddLatest = true }.ToApplicationConfig()
                },

            };
        }
    }
}