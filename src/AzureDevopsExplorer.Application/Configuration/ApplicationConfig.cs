namespace AzureDevopsExplorer.Application.Configuration;
public class ApplicationConfig
{
    public DataConfig DataConfig { get; set; } = new DataConfig();

    public bool LoadToNeo4j { get; set; }

    public ApplicationConfig Combine(ApplicationConfig config)
    {
        DataConfig = (DataConfig ?? new DataConfig()).Combine(config.DataConfig);
        LoadToNeo4j = LoadToNeo4j || config.LoadToNeo4j;
        return this;
    }
}
