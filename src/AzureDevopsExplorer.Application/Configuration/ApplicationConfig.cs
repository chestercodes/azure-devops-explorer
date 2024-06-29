namespace AzureDevopsExplorer.Application.Configuration;

public class ApplicationConfig
{
    public DataConfig DataConfig { get; set; } = new DataConfig();
    public ProcessConfig ProcessConfig { get; set; } = new ProcessConfig();

    public bool LoadToNeo4j { get; set; }

    public ApplicationConfig Combine(ApplicationConfig config)
    {
        DataConfig = (DataConfig ?? new DataConfig()).Combine(config.DataConfig);
        ProcessConfig = (ProcessConfig ?? new ProcessConfig()).Combine(config.ProcessConfig);
        LoadToNeo4j = LoadToNeo4j || config.LoadToNeo4j;
        return this;
    }
}
