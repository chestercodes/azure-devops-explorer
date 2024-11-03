namespace AzureDevopsExplorer.AzureDevopsApi.Pipelines;

public class PipelineYaml
{
    public Dictionary<string, Link>? _links { get; set; }
    public PipelineYamlConfiguration? configuration { get; set; }
    public string? url { get; set; }
    public int id { get; set; }
    public int? revision { get; set; }
    public string? name { get; set; }
    public string? folder { get; set; }
}

public class PipelineYamlConfiguration
{
    public string? path { get; set; }
    public Repository? repository { get; set; }
    public string? type { get; set; }
    public Dictionary<string, PipelineYamlVariable>? variables { get; set; }
}

public class PipelineYamlVariable
{
    public string? value { get; set; }
    public bool? isSecret { get; set; }
}
