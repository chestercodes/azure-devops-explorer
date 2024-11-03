namespace AzureDevopsExplorer.AzureDevopsApi.DistributedTask;

public class AgentPool
{
    public DateTime createdOn { get; set; }
    public bool autoProvision { get; set; }
    public bool autoUpdate { get; set; }
    public bool autoSize { get; set; }
    public int? targetSize { get; set; }
    public int? agentCloudId { get; set; }
    public AgentPoolCreatedby? createdBy { get; set; }
    public AgentPoolOwner? owner { get; set; }
    public int id { get; set; }
    public string scope { get; set; }
    public string name { get; set; }
    public bool isHosted { get; set; }
    public string poolType { get; set; }
    public int size { get; set; }
    public bool isLegacy { get; set; }
    public string options { get; set; }
}

public class AgentPoolCreatedby
{
    public string id { get; set; }
}

public class AgentPoolOwner
{
    public string id { get; set; }
}

