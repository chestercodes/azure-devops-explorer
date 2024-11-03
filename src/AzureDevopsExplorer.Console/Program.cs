using AzureDevopsExplorer.Application;
using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database;
using Cocona;
using Microsoft.Extensions.DependencyInjection;

var builder = CoconaApp.CreateBuilder();
builder.Services.AddTransient<Run>();

var app = builder.Build();

app.Run(
    async (
        Run run,
        CoconaAppContext ctx,
        ImportParameters import,
        ProcessParameters process,
        AzureDevopsParameters azureDevops,
        SqlDatabaseParameters sqlDatabase,
        Neo4jParameters neo4j
    ) =>
    {
        if (Environment.GetEnvironmentVariable("RUN_STATIC_TEST_CONFIG") != null)
        {
            var organisation = "XXXX";
            var project = "XXXX";

            var testConfig = new ApplicationConfig
            {
                AzureDevopsConfig = new AzureDevopsConfig
                {
                    Organisation = organisation,
                    Projects = new List<string>
                    {
                        project
                    }
                },
                SqlConfig = new SqlConfig
                {
                    DatabaseType = DatabaseType.Sqlite,
                    ConnectionString = DatabaseConnection.DebugSqliteFileConnectionString,
                    //ConnectionString = $"Server=(local)\\SQLExpress;Database=AzureDevopsExplorer;Integrated Security=True;Encrypt=False;",
                },
                ImportConfig = new ImportConfig
                {
                    //AccessControlListImport = true,
                    //AgentPoolImport = true,
                    //AuditLogImport = true,
                    //AuditLogStartDate = DateTime.Now.AddDays(-1),
                    //BuildAddArtifacts = true,
                    //BuildAddPipelineRun = true,
                    //BuildAddTimeline = true,
                    //BuildRunYamlAnalysis = true,
                    //BuildsAddFromStart = true,
                    //BuildsAddLatestDefaultFromPipeline = true,
                    //CheckConfigurationImport = true,
                    //CodeSearchImport = true,
                    //GitAddPullRequests = true,
                    //GitAddRepositories = true,
                    //GraphAddApplications = true,
                    //GraphAddGroups = true,
                    //IdentityImport = true,
                    //PipelineEnvironmentImport = true,
                    //PipelineCurrentImport = true,
                    //PipelineIds = [],
                    //PipelinePermissionsImport = true,
                    //PipelineRunImport = true,
                    //PipelineRunTemplateImport = true,
                    //SecureFileImport = true,
                    //SecurityNamespaceImport = true,
                    //ServiceEndpointAddHistory = true,
                    //ServiceEndpointAddLatest = true,
                    //VariableGroupAddLatest = true,
                },
                ProcessConfig = new ProcessConfig
                {

                }
            };
            await run.WithApplicationConfig(testConfig, ctx.CancellationToken);
            return;
        }

        var configParser = new ApplicationConfigParser();
        var config = configParser.Parse(import, process, azureDevops, sqlDatabase, neo4j);
        if (config == null)
        {
            Console.WriteLine("Cannot continue");
            return;
        }

        await run.WithApplicationConfig(config, ctx.CancellationToken);
    }
);


