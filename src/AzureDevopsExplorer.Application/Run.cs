using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Entrypoints.Import;
using AzureDevopsExplorer.Application.Entrypoints.Evaluate;
using AzureDevopsExplorer.Application.Entrypoints.Loader;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi;
using Microsoft.Extensions.Logging;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Neo4j;

namespace AzureDevopsExplorer.Application;

public class Run
{
    public async Task WithApplicationConfig(ApplicationConfig config, CancellationToken cancellationToken)
    {
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(cfg =>
            {
                cfg.AddSimpleConsole(x =>
                {
                    x.SingleLine = true;
                    x.UseUtcTimestamp = true;
                    x.TimestampFormat = "HH:mm:ss ";
                }
                );

                if (config.LoggingConfig != null)
                {
                    if (config.LoggingConfig.Verbose)
                    {
                        cfg.SetMinimumLevel(LogLevel.Debug);
                    }
                }
            });

        var dataContextFactory = new DataContextFactory(config.SqlConfig.ConnectionString, config.SqlConfig.DatabaseType.Value);

        if (config.SqlConfig.DatabaseType == DatabaseType.Sqlite)
        {
            using (var db = dataContextFactory.Create())
            {
                db.Database.EnsureCreated();
            }
        }

        if (config?.AzureDevopsConfig?.Organisation != null)
        {
            var pat = config?.AzureDevopsConfig?.Pat.LiftToPat();

            var orgName = config.AzureDevopsConfig.Organisation;
            var authHeader = new AzureDevopsApiAuthHeaderRetriever(pat, cancellationToken);
            var orgClientFactory = new AzureDevopsApiOrganisationClientFactory(orgName, loggerFactory, authHeader, cancellationToken);
            var orgQueries = new AzureDevopsApiOrgQueries(orgClientFactory);
            var orgDataContext = new AzureDevopsOrganisationDataContext(
                orgClientFactory,
                orgQueries,
                loggerFactory,
                dataContextFactory,
                cancellationToken);

            var runImport = new RunImport();
            await runImport.RunOrganisationEntityImport(config.ImportConfig, orgDataContext);

            var projects = await GetProjects(orgDataContext, config.AzureDevopsConfig.Projects ?? new List<string>());
            foreach (var proj in projects)
            {
                var httpClientProjectFactory = new AzureDevopsApiProjectClientFactory(orgName, proj.ProjectName, loggerFactory, authHeader, cancellationToken);
                var projectQueries = new AzureDevopsApiProjectQueries(httpClientProjectFactory, proj.ProjectName);
                var projectDataContext = new AzureDevopsProjectDataContext(
                    orgClientFactory,
                    orgQueries,
                    proj,
                    httpClientProjectFactory,
                    projectQueries,
                    loggerFactory,
                    dataContextFactory,
                    cancellationToken);


                await runImport.RunProjectEntityImport(config.ImportConfig, projectDataContext);
            }
        }

        var runGraphImport = new RunGraphImport(loggerFactory, dataContextFactory, cancellationToken);
        await runGraphImport.Run(config.ImportConfig);

        var runEvaluate = new RunEvaluate(dataContextFactory, cancellationToken);
        await runEvaluate.Run(config.ProcessConfig);

        if (config?.Neo4jConfig?.LoadData == true)
        {
            string user = config?.Neo4jConfig?.Username ?? Environment.GetEnvironmentVariable("NEO4J_USERNAME") ?? throw new ApplicationException("Can not get value for neo4j username");
            string password = config?.Neo4jConfig?.Password ?? Environment.GetEnvironmentVariable("NEO4J_PASSWORD") ?? throw new ApplicationException("Can not get value for neo4j password");
            string uri = config?.Neo4jConfig?.Url ?? Environment.GetEnvironmentVariable("NEO4J_URL") ?? throw new ApplicationException("Can not get value for neo4j url");
            var neo4j = new Neo4jDriverFactory(uri, user, password);

            var loadLatest = new LoadLatest(dataContextFactory, neo4j, cancellationToken);
            await loadLatest.Run();
        }
    }

    private static async Task<List<AzureDevopsProject>> GetProjects(AzureDevopsOrganisationDataContext dataContext, List<string> projectNames)
    {
        var projectsResult = await dataContext.Queries.Core.GetProjects();

        if (projectsResult.IsT1)
        {
            var err = projectsResult.AsT1;
            Console.WriteLine(err.AsError);
            throw err.Exception;
        }

        var projects = projectsResult.AsT0.value;


        if (projectNames.Count == 0)
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
