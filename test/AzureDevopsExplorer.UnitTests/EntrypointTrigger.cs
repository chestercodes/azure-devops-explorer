using AzureDevopsExplorer.Application;
using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.Application.Entrypoints.Data;
using AzureDevopsExplorer.Application.Entrypoints.Evaluate;
using AzureDevopsExplorer.Application.Entrypoints.Loader;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Auth;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.UnitTests;

public class EntrypointTrigger
{
    public const string OrganisationName = "xxxx";
    public const string ProjectName = "xxxx";
    public static readonly Guid ProjectId = Guid.Parse("xxxx");

#if DEBUG
    [Fact]
#endif
    public async Task RunSingle()
    {
        Environment.SetEnvironmentVariable("SET_IF_LOCAL_DEV", "");

        var tokenProvider = new AzureDevopsAccessTokenProvider();
        var accessToken = await tokenProvider.GetAccessToken(new CancellationToken());
        var bearerToken = accessToken.Token;
        var connection = new VssConnection(
            new Uri($"https://dev.azure.com/{OrganisationName}"),
            new VssOAuthAccessTokenCredential(bearerToken)
        );

        var azureDevopsApiOrgInfo = new AzureDevopsApiOrgInfo
        {
            BearerToken = bearerToken,
            OrgName = OrganisationName,
        };

        //using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        using ILoggerFactory factory = LoggerFactory.Create(cfg => { });
        ILogger logger = factory.CreateLogger("Program");
        Func<ILogger> getLogger = () => logger;

        var httpClientOrg = new AzureDevopsApiOrgClient(azureDevopsApiOrgInfo, getLogger);
        var orgQueries = new AzureDevopsApiOrgQueries(httpClientOrg);

        var httpClientProject = new AzureDevopsApiProjectClient(azureDevopsApiOrgInfo.AsProjectInfo(ProjectName, ProjectId), getLogger);
        var projectQueries = new AzureDevopsApiProjectQueries(httpClientProject);

        var orgDataContext = new AzureDevopsOrganisationDataContext(
            new Lazy<VssConnection>(connection),
            new Lazy<AzureDevopsApiOrgClient>(httpClientOrg),
            new Lazy<AzureDevopsApiOrgQueries>(orgQueries),
            getLogger
            );

        var project = new AzureDevopsProject(ProjectName, ProjectId);
        var projectDataContext = new AzureDevopsProjectDataContext(
            project,
            new Lazy<VssConnection>(connection),
            new Lazy<AzureDevopsApiProjectClient>(httpClientProject),
            new Lazy<AzureDevopsApiProjectQueries>(projectQueries),
            getLogger
            );

        var entrypoint = new AuditLogImport(orgDataContext);
        await entrypoint.AddAuditLogDates(
            new DateTime(2024, 06, 27)
            );

        await entrypoint.AddAuditLog();

    }

#if DEBUG
    [Fact]
#endif
    public async Task RunMultiple()
    {
        Environment.SetEnvironmentVariable("SET_IF_LOCAL_DEV", "");

        var tokenProvider = new AzureDevopsAccessTokenProvider();
        var accessToken = await tokenProvider.GetAccessToken(new CancellationToken());
        var bearerToken = accessToken.Token;
        var connection = new VssConnection(
            new Uri($"https://dev.azure.com/{OrganisationName}"),
            new VssOAuthAccessTokenCredential(bearerToken)
        );

        var azureDevopsApiOrgInfo = new AzureDevopsApiOrgInfo
        {
            BearerToken = bearerToken,
            OrgName = OrganisationName,
        };

        using ILoggerFactory factory = LoggerFactory.Create(cfg => { });
        ILogger logger = factory.CreateLogger("Program");
        Func<ILogger> getLogger = () => logger;

        var httpClientOrg = new AzureDevopsApiOrgClient(azureDevopsApiOrgInfo, getLogger);
        var orgQueries = new AzureDevopsApiOrgQueries(httpClientOrg);

        var httpClientProject = new AzureDevopsApiProjectClient(azureDevopsApiOrgInfo.AsProjectInfo(ProjectName, ProjectId), getLogger);
        var projectQueries = new AzureDevopsApiProjectQueries(httpClientProject);

        var orgDataContext = new AzureDevopsOrganisationDataContext(
            new Lazy<VssConnection>(connection),
            new Lazy<AzureDevopsApiOrgClient>(httpClientOrg),
            new Lazy<AzureDevopsApiOrgQueries>(orgQueries),
            getLogger
            );

        var project = new AzureDevopsProject(ProjectName, ProjectId);
        var projectDataContext = new AzureDevopsProjectDataContext(
            project,
            new Lazy<VssConnection>(connection),
            new Lazy<AzureDevopsApiProjectClient>(httpClientProject),
            new Lazy<AzureDevopsApiProjectQueries>(projectQueries),
            getLogger
            );

        var config = new DataConfig
        {
            AccessControlListImport = true,
            BuildAddArtifacts = true,
            BuildAddPipelineRun = true,
            BuildRunYamlAnalysis = true,
            BuildsAddLatestDefaultFromPipeline = true,
            CheckConfigurationImport = true,
            GitAddRepositories = true,
            IdentityImport = true,
            PipelineEnvironmentImport = true,
            PipelineRunImport = true,
            PipelineRunTemplateImport = true,
            SecureFileImport = true,
            SecurityNamespaceImport = true,
            ServiceEndpointAddLatest = true,
            VariableGroupAddLatest = true,
        };

        var entrypoint = new RunImport();
        //await entrypoint.Run(config);
    }

#if DEBUG
    [Fact]
#endif
    public async Task RunLoadToNeo4j()
    {
        var latestPipelineAndRun = new LatestPipelineAndRun();
        await latestPipelineAndRun.Run();

        var entrypoint = new LoadLatest();
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
}