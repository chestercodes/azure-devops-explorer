namespace AzureDevopsExplorer.AzureDevopsApi.Build;

public class BuildTimeline
{
    public BuildTimelineRecord[] records { get; set; }
    public string lastChangedBy { get; set; }
    public DateTime lastChangedOn { get; set; }
    public string id { get; set; }
    public int changeId { get; set; }
    public string url { get; set; }
}

public class BuildTimelineRecord
{
    public object[]? previousAttempts { get; set; }
    public string id { get; set; }
    public string? parentId { get; set; }
    public string type { get; set; }
    public string name { get; set; }
    public DateTime? startTime { get; set; }
    public DateTime? finishTime { get; set; }
    public string? currentOperation { get; set; }
    public int? percentComplete { get; set; }
    public string state { get; set; }
    public string result { get; set; }
    public string? resultCode { get; set; }
    public int changeId { get; set; }
    public DateTime? lastModified { get; set; }
    public string? workerName { get; set; }
    public BuildTimelineDetails? details { get; set; }
    public int errorCount { get; set; }
    public int warningCount { get; set; }
    public string? url { get; set; }
    public BuildTimelineLog? log { get; set; }
    public BuildTimelineTask? task { get; set; }
    public int attempt { get; set; }
    public string identifier { get; set; }
    public int order { get; set; }
    public BuildTimelineIssue[]? issues { get; set; }
    public int? queueId { get; set; }
}

public class BuildTimelineLog
{
    public int id { get; set; }
    public string type { get; set; }
    public string url { get; set; }
}

public class BuildTimelineTask
{
    public string id { get; set; }
    public string name { get; set; }
    public string version { get; set; }
}

public class BuildTimelineIssue
{
    public string type { get; set; }
    public string? category { get; set; }
    public string message { get; set; }
    public BuildTimelineData data { get; set; }
}

public class BuildTimelineData
{
    public string logFileLineNumber { get; set; }
}

public class BuildTimelineDetails
{
    public Guid? id { get; set; }
    public int? changeId { get; set; }
}
