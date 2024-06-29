using AzureDevopsExplorer.Application.Entrypoints.Data;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi;
using Microsoft.VisualStudio.Services.WebApi;
using System.CommandLine;
using AzureDevopsExplorer.AzureDevopsApi.Auth;
using Microsoft.VisualStudio.Services.OAuth;
using AzureDevopsExplorer.Application.Entrypoints.Loader;
using AzureDevopsExplorer.Application.Entrypoints.Evaluate;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.Application;
using Microsoft.Extensions.Logging;

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

        var processActionsOption = new Option<string[]>(
            name: "--process-actions",
            description: "Actions relating to data processing and reporting that you want to run in this console app call.")
        {
            IsRequired = false
        };

        var auditStartDateOption = new Option<string>(
            name: "--audit-log-start-date",
            description: "If downloading audit log data this optional parameter sets the start date of downloading, defaults to a year before run.")
        {
            IsRequired = false,
        };

        var rootCommand = new RootCommand("Azure devops explorer app");
        rootCommand.AddOption(organisationNameOption);
        rootCommand.AddOption(projectNamesOption);
        rootCommand.AddOption(dataActionsOption);
        rootCommand.AddOption(pipelineIdsOption);
        rootCommand.AddOption(loadToNeo4jOption);
        rootCommand.AddOption(processActionsOption);
        rootCommand.AddOption(auditStartDateOption);

        rootCommand.SetHandler(CommandHandler, organisationNameOption, projectNamesOption, dataActionsOption, pipelineIdsOption, loadToNeo4jOption, processActionsOption, auditStartDateOption);
        return rootCommand;
    }

    private async Task CommandHandler(string organisationName, string[] projectNames, string[] dataActions, int[] pipelineIds, bool loadToNeo4J, string[] processActions, string auditStartDate)
    {
        var configParser = new ApplicationConfigParser();
        var config = configParser.Parse(dataActions, pipelineIds, processActions);
        if (config == null)
        {
            // could not find data action, do not continue
            return;
        }

        if (auditStartDate != null)
        {
            config.DataConfig.AuditLogStartDate = auditStartDate;
        }

        if (config.DataConfig.AnyAzureDevopsDownloadingNeeded)
        {
            var bearerToken = await GetBearerToken();
            Lazy<VssConnection> lazyConnection = new Lazy<VssConnection>(
                () => new VssConnection(
                    new Uri($"https://dev.azure.com/{organisationName}"),
                    new VssOAuthAccessTokenCredential(bearerToken))
                );

            var azureDevopsApiOrgInfo = new AzureDevopsApiOrgInfo
            {
                BearerToken = bearerToken,
                OrgName = organisationName,
            };

            using ILoggerFactory factory = LoggerFactory.Create(cfg => { });
            ILogger logger = factory.CreateLogger("AzureDevopsExplorer");
            Func<ILogger> getLogger = () => logger;

            var httpClientOrg = new AzureDevopsApiOrgClient(azureDevopsApiOrgInfo, getLogger);
            var orgDataContext = new AzureDevopsOrganisationDataContext(
                lazyConnection,
                new Lazy<AzureDevopsApiOrgClient>(httpClientOrg),
                new Lazy<AzureDevopsApiOrgQueries>(new AzureDevopsApiOrgQueries(httpClientOrg)),
                getLogger);

            var runImport = new RunImport();
            await runImport.RunOrganisationEntityImport(config.DataConfig, orgDataContext);

            var projects = await GetProjects(httpClientOrg, projectNames);
            foreach (var proj in projects)
            {
                var httpClientProject = new AzureDevopsApiProjectClient(azureDevopsApiOrgInfo.AsProjectInfo(proj.ProjectName, proj.ProjectId), getLogger);
                var projectDataContext = new AzureDevopsProjectDataContext(
                    proj,
                    lazyConnection,
                    new Lazy<AzureDevopsApiProjectClient>(httpClientProject),
                    new Lazy<AzureDevopsApiProjectQueries>(new AzureDevopsApiProjectQueries(httpClientProject)),
                    getLogger);


                await runImport.RunProjectEntityImport(config.DataConfig, projectDataContext);
            }

            var runGraphImport = new RunGraphImport();
            await runGraphImport.Run(config.DataConfig);
        }

        var runEvaluate = new RunEvaluate();
        await runEvaluate.Run(config.ProcessConfig);

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

    private static async Task<List<AzureDevopsProject>> GetProjects(AzureDevopsApiOrgClient httpClientOrg, string[] projectNames)
    {
        var orgQueries = new AzureDevopsApiOrgQueries(httpClientOrg);
        var projectsResult = await orgQueries.GetProjects();

        if (projectsResult.IsT1)
        {
            var err = projectsResult.AsT1;
            Console.WriteLine(err.AsError);
            throw err.AsT1;
        }

        var projects = projectsResult.AsT0.Value;


        if (projectNames.Length == 0)
        {
            return projects.Select(x => new AzureDevopsProject(x.name, Guid.Parse(x.id))).ToList();
        }

        var toReturn = new List<AzureDevopsProject>();
        foreach (var projectName in projectNames)
        {
            var fromApiOrNull = projects.SingleOrDefault(x => x.id.ToString() == projectName || x.name == projectName);
            if (fromApiOrNull == null)
            {
                Console.WriteLine($"Cannot find project with name or id with value '{projectName}'");
                return new List<AzureDevopsProject>();
            }

            toReturn.Add(new AzureDevopsProject(fromApiOrNull.name, Guid.Parse(fromApiOrNull.id)));
        }
        return toReturn;
    }
}