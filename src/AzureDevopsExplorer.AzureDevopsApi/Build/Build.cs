using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AzureDevopsExplorer.AzureDevopsApi.Build;


public class BuildDto
{
    public Dictionary<string, string> properties { get; set; }
    public object[] tags { get; set; }
    public object[] validationResults { get; set; }
    public Plan[] plans { get; set; }
    public Dictionary<string, string> templateParameters { get; set; }
    public Dictionary<string, string> triggerInfo { get; set; }
    public int id { get; set; }
    public string buildNumber { get; set; }
    //public string status { get; set; }
    //public string result { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BuildStatus status { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BuildResult result { get; set; }
    public DateTime queueTime { get; set; }
    public DateTime startTime { get; set; }
    public DateTime finishTime { get; set; }
    public string url { get; set; }
    public Definition definition { get; set; }
    public int buildNumberRevision { get; set; }
    public Project project { get; set; }
    public string uri { get; set; }
    public string sourceBranch { get; set; }
    public string sourceVersion { get; set; }
    public Queue queue { get; set; }
    public string priority { get; set; }
    public string reason { get; set; }
    public IdentityRef requestedFor { get; set; }
    public IdentityRef requestedBy { get; set; }
    public DateTime lastChangedDate { get; set; }
    public IdentityRef lastChangedBy { get; set; }
    public Orchestrationplan orchestrationPlan { get; set; }
    public Logs logs { get; set; }
    public Repository repository { get; set; }
    public bool retainedByRelease { get; set; }
    public int? triggeredByBuild { get; set; }
    public bool appendCommitMessageToRunName { get; set; }
}

public class Definition
{
    public object[] drafts { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public string uri { get; set; }
    public string path { get; set; }
    public string type { get; set; }
    public string queueStatus { get; set; }
    public int revision { get; set; }
    public DefinitionProject project { get; set; }
}

public class DefinitionProject
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string url { get; set; }
    public string state { get; set; }
    public int revision { get; set; }
    public string visibility { get; set; }
    public DateTime lastUpdateTime { get; set; }
}

public class Project
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string url { get; set; }
    public string state { get; set; }
    public int revision { get; set; }
    public string visibility { get; set; }
    public DateTime lastUpdateTime { get; set; }
}

public class Queue
{
    public int id { get; set; }
    public string name { get; set; }
    public Pool pool { get; set; }
}

public class Pool
{
    public int id { get; set; }
    public string name { get; set; }
    public bool isHosted { get; set; }
}

public class IdentityRef
{
    public string displayName { get; set; }
    public string url { get; set; }
    public string id { get; set; }
    public string uniqueName { get; set; }
    public string imageUrl { get; set; }
    public string descriptor { get; set; }
}

public class Orchestrationplan
{
    public string planId { get; set; }
}

public class Logs
{
    public int id { get; set; }
    public string type { get; set; }
    public string url { get; set; }
}

public class Repository
{
    public string id { get; set; }
    public string type { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public object clean { get; set; }
    public bool checkoutSubmodules { get; set; }
}

public class Plan
{
    public Guid planId { get; set; }
    public int? orchestrationType { get; set; }
}

public enum BuildResult
{
    [EnumMember(Value = "canceled")]
    Canceled,
    [EnumMember(Value = "failed")]
    Failed,
    [EnumMember(Value = "none")]
    None,
    [EnumMember(Value = "partiallySucceeded")]
    PartiallySucceeded,
    [EnumMember(Value = "succeeded")]
    Succeeded
}

public enum BuildStatus
{
    [EnumMember(Value = "cancelling")]
    Cancelling,
    [EnumMember(Value = "completed")]
    Completed,
    [EnumMember(Value = "inProgress")]
    InProgress,
    [EnumMember(Value = "none")]
    None,
    [EnumMember(Value = "notStarted")]
    NotStarted,
    [EnumMember(Value = "postponed")]
    Postponed
}
