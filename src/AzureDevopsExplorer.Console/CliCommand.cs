using AzureDevopsExplorer.Application.Entrypoints.Data;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi;
using Microsoft.VisualStudio.Services.WebApi;
using System.CommandLine;
using AzureDevopsExplorer.AzureDevopsApi.Auth;
using Microsoft.VisualStudio.Services.OAuth;
using AzureDevopsExplorer.Application.Entrypoints.Loader;
using AzureDevopsExplorer.Application.Entrypoints.Evaluate;

public class CliCommand
{
    public RootCommand GetRootCommand()
    {
        var organisationNameOption = new Option<string>(
            name: "--org",
            description: "Azure Devops organisation name")
        {
            IsRequired = false,
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

        var loadToNeo4jOption = new Option<bool>(
            name: "--load-to-neo4j",
            description: "Load entities to neo4j")
        {
            IsRequired = false
        };

        var rootCommand = new RootCommand("Azure devops explorer app");
        rootCommand.AddOption(organisationNameOption);
        rootCommand.AddOption(projectNamesOption);
        rootCommand.AddOption(dataActionsOption);
        rootCommand.AddOption(pipelineIdsOption);
        rootCommand.AddOption(loadToNeo4jOption);

        rootCommand.SetHandler(CommandHandler, organisationNameOption, projectNamesOption, dataActionsOption, pipelineIdsOption, loadToNeo4jOption);
        return rootCommand;
    }

    private async Task CommandHandler(string organisationName, string[] projectNames, string[] dataActions, int[] pipelineIds, bool loadToNeo4J)
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
            var projNames = await GetProjectNames(httpClientOrg, projectNames);
            foreach (var projName in projNames)
            {
                var runImport = new RunImport(httpClientOrg, connection, projName);
                await runImport.Run(config.DataConfig);
            }
        }

        var runEvaluate = new RunEvaluate();
        await runEvaluate.Run();

        if (loadToNeo4J)
        {
            var loadLatest = new LoadLatest();
            await loadLatest.Run();
        }
    }

    private static async Task<string> GetBearerToken()
    {
        var tokenProvider = new AzureDevopsAccessTokenProvider();
        var accessToken = await tokenProvider.GetAccessToken(new CancellationToken());
        var bearerToken = accessToken.Token;
        return bearerToken;
    }

    private static async Task<List<string>> GetProjectNames(AzureDevopsApiOrgClient httpClientOrg, string[] projectNames)
    {
        if (projectNames.Length != 0)
        {
            return projectNames.ToList();
        }

        var orgQueries = new AzureDevopsApiOrgQueries(httpClientOrg);
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