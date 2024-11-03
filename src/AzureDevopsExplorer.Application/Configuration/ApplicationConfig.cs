namespace AzureDevopsExplorer.Application.Configuration;

public class ApplicationConfig
{
    public ImportConfig ImportConfig { get; set; } = new ImportConfig();
    public ProcessConfig ProcessConfig { get; set; } = new ProcessConfig();
    public AzureDevopsConfig? AzureDevopsConfig { get; set; }
    public SqlConfig? SqlConfig { get; set; }
    public Neo4jConfig? Neo4jConfig { get; set; }

    public ApplicationConfig Combine(ApplicationConfig config)
    {
        ImportConfig = (ImportConfig ?? new ImportConfig()).Combine(config.ImportConfig);
        ProcessConfig = (ProcessConfig ?? new ProcessConfig()).Combine(config.ProcessConfig);
        return this;
    }
}
