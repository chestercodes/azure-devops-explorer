using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace AzureDevopsExplorer.Database.Model.Data;

// Microsoft.TeamFoundation.Build.WebApi.TimelineRecord
// for some reason BuildTimelineRecord.Id being the PK fails sometimes
[PrimaryKey(nameof(BuildTimelineId), nameof(Id))]
public class BuildTimelineRecord
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid BuildTimelineId { get; set; }
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    // RecordType longest value seemed to be Checkpoint.ExtendsCheck
    [StringLength(50)]
    public string? RecordType { get; set; }
    // Name seen values up to 178 chars
    [StringLength(512)]
    public string? Name { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? FinishTime { get; set; }
    public string? CurrentOperation { get; set; }
    public int? PercentComplete { get; set; }
    public BuildTimelineRecordState? State { get; set; }
    public BuildTaskResult? Result { get; set; }
    // ResultCode is stopped at 512 chars
    [StringLength(512)]
    public string? ResultCode { get; set; }
    public int? ChangeId { get; set; }
    public DateTime? LastModified { get; set; }
    // not 100% on workername length
    [StringLength(128)]
    public string? WorkerName { get; set; }
    public int? QueueId { get; set; }
    public int? Order { get; set; }
    public Guid? DetailsId { get; set; }
    public int? DetailsChangeId { get; set; }
    // DetailsUrl not really worth storing
    // takes form https://dev.azure.com/orgname/proj-guid/_apis/build/builds/1234/Timeline/timeline-guid
    //public string? DetailsUrl { get; set; }
    public int? ErrorCount { get; set; }
    public int? WarningCount { get; set; }
    public List<BuildIssue> Issues { get; set; }
    //public Uri? Url { get; set; }
    public int? LogId { get; set; }
    // LogType seems to be null or "Container"
    [StringLength(50)]
    public string? LogType { get; set; }
    // LogUrl not really worth storing
    // takes form https://dev.azure.com/orgname/proj-guid/_apis/build/builds/1234/logs/2
    // public string? LogUrl { get; set; }
    public Guid? TaskId { get; set; }
    // TaskName seen value up to 35
    [StringLength(128)]
    public string? TaskName { get; set; }
    // TaskVersion seen value up to 11
    [StringLength(128)]
    public string? TaskVersion { get; set; }
    public int? Attempt { get; set; }
    // Identifier seen value up to 185
    [StringLength(512)]
    public string? Identifier { get; set; }
    public List<BuildTimelineAttempt> PreviousAttempts { get; set; }
    //public ReferenceLinks? Links { get; set; }
}

// Microsoft.TeamFoundation.Build.WebApi.TimelineAttempt
public class BuildTimelineAttempt
{
    [Key]
    public long Id { get; set; }
    public Guid BuildTimelineRecordId { get; set; }
    public int Attempt { get; set; }
    public Guid TimelineId { get; set; }
    public Guid RecordId { get; set; }
}
[DataContract]
public enum BuildTaskResult
{
    [EnumMember]
    Succeeded,
    [EnumMember]
    SucceededWithIssues,
    [EnumMember]
    Failed,
    [EnumMember]
    Canceled,
    [EnumMember]
    Skipped,
    [EnumMember]
    Abandoned
}
[DataContract]
public enum BuildTimelineRecordState
{
    [EnumMember]
    Pending,
    [EnumMember]
    InProgress,
    [EnumMember]
    Completed
}
// Microsoft.TeamFoundation.Build.WebApi.Issue
public class BuildIssue
{
    public long Id { get; set; }
    public Guid BuildTimelineRecordId { get; set; }
    public BuildIssueType? Type { get; set; }
    // Category seems to be either NULL, General or Code
    [StringLength(50)]
    public string? Category { get; set; }
    // seen value over 16k chars, leave as nvarchar max
    public string? Message { get; set; }
    public List<BuildIssueData> Data { get; set; }
}
public class BuildIssueData
{
    public long Id { get; set; }
    public long BuildIssueId { get; set; }
    // Name seen value up to 17
    [StringLength(128)]
    public string Name { get; set; }
    // Value seen value up to 214
    [StringLength(512)]
    public string Value { get; set; }
}
[DataContract]
public enum BuildIssueType
{
    [EnumMember]
    Error = 1,
    [EnumMember]
    Warning
}
