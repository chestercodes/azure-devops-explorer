namespace AzureDevopsExplorer.AzureDevopsApi.ApprovalsAndChecks;

public class PipelineResourceApproval
{
    public PipelineResourceApprovalAllPipelines? allPipelines { get; set; }
    public PipelineResourceApprovalResource? resource { get; set; }
    public PipelineResourceApprovalPipeline[]? pipelines { get; set; }
}

public class PipelineResourceApprovalResource
{
    public string type { get; set; }
    public string id { get; set; }
}

public class PipelineResourceApprovalPipeline
{
    public int id { get; set; }
    public bool authorized { get; set; }
    public PipelineResourceApprovalAuthorizedby authorizedBy { get; set; }
    public DateTime authorizedOn { get; set; }
}

public class PipelineResourceApprovalAuthorizedby
{
    public string displayName { get; set; }
    public string id { get; set; }
    public string uniqueName { get; set; }
    public string descriptor { get; set; }
}

public class PipelineResourceApprovalAllPipelines
{
    public bool authorized { get; set; }
    public PipelineResourceApprovalAuthorizedby authorizedBy { get; set; }
    public DateTime authorizedOn { get; set; }
}
