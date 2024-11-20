namespace AzureDevopsExplorer.Application.Configuration;

public class ProcessConfig
{
    public bool ScanVariables { get; set; } = false;
    public bool ScanVariablesForEntraApplications { get; set; } = false;
    public bool UpdateLatestPipelineAndRun { get; set; } = false;
    public bool DerivePermissions { get; set; } = false;

    public ApplicationConfig ToApplicationConfig()
    {
        return new ApplicationConfig
        {
            ProcessConfig = this,
        };
    }

    public ProcessConfig Combine(ProcessConfig other)
    {
        var configProperties = typeof(ProcessConfig).GetProperties();
        var ignore = new string[] {
            //nameof(AnyAzureDevopsDownloadingNeeded)
        };
        foreach (var configProperty in configProperties)
        {
            if (ignore.Contains(configProperty.Name))
            {
                continue;
            }
            if (configProperty.PropertyType == typeof(bool))
            {
                var thisValue = (bool)configProperty.GetValue(this);
                var otherValue = (bool)configProperty.GetValue(other);
                var newValue = thisValue || otherValue;
                configProperty.SetValue(this, newValue);
            }
        }
        return this;
    }
}
