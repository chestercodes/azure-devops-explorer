using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace AzureDevopsExplorer.Database.Model.Data;

// Microsoft.TeamFoundation.Build.WebApi.Build
[PrimaryKey(nameof(Id))]
public class Build
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    [StringLength(Constants.BuildNameLength)]
    public string? BuildNumber { get; set; }
    // TeamProjectReference
    public Guid ProjectId { get; set; }
    public BuildStatus? Status { get; set; }
    public BuildResult? Result { get; set; }
    public DateTime? QueueTime { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? FinishTime { get; set; }
    //public List<ReferenceLink> Links { get; set; } = new();
    // not worth storing, template is https://dev.azure.com/someorg/proj-guid/_apis/build/Builds/1234
    //public string? Url { get; set; }
    public int DefinitionId { get; set; }
    public int DefinitionRevision { get; set; }
    public int? BuildNumberRevision { get; set; }
    //public Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference Project { get; set; }
    [StringLength(1000)]
    public string? Uri { get; set; }
    [StringLength(Constants.BranchRefNameLength)]
    public string SourceBranch { get; set; }
    [StringLength(Constants.GitHashLength)]
    public string SourceVersion { get; set; }
    //public AgentPoolQueue? Queue { get; set; }
    //public AgentSpecification AgentSpecification { get; set; }
    //public BuildController Controller { get; set; }
    public int? QueuePosition { get; set; }
    public QueuePriority? Priority { get; set; }
    public BuildReason? Reason { get; set; }
    public Guid? RequestedForId { get; set; }
    public Guid? RequestedById { get; set; }
    public DateTime? LastChangedDate { get; set; }
    public Guid? LastChangedById { get; set; }
    public DateTime? DeletedDate { get; set; }
    public Guid? DeletedById { get; set; }
    public string? DeletedReason { get; set; }
    public string? Parameters { get; set; }
    //public List<Demand> Demands { get; set; }
    public Guid? OrchestrationPlanPlanId { get; set; }
    public int? OrchestrationPlanOrchestrationType { get; set; }
    //public List<TaskOrchestrationPlanReference> Plans { get; set; }
    //public BuildLogReference? Logs { get; set; }
    public int? LogsId { get; set; }
    // LogsType mostly equals container
    [StringLength(100)]
    public string? LogsType { get; set; }
    // not worth storing, template is https://dev.azure.com/someorg/proj-guid/_apis/build/Builds/1234/logs
    // public string? LogsUrl { get; set; }

    // is normally a guid, but can be a string
    [StringLength(512)]
    public string RepositoryId { get; set; }
    public QueueOptions? QueueOptions { get; set; }
    public bool? Deleted { get; set; }
    //public Dictionary<string, object> Properties { get; set; }
    //public List<string> Tags { get; set; }
    //public List<BuildRequestValidationResult> ValidationResults { get; set; }
    public bool? KeepForever { get; set; }
    //public string? Quality { get; set; }
    public bool? RetainedByRelease { get; set; }
    //public List<BuildTemplateParameter> BuildTemplateParameters { get; set; } = new();
    public int? TriggeredByBuildId { get; set; }
    //public List<TriggerInfo> TriggerInfo { get; set; }
}
public class TriggerInfo
{
    public long Id { get; set; }
    public long BuildId { get; set; }
    // bit of a guess
    [StringLength(512)]
    public string Name { get; set; }
    // bit of a guess longest seen 186
    [StringLength(512)]
    public string Value { get; set; }
}
public class BuildRequestValidationResult
{
    public ValidationResult Result { get; set; }
    public string Message { get; set; }
}
public enum ValidationResult
{
    OK,
    Warning,
    Error,
}
public class Demand
{
    public string Name { get; private set; }
    public string Value { get; private set; }
}
public class AgentSpecification
{
    public string Identifier { get; set; }
}
public class AgentPoolQueue
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public TaskAgentPoolReference Pool { get; set; }
    //public List<ReferenceLink> Links { get; set; } = new();
}
public class TaskAgentPoolReference
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsHosted { get; set; }
}
public class BuildController
{
    public System.Uri Uri { get; set; }
    public string Description { get; set; }
    public ControllerStatus Status { get; set; }
    public bool Enabled { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
}
public enum QueueOptions
{
    [EnumMember]
    None = 0,
    [EnumMember]
    DoNotRun = 1
}
public enum BuildReason
{
    [EnumMember]
    None = 0,
    [EnumMember]
    Manual = 1,
    [EnumMember]
    IndividualCI = 2,
    [EnumMember]
    BatchedCI = 4,
    [EnumMember]
    Schedule = 8,
    [EnumMember]
    ScheduleForced = 16,
    [EnumMember]
    UserCreated = 32,
    [EnumMember]
    ValidateShelveset = 64,
    [EnumMember]
    CheckInShelveset = 128,
    [EnumMember]
    PullRequest = 256,
    [EnumMember]
    BuildCompletion = 512,
    [EnumMember]
    ResourceTrigger = 1024,
    [EnumMember]
    Triggered = 1967,
    [EnumMember]
    All = 2031
}
public enum ControllerStatus
{
    Unavailable,
    Available,
    Offline,
}
public enum BuildResult
{
    [EnumMember]
    None = 0,
    [EnumMember]
    Succeeded = 2,
    [EnumMember]
    PartiallySucceeded = 4,
    [EnumMember]
    Failed = 8,
    [EnumMember]
    Canceled = 0x20
}
public enum BuildStatus
{
    [EnumMember]
    None = 0,
    [EnumMember]
    InProgress = 1,
    [EnumMember]
    Completed = 2,
    [EnumMember]
    Cancelling = 4,
    [EnumMember]
    Postponed = 8,
    [EnumMember]
    NotStarted = 32,
    [EnumMember]
    All = 47
}
public enum QueuePriority
{
    Low = 5,
    BelowNormal = 4,
    Normal = 3,
    AboveNormal = 2,
    High = 1
}