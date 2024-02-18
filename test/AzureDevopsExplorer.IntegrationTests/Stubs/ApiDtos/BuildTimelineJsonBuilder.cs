namespace AzureDevopsExplorer.IntegrationTests.Stubs.ApiDtos;

using Newtonsoft.Json;
using static Constants;

public class BuildTimelineJsonBuilder
{
    public BuildTimelineJson Value { get; private set; }

    public BuildTimelineJson Build()
    {
        return Value;
    }
    public string AsJson()
    {
        return JsonConvert.SerializeObject(Value);
    }

    public BuildTimelineJsonBuilder(int buildId)
    {
        var guidPart = buildId.ToString("0000");
        var id = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000001");
        var lastChangedBy = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000002");
        var record1Id = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000003");
        var record2Id = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000004");
        var record1ParentId = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000005");
        var record2ParentId = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000006");

        Value = new BuildTimelineJson
        {
            lastChangedBy = lastChangedBy,
            changeId = 14,
            id = id,
            url = $"https://dev.azure.com/{OrganisationName}/{ProjectId}/_apis/build/builds/{buildId}/Timeline/{id}",
            lastChangedOn = new DateTime(2023, 01, 01, 12, 01, 01),
            records =
            [
                new Record
                {
                    id = record1Id,
                    parentId = record1ParentId,
                    type = "Task",
                    name = "Run pwsh script 1",
                    startTime = new DateTime(2023, 01, 01, 12, 01, 02),
                    finishTime = new DateTime(2023, 01, 01, 12, 01, 03),
                    //currentOperation
                    percentComplete = 16,
                    state = "completed",
                    result = "skipped",
                    resultCode = "Skipping",
                    changeId = 18,
                    lastModified = new DateTime(2023, 01, 01, 12, 01, 04),
                    workerName = "Azure Pipelines worker",
                    order = 20,
                    errorCount = 0,
                    warningCount = 0,
                    url = $"https://dev.azure.com/{OrganisationName}/{ProjectName}/_apis/build/builds/{buildId}/logs/22/notsure",
                    log = new Log
                    {
                        id = 22,
                        type = "Container",
                        url = $"https://dev.azure.com/{OrganisationName}/{ProjectName}/_apis/build/builds/{buildId}/logs/22"
                    },
                    task = new Task
                    {
                        id = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000007"),
                        name = "PowerShell",
                        version = "1.2.3"
                    },
                    attempt = 1,
                    identifier = "record1",
                    previousAttempts = new List<PreviousAttempt>
                    {
                        new PreviousAttempt
                        {
                            attempt = 1,
                            recordId = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000008"),
                            timelineId = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000009")
                        },
                        new PreviousAttempt
                        {
                            attempt = 2,
                            recordId = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000010"),
                            timelineId = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000011")
                        }
                    },
                    details = new Details
                    {
                        changeId = 32,
                        id = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000013").ToString(),
                        url = "https://some.url.com/details1"
                    },
                    queueId = 34,
                    issues = new List<Issue>
                    {
                        new Issue
                        {
                            category = "thing",
                            message = "Warning about your build",
                            type = "warning",
                            data = new Dictionary<string, string>
                            {
                                ["thing1"] = "thing2"
                            }
                        }
                    }
                },
                new Record
                {
                    id = record2Id,
                    parentId = record2ParentId,
                    type = "Task",
                    name = "Run pwsh script 2",
                    startTime = new DateTime(2023, 01, 01, 12, 02, 01),
                    finishTime = new DateTime(2023, 01, 01, 12, 02, 02),
                    //currentOperation
                    percentComplete = null,
                    state = "completed",
                    result = "succeeded",
                    resultCode = null,
                    changeId = 26,
                    lastModified = new DateTime(2023, 01, 01, 12, 02, 03),
                    workerName = "Azure Pipelines worker",
                    order = 28,
                    errorCount = 0,
                    warningCount = 0,
                    url = $"https://dev.azure.com/{OrganisationName}/{ProjectName}/_apis/build/builds/{buildId}/logs/30/notsure",
                    log = new Log
                    {
                        id = 30,
                        type = "Container",
                        url = $"https://dev.azure.com/{OrganisationName}/{ProjectName}/_apis/build/builds/{buildId}/logs/30"
                    },
                    task = new Task
                    {
                        id = Guid.Parse($"10000000-0000-0000-{guidPart}-000000000012"),
                        name = "PowerShell",
                        version = "1.2.3"
                    },
                    attempt = 1,
                    identifier = "record2",
                    previousAttempts = new List<PreviousAttempt> { }
                }
            ]
        };
    }



    public class BuildTimelineJson
    {
        public Record[] records { get; set; }
        public Guid lastChangedBy { get; set; }
        public DateTime lastChangedOn { get; set; }
        public Guid id { get; set; }
        public int changeId { get; set; }
        public string url { get; set; }
    }

    public class Record
    {
        public List<PreviousAttempt> previousAttempts { get; set; }
        public Guid id { get; set; }
        public Guid parentId { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public DateTime? startTime { get; set; }
        public DateTime? finishTime { get; set; }
        public object currentOperation { get; set; }
        public int? percentComplete { get; set; }
        public string state { get; set; }
        public string result { get; set; }
        public string resultCode { get; set; }
        public int changeId { get; set; }
        public DateTime lastModified { get; set; }
        public string workerName { get; set; }
        public int order { get; set; }
        public Details details { get; set; }
        public int errorCount { get; set; }
        public int warningCount { get; set; }
        public string url { get; set; }
        public Log log { get; set; }
        public Task task { get; set; }
        public int attempt { get; set; }
        public string identifier { get; set; }
        public int queueId { get; set; }
        public List<Issue> issues { get; set; }
    }

    public class PreviousAttempt
    {
        public int attempt { get; set; }
        public Guid timelineId { get; set; }
        public Guid recordId { get; set; }
    }

    public class Details
    {
        public string id { get; set; }
        public int changeId { get; set; }
        public string url { get; set; }
    }

    public class Log
    {
        public int id { get; set; }
        public string type { get; set; }
        public string url { get; set; }
    }

    public class Task
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string version { get; set; }
    }

    public class Issue
    {
        public string? type { get; set; }
        public string? category { get; set; }
        public string message { get; set; }
        public Dictionary<string, string> data { get; set; }
    }
}
