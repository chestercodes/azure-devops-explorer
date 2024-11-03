namespace AzureDevopsExplorer.AzureDevopsApi.Environments;

public class PipelineEnvironment
{
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public PipelineEnvironmentIdentity createdBy { get; set; }
    public DateTime createdOn { get; set; }
    public PipelineEnvironmentIdentity lastModifiedBy { get; set; }
    public DateTime lastModifiedOn { get; set; }
    public PipelineEnvironmentProject project { get; set; }
}

public class PipelineEnvironmentIdentity
{
    public string displayName { get; set; }
    public string id { get; set; }
}

public class PipelineEnvironmentProject
{
    public string id { get; set; }
}
