using Cocona;

public record ImportParameters(
    [Option('i', Description = "Specifies the azure devops import actions to run.")]
    string[]? AzdoImportActions,
    [Option(Description = "Optionally specifies the pipeline ids to run actions for.")]
    int[]? PipelineIds,
    [Option(Description = "If running the audit log import, start initial loading from this date, will default to a year ago.")]
    DateTime? AuditLogStart
) : ICommandParameterSet;

public record ProcessParameters(
    [Option('p', Description = "Specifies the process actions to run.")]
    string[]? ProcessActions
) : ICommandParameterSet;

public record AzureDevopsParameters(
    [Option(Description = "Specifies the azure devops organisation to import data from.")]
    string? Organisation,
    [Option(Description = "Optionally specify project(s) to import data from. Defaults to all available if non specified.")]
    string[]? Projects,
    [Option(Description = "Optionally specify PAT to use for authentication, default to trying to use DefaultAzureCredential to get a token.")]
    string? Pat
) : ICommandParameterSet;

public record SqlDatabaseParameters(
    [Option(Description = "Optionally specify the database connection string, defaults to one for localdb.")]
    string? SqlConnectionString,
    [Option(Description = "Optionally specify the type of database, either sqlserver or sqlite, defaults to sqlserver.")]
    string? SqlDatabaseType
) : ICommandParameterSet;

public record Neo4jParameters(
    [Option(Description = "Load to Neo4j")]
    bool? LoadToNeo4j,
    [Option(Description = "Neo4j Url, required if loading to Neo4j.")]
    string? Neo4jUrl,
    [Option(Description = "Neo4j username, required if loading to Neo4j.")]
    string? Neo4jUsername,
    [Option(Description = "Neo4j password, required if loading to Neo4j.")]
    string? Neo4jPassword
) : ICommandParameterSet;
