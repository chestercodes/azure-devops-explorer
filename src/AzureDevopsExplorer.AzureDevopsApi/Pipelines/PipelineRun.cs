using System.Runtime.Serialization;

namespace AzureDevopsExplorer.AzureDevopsApi.Pipelines;

public class PipelineRun
{
    public Dictionary<string, Link> _links { get; set; }
    public Dictionary<string, string>? templateParameters { get; set; }
    public PipelineInfo pipeline { get; set; }
    public string state { get; set; }
    public string? result { get; set; }
    public DateTime createdDate { get; set; }
    public DateTime? finishedDate { get; set; }
    public string? url { get; set; }
    public Resources? resources { get; set; }
    public int id { get; set; }
    public string name { get; set; }
}

public class Link
{
    public string href { get; set; }
}

public class Resources
{
    public Dictionary<string, RepositoryInfo>? repositories { get; set; }
    public Dictionary<string, PipelineObj>? pipelines { get; set; }
}

public class RepositoryInfo
{
    public Repository repository { get; set; }
    public string refName { get; set; }
    public string version { get; set; }
}

public class Repository
{
    public string id { get; set; }
    public string type { get; set; }
}

public class PipelineObj
{
    public PipelineInfo pipeline { get; set; }
    public string version { get; set; }
}

public class PipelineInfo
{
    public string url { get; set; }
    public int id { get; set; }
    public int revision { get; set; }
    public string name { get; set; }
    public string folder { get; set; }
}

public enum RunResult
{
    [EnumMember(Value = "unknown")]
    Unknown,
    [EnumMember(Value = "succeeded")]
    Succeeded,
    [EnumMember(Value = "failed")]
    Failed,
    [EnumMember(Value = "canceled")]
    Canceled
}

public enum RunState
{
    [EnumMember(Value = "unknown")]
    Unknown,
    [EnumMember(Value = "inProgress")]
    InProgress,
    [EnumMember(Value = "canceling")]
    Canceling,
    [EnumMember(Value = "completed")]
    Completed
}