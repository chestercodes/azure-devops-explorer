namespace AzureDevopsExplorer.AzureDevopsApi.Pipelines;

public class PipelineRef
{
    //public Dictionary<string, Link> _links { get; set; }
    public string? url { get; set; }
    public int id { get; set; }
    public int revision { get; set; }
    public string name { get; set; }
    public string folder { get; set; }
}

