using AzureDevopsExplorer.Application.Entrypoints.Data;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi;
using Microsoft.VisualStudio.Services.WebApi;
using System.CommandLine;
using AzureDevopsExplorer.AzureDevopsApi.Auth;
using Microsoft.VisualStudio.Services.OAuth;

public class CliCommand
{
    public RootCommand GetRootCommand()
    {
        var organisationNameOption = new Option<string>(
            name: "--org",
            description: "Azure Devops organisation name")
        {
            IsRequired = true,
        };

        var projectNamesOption = new Option<string[]>(
            name: "--project-names",
            description: "Azure Devops project names")
        {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true,
        };

        var dataActionsOption = new Option<string[]>(
            name: "--data-actions",
            description: "Actions relating to data download that you want to run in this console app call.")
        {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true,
        };

        var pipelineIdsOption = new Option<int[]>(
            aliases: ["--pipeline-ids", "--definition-ids"],
            description: "Run data actions for specific pipeline ids (definition ids).")
        {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true
        };

        var rootCommand = new RootCommand("Azure devops explorer app");
        rootCommand.AddOption(organisationNameOption);
        rootCommand.AddOption(projectNamesOption);
        rootCommand.AddOption(dataActionsOption);
        rootCommand.AddOption(pipelineIdsOption);

        rootCommand.SetHandler(CommandHandler, organisationNameOption, projectNamesOption, dataActionsOption, pipelineIdsOption);
        return rootCommand;
    }

    private async Task CommandHandler(string organisationName, string[] projectNames, string[] dataActions, int[] pipelineIds)
    {
        var configParser = new ApplicationConfigParser();
        var config = configParser.Parse(dataActions, pipelineIds);
        if (config == null)
        {
            return;
        }

        if (config.DataConfig.AnyAzureDevopsDownloadingNeeded)
        {
            var bearerToken = await GetBearerToken();
            var connection = new VssConnection(
                new Uri($"https://dev.azure.com/{organisationName}"),
                new VssOAuthAccessTokenCredential(bearerToken));

            var azureDevopsApiOrgInfo = new AzureDevopsApiOrgInfo
            {
                BearerToken = bearerToken,
                OrgName = organisationName,
            };
            var httpClientOrg = new AzureDevopsApiOrgClient(azureDevopsApiOrgInfo);
            var orgQueries = new AzureDevopsApiOrgQueries(httpClientOrg);

            var projNames = await GetProjectNames(orgQueries, projectNames);
            foreach (var projName in projNames)
            {
                var azureDevopsApiInfo = azureDevopsApiOrgInfo.AsProjectInfo(projName);
                var httpClient = new AzureDevopsApiProjectClient(azureDevopsApiInfo);
                var runImport = new RunImport(httpClient, connection, projName);
                await runImport.Run(config.DataConfig);
            }
        }
    }

    private static async Task<string> GetBearerToken()
    {
        var tokenProvider = new AzureDevopsAccessTokenProvider();
        var accessToken = await tokenProvider.GetAccessToken(new CancellationToken());
        var bearerToken = accessToken.Token;
        return bearerToken;
    }

    private static async Task<List<string>> GetProjectNames(AzureDevopsApiOrgQueries orgQueries, string[] projectNames)
    {
        if (projectNames.Length != 0)
        {
            return projectNames.ToList();
        }

        var projectsResult = await orgQueries.GetProjects();

        return projectsResult.Match(
            projects =>
            {
                return projects.Value.Select(x => x.name).ToList();
            },
            err =>
            {
                Console.WriteLine(err.AsError);
                throw err.AsT1;
            });
    }
}