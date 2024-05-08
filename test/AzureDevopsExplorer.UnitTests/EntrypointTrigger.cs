using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Entrypoints.Data;
using AzureDevopsExplorer.Application.Entrypoints.Evaluate;
using AzureDevopsExplorer.Application.Entrypoints.Loader;
using AzureDevopsExplorer.AzureDevopsApi.Auth;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.UnitTests;

public class EntrypointTrigger
{
    public const string OrganisationName = "xxxx";
    public const string ProjectName = "xxxx";

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
        var httpClientOrg = new AzureDevopsApiOrgClient(azureDevopsApiOrgInfo);
        var httpClientProject = new AzureDevopsApiProjectClient(azureDevopsApiOrgInfo.AsProjectInfo(ProjectName));

        var entrypoint = new SecurityNamespacesImport(httpClientOrg);
        await entrypoint.ImportSecurityNamespaces();

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
        var httpClientOrg = new AzureDevopsApiOrgClient(azureDevopsApiOrgInfo);
        var httpClientProject = new AzureDevopsApiProjectClient(azureDevopsApiOrgInfo.AsProjectInfo(ProjectName));

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

        var entrypoint = new RunImport(httpClientOrg, connection, ProjectName);
        await entrypoint.Run(config);
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