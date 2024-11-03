using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

public class PipelineRun
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    //public Dictionary<string, Link> Links { get; set; }
    // template parameters don't differ those retrieved by build
    //public Dictionary<string, string>? TemplateParameters { get; set; }
    // not worth storing and example is
    // https://dev.azure.com/someorg/proj-guid/_apis/pipelines/123?revision=2
    // public string PipelineUrl { get; set; }
    public int PipelineId { get; set; }
    public int PipelineRevision { get; set; }
    [StringLength(Constants.PipelineNameLength)]
    public string PipelineName { get; set; }
    [StringLength(Constants.PipelineNameLength)]
    public string PipelineFolder { get; set; }
    public RunState State { get; set; }
    public RunResult? Result { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? FinishedDate { get; set; }
    // not worth storing and example is
    // https://dev.azure.com/someorg/proj-guid/_apis/pipelines/123/runs/234
    // public string? Url { get; set; }
    public List<PipelineRunRepositoryInfo> ResourcesRepositories { get; set; } = new();
    public List<PipelineRunPipelineInfo> ResourcesPipelines { get; set; } = new();
    // seen up to 103
    [StringLength(512)]
    public string? Name { get; set; }
}

//public class Link
//{
//    public string Href { get; set; }
//}

//public enum RunResult
//{
//    Unknown = 0,
//    Succeeded = 1,
//    Failed = 2,
//    Canceled = 4
//}
//public enum RunState
//{
//    Unknown = 0,
//    InProgress = 1,
//    Canceling = 2,
//    Completed = 4
//}

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