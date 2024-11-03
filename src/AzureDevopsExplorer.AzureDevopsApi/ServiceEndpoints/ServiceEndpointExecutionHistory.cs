namespace AzureDevopsExplorer.AzureDevopsApi.ServiceEndpoints;

public class ServiceEndpointExecutionHistory
{
    public string endpointId { get; set; }
    public Data data { get; set; }
}

public class Data
{
    public int id { get; set; }
    public string planType { get; set; }
    public Definition? definition { get; set; }
    public Owner? owner { get; set; }
    public DateTime startTime { get; set; }
    public DateTime? finishTime { get; set; }
    public string? result { get; set; }
    public string ownerDetails { get; set; }
}

public class Definition
{
    //public Dictionary<string, Link>? _links { get; set; }
    public int id { get; set; }
    public string name { get; set; }
}
public class Owner
{
    //public Dictionary<string, Link>? _links { get; set; }
    public int id { get; set; }
    public string name { get; set; }
}
