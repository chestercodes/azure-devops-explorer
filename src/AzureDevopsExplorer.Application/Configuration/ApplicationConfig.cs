namespace AzureDevopsExplorer.Application.Configuration;
public class ApplicationConfig
{
    public DataConfig DataConfig { get; set; } = new DataConfig();

    public ApplicationConfig Combine(ApplicationConfig config)
    {
        DataConfig = (DataConfig ?? new DataConfig()).Combine(config.DataConfig);
        return this;
    }
}
