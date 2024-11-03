using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Application.Configuration;

public class SqlConfig
{
    public DatabaseType? DatabaseType { get; set; }
    public string? ConnectionString { get; set; }
}
