using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Configuration.Actions;

public class ApplicationConfigParser
{
    public ApplicationConfig? Parse(
        ImportParameters import,
        ProcessParameters process,
        AzureDevopsParameters azureDevops,
        SqlDatabaseParameters sql,
        Neo4jParameters neo4j)
    {
        var config = new ApplicationConfig();

        if (import.AzdoImportActions != null)
        {
            var parsedConfig = ParseImportActions(import.AzdoImportActions);
            if (parsedConfig == null)
            {
                // could not find import action, do not continue
                return null;
            }
            config = config.Combine(parsedConfig);
        }

        if (process.ProcessActions != null)
        {
            var parsedConfig = ParseProcessActions(process.ProcessActions);
            if (parsedConfig == null)
            {
                // could not find process action, do not continue
                return null;
            }
            config = config.Combine(parsedConfig);
        }

        if (import.AuditLogStart != null)
        {
            config.ImportConfig.AuditLogStartDate = import.AuditLogStart;
        }

        if (import.PipelineIds != null)
        {
            config.ImportConfig.PipelineIds = import.PipelineIds.ToList();
        }

        if (azureDevops.Organisation != null)
        {
            config.AzureDevopsConfig = new AzureDevopsConfig { Organisation = azureDevops.Organisation };

            if (azureDevops.Projects != null)
            {
                config.AzureDevopsConfig.Projects = azureDevops.Projects.ToList();
            }

            if (azureDevops.Pat != null)
            {
                config.AzureDevopsConfig.Pat = azureDevops.Pat;
            }
        }

        if (sql.SqlConnectionString != null)
        {
            config.SqlConfig = new SqlConfig
            {
                ConnectionString = sql.SqlConnectionString
            };
        }

        if (neo4j.LoadToNeo4j != null)
        {
            config.Neo4jConfig = new Neo4jConfig
            {
                LoadData = neo4j.LoadToNeo4j.HasValue && neo4j.LoadToNeo4j.Value,
                Url = neo4j.Neo4jUrl
            };
        }

        return config;
    }

    private ApplicationConfig? ParseImportActions(string[] importActions)
    {
        var importCommands = ImportActions.All;
        var importCommandNames = importCommands.Select(x => x.Command).ToList();
        var notRecognisedDataActions = importActions.Where(x => importCommandNames.Contains(x) == false).ToList();
        if (notRecognisedDataActions.Count > 0)
        {
            Console.WriteLine("Import action not recognised. Available ones are:");
            Console.WriteLine(ImportInfo);
            return null;
        }

        var initialConfig = new ApplicationConfig();

        var withDataActions = importActions.Select(x => importCommands.Single(y => y.Command == x))
            .Aggregate(initialConfig, (agg, el) => { return agg.Combine(el.Config); });

        return withDataActions;
    }

    private ApplicationConfig? ParseProcessActions(string[] processActions)
    {
        var processCommands = ProcessActions.All;
        var processCommandNames = processCommands.Select(x => x.Command).ToList();
        var notRecognisedProcessActions = processActions.Where(x => processCommandNames.Contains(x) == false).ToList();
        if (notRecognisedProcessActions.Count > 0)
        {
            Console.WriteLine("Process action not recognised. Available ones are:");
            Console.WriteLine(ProcessInfo);
            return null;
        }

        var initialConfig = new ApplicationConfig();

        var withBothActions = processActions.Select(x => processCommands.Single(y => y.Command == x))
            .Aggregate(initialConfig, (agg, el) => { return agg.Combine(el.Config); });

        return withBothActions;
    }

    public static string ImportInfo => string.Join(Environment.NewLine, ImportActions.All.Select(x => $"\t{x.Command} - {x.Info}"));
    public static string ProcessInfo => string.Join(Environment.NewLine, ProcessActions.All.Select(x => $"\t{x.Command} - {x.Info}"));
}