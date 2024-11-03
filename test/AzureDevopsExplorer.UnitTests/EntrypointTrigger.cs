using AzureDevopsExplorer.Application;
using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.Application.Entrypoints.Import;
using AzureDevopsExplorer.Application.Entrypoints.Evaluate;
using AzureDevopsExplorer.Application.Entrypoints.Loader;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Neo4j;
using Microsoft.Extensions.Logging;
using AzureDevopsExplorer.Application.Entrypoints.Import.Historical;

namespace AzureDevopsExplorer.UnitTests;

public class EntrypointTrigger
{
    public const string OrganisationName = "XXXX";
    public const string ProjectName = "XXXX";
    public static readonly Guid ProjectId = Guid.Parse("XXXX");

    public const string SqlConnectionString = "TODO";


#if DEBUG
    [Fact]
#endif
    public async Task RunSingle()
    {
        Environment.SetEnvironmentVariable("SET_IF_LOCAL_DEV", "");

        var (orgDataContext, projectDataContext) = GetContexts();

        var entrypoint = new AuditLogImport(orgDataContext);
        await entrypoint.AddAuditLogDates(
            new DateTime(2024, 06, 27)
            );

        await entrypoint.AddAuditLog();

    }

    private static (AzureDevopsOrganisationDataContext, AzureDevopsProjectDataContext) GetContexts()
    {
        var loggerFactory = LoggerFactory.Create(cfg => { });
        var ct = new CancellationToken();
        var authHeader = new AzureDevopsApiAuthHeaderRetriever(null, ct);
        var httpClientOrg = new AzureDevopsApiOrganisationClientFactory(OrganisationName, loggerFactory, authHeader, ct);
        var orgQueries = new AzureDevopsApiOrgQueries(httpClientOrg);

        var httpClientProject = new AzureDevopsApiProjectClientFactory(OrganisationName, ProjectName, loggerFactory, authHeader, ct);
        var projectQueries = new AzureDevopsApiProjectQueries(httpClientProject, ProjectName);

        var orgDataContext = new AzureDevopsOrganisationDataContext(
            httpClientOrg,
            orgQueries,
            loggerFactory,
            new DataContextFactory(SqlConnectionString, DatabaseType.Sqlite),
            ct
            );
        var project = new AzureDevopsProject(ProjectName, ProjectId);
        var projectDataContext = new AzureDevopsProjectDataContext(
            httpClientOrg,
            orgQueries,
            project,
            httpClientProject,
            projectQueries,
            loggerFactory,
            new DataContextFactory(SqlConnectionString, DatabaseType.Sqlite),
            ct
            );

        return (orgDataContext, projectDataContext);
    }

#if DEBUG
    [Fact]
#endif
    public async Task RunMultiple()
    {
        Environment.SetEnvironmentVariable("SET_IF_LOCAL_DEV", "");

        var (orgDataContext, projectDataContext) = GetContexts();

        var config = new ImportConfig
        {
            //AccessControlListImport = true,
            //AgentPoolImport = true,
            AuditLogImport = true,
            //BuildAddArtifacts = true,
            //BuildAddTimeline = true,
            //BuildAddPipelineRun = true,
            //BuildRunYamlAnalysis = true,
            //BuildsAddLatestDefaultFromPipeline = true,
            CodeSearchImport = true,
            CheckConfigurationImport = true,
            //GitAddRepositories = true,
            //IdentityImport = true,
            PipelineEnvironmentImport = true,
            PipelinePermissionsImport = true,
            PipelineRunImport = true,
            PipelineRunTemplateImport = true,
            SecureFileImport = true,
            //SecurityNamespaceImport = true,
            //ServiceEndpointAddLatest = true,
            //ServiceEndpointAddHistory = true,
            VariableGroupAddLatest = true,
        };

        var entrypoint = new RunImport();
        await entrypoint.RunOrganisationEntityImport(config, orgDataContext);
        await entrypoint.RunProjectEntityImport(config, projectDataContext);
        //await entrypoint.Run(config);
    }

#if DEBUG
    [Fact]
#endif
    public async Task RunLoadToNeo4j()
    {
        var latestPipelineAndRun = new LatestPipelineAndRun(new DataContextFactory(SqlConnectionString, DatabaseType.Sqlite));
        await latestPipelineAndRun.Run();

        string user = Environment.GetEnvironmentVariable("NEO4J_USERNAME") ?? "neo4j";
        string password = Environment.GetEnvironmentVariable("NEO4J_PASSWORD") ?? "somepassword";
        string uri = Environment.GetEnvironmentVariable("NEO4J_URL") ?? "bolt://localhost:7687";
        var neo4j = new Neo4jDriverFactory(uri, user, password);

        var entrypoint = new LoadLatest(new DataContextFactory(SqlConnectionString, DatabaseType.Sqlite), neo4j, new CancellationToken());
        await entrypoint.Run();
    }

#if DEBUG
    [Fact]
#endif
    public async Task RunPermissionsCalculation()
    {
        var entrypoint = new DeriveResourcePermissions();
        await entrypoint.Run();
    }

#if DEBUG
    [Fact]
#endif
    public async Task RunQuery()
    {
        Environment.SetEnvironmentVariable("SET_IF_LOCAL_DEV", "");

        var (orgDataContext, projectDataContext) = GetContexts();

        var aMonthAgo = DateTime.Now.AddMonths(-1);
        var builds = await projectDataContext.Queries.Build.GetBuildsFrom(aMonthAgo);
    }
}