namespace AzureDevopsExplorer.IntegrationTests.Stubs.ApiDtos;

using Newtonsoft.Json;
using static Constants;

public class BuildJsonBuilder
{
    public BuildJson Value { get; private set; }

    public string AsJson()
    {
        return JsonConvert.SerializeObject(Value);
    }

    public BuildJsonBuilder(int buildId)
    {
        var definitionId = 1;
        var revisionId = 1;
        var startDateTime = new DateTime(2020, 01, 01, 12, 00, 00);
        var planId = Guid.NewGuid().ToString();
        var userIdentity = new Identity
        {
            descriptor = UserDescriptor,
            displayName = UserDisplayName,
            id = UserId,
            uniqueName = UserUniqueName,
            imageUrl = $"https://dev.azure.com/{OrganisationName}/_apis/GraphProfile/MemberAvatars/{UserDescriptor}",
            url = $"https://spsprodcus5.vssps.visualstudio.com/Ac18a6228-dfb7-4788-b2fe-c980ba3bd818/_apis/Identities/{UserId}",
            _links = new _IdentityLinks
            {
                avatar = new Avatar
                {
                    href = $"https://dev.azure.com/{OrganisationName}/_apis/GraphProfile/MemberAvatars/{UserDescriptor}"
                }
            }
        };
        var project = new Project
        {
            id = ProjectId,
            lastUpdateTime = startDateTime,
            name = ProjectName,
            revision = 1,
            state = "wellFormed",
            visibility = "private",
            url = $"https://dev.azure.com/{OrganisationName}/_apis/projects/{ProjectId}"
        };

        Value = new BuildJson
        {
            appendCommitMessageToRunName = true,
            buildNumber = $"build.{buildId}",
            buildNumberRevision = 1,
            definition = new Definition
            {
                drafts = new string[] { },
                id = definitionId,
                name = "mypipeline",
                path = "\\",
                project = project,
                queueStatus = "enabled",
                revision = revisionId,
                type = "build",
                uri = $"vstfs:///Build/Definition/{definitionId}",
                url = $"https://dev.azure.com/{OrganisationName}/{ProjectId}/_apis/build/Definitions/{definitionId}?revision={revisionId}"
            },
            finishTime = startDateTime.AddDays(buildId + 2),
            id = buildId,
            lastChangedBy = userIdentity,
            lastChangedDate = startDateTime.AddDays(buildId + 1),
            logs = new Logs
            {
                id = 0,
                type = "Container",
                url = $"https://dev.azure.com/{OrganisationName}/{ProjectId}/_apis/build/builds/{buildId}/logs"
            },
            orchestrationPlan = new Orchestrationplan
            {
                planId = planId
            },
            parameters = "{'thing1': 'hi'}",
            plans = new Plan[]
            {
                new Plan
                {
                    planId = planId
                }
            },
            priority = "normal",
            project = project,
            properties = new Properties(),
            queue = new Queue
            {
                id = 9,
                name = "Azure Pipelines",
                pool = new Pool
                {
                    id = 9,
                    name = "Azure Pipelines",
                    isHosted = true
                }
            },
            queueTime = startDateTime,
            reason = "manual",
            repository = new Repository
            {
                id = RepositoryId,
                type = "TfsGit",
                name = RepositoryName,
                checkoutSubmodules = false,
                clean = null,
                url = $"https://dev.azure.com/{OrganisationName}/{ProjectName}/_git/{RepositoryName}"
            },
            requestedBy = userIdentity,
            requestedFor = userIdentity,
            result = "failed",
            retainedByRelease = false,
            sourceBranch = "refs/heads/main",
            sourceVersion = "repo1-main-59113b4876e93f6a4aea54040e92d",
            startTime = startDateTime,
            status = "completed",
            tags = new string[] { },
            triggeredByBuild = null,
            templateParameters = new Dictionary<string, string>
            {
                ["env"] = "prod"
            },
            triggerInfo = new Dictionary<string, string>
            {
                // not a usual value
                ["isPr"] = "true"
            },
            uri = $"vstfs:///Build/Build/{buildId}",
            url = $"https://dev.azure.com/{OrganisationName}/{ProjectId}/_apis/build/Builds/{buildId}",
            validationResults = new object[] { },
            _links = new _Links
            {
                self = new LinkValue
                {
                    href = $"https://dev.azure.com/{OrganisationName}/{ProjectId}/_apis/build/Builds/{buildId}"
                },
                web = new LinkValue
                {
                    href = $"https://dev.azure.com/{OrganisationName}/{ProjectId}/_build/results?buildId={buildId}"
                },
                sourceVersionDisplayUri = new LinkValue
                {
                    href = $"https://dev.azure.com/{OrganisationName}/{ProjectId}/_apis/build/builds/{buildId}/sources"
                },
                timeline = new LinkValue
                {
                    href = $"https://dev.azure.com/{OrganisationName}/{ProjectId}/_apis/build/builds/{buildId}/Timeline"
                },
                badge = new LinkValue
                {
                    href = $"https://dev.azure.com/{OrganisationName}/{ProjectId}/_apis/build/status/{buildId}"
                }
            }
        };
    }

    public class BuildJson
    {
        public _Links _links { get; set; }
        public Properties properties { get; set; }
        public object[] tags { get; set; }
        public object[] validationResults { get; set; }
        public Plan[] plans { get; set; }
        public Dictionary<string, string> templateParameters { get; set; } = new();
        public Dictionary<string, string> triggerInfo { get; set; }
        public int id { get; set; }
        public string buildNumber { get; set; }
        public string status { get; set; }
        public string result { get; set; }
        public string parameters { get; set; }
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
        public Identity requestedFor { get; set; }
        public Identity requestedBy { get; set; }
        public DateTime lastChangedDate { get; set; }
        public Identity lastChangedBy { get; set; }
        public Orchestrationplan orchestrationPlan { get; set; }
        public Logs logs { get; set; }
        public Repository repository { get; set; }
        public bool retainedByRelease { get; set; }
        public object triggeredByBuild { get; set; }
        public bool appendCommitMessageToRunName { get; set; }
    }

    public class _Links
    {
        public LinkValue self { get; set; }
        public LinkValue web { get; set; }
        public LinkValue sourceVersionDisplayUri { get; set; }
        public LinkValue timeline { get; set; }
        public LinkValue badge { get; set; }
    }

    public class LinkValue
    {
        public string href { get; set; }
    }

    public class Properties
    {
    }

    public class Triggerinfo
    {
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
        public Project project { get; set; }
    }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
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

    public class Identity
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public _IdentityLinks _links { get; set; }
        public Guid id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class _IdentityLinks
    {
        public Avatar avatar { get; set; }
    }

    public class Avatar
    {
        public string href { get; set; }
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
        public string planId { get; set; }
    }

}
