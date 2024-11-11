namespace AzureDevopsExplorer.Application.Configuration;

public class ImportConfig
{
    public List<int> PipelineIds { get; set; } = new();
    public bool AccessControlListImport { get; set; } = false;
    public bool AgentPoolImport { get; set; } = false;
    public bool AuditLogImport { get; set; } = false;
    public DateTime? AuditLogStartDate { get; set; } = null;
    public bool BuildsAddFromStart { get; set; } = false;
    public bool BuildsAddLatestDefaultFromPipeline { get; set; } = false;
    public bool BuildAddPipelineRun { get; set; } = false;
    public bool BuildAddArtifacts { get; set; } = false;
    public bool BuildAddTimeline { get; set; } = false;
    public bool BuildRunYamlAnalysis { get; set; } = false;
    public bool CheckConfigurationImport { get; set; } = false;
    public bool CodeSearchImport { get; set; } = false;
    public bool GraphAddEntraApplications { get; set; } = false;
    public bool GraphAddEntraGroups { get; set; } = false;
    public bool GraphAddGroups { get; set; } = false;
    public bool GraphAddGroupMemberships { get; set; } = false;
    public bool GraphAddServicePrincipals { get; set; } = false;
    public bool GraphAddUsers { get; set; } = false;
    public bool GitAddRepositories { get; set; } = false;
    public bool GitAddRepositoriesDefaultBranchCommits { get; set; } = false;
    public bool GitAddPullRequests { get; set; } = false;
    public bool IdentityImport { get; set; } = false;
    public bool PipelineEnvironmentImport { get; set; } = false;
    public bool PipelinePermissionsImport { get; set; } = false;
    public bool PipelineCurrentImport { get; set; } = false;
    public bool PipelineRunImport { get; set; } = false;
    public bool PipelineRunTemplateImport { get; set; } = false;
    public bool PolicyConfigurationImport { get; set; } = false;
    public bool ServiceEndpointAddLatest { get; set; } = false;
    public bool SecureFileImport { get; set; } = false;
    public bool SecurityNamespaceImport { get; set; } = false;
    public bool ServiceEndpointAddHistory { get; set; } = false;
    public bool VariableGroupAddLatest { get; set; } = false;

    public ApplicationConfig ToApplicationConfig()
    {
        return new ApplicationConfig
        {
            ImportConfig = this,
        };
    }

    public ImportConfig Combine(ImportConfig other)
    {
        var configProperties = typeof(ImportConfig).GetProperties();
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
