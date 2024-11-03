using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AzureDevopsExplorer.AzureDevopsApi.Git;

public class GitPullRequest
{
    public GitPullRequestRepository repository { get; set; }
    public int pullRequestId { get; set; }
    public int codeReviewId { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PullRequestStatus status { get; set; }
    public IdentityRef createdBy { get; set; }
    public DateTime creationDate { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string sourceRefName { get; set; }
    public string targetRefName { get; set; }
    public string? mergeStatus { get; set; }
    public bool isDraft { get; set; }
    public string? mergeId { get; set; }
    public CommitRef? lastMergeSourceCommit { get; set; }
    public CommitRef? lastMergeTargetCommit { get; set; }
    public CommitRef? lastMergeCommit { get; set; }
    public GitPullRequestReviewer[] reviewers { get; set; }
    public string? url { get; set; }
    public GitPullRequestCompletionOptions? completionOptions { get; set; }
    public bool? supportsIterations { get; set; }
    public GitPullRequestAutoCompleteSetBy? autoCompleteSetBy { get; set; }
}

public class GitPullRequestRepository
{
    public string id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public GitPullRequestProject project { get; set; }
}

public class GitPullRequestProject
{
    public string id { get; set; }
    public string name { get; set; }
    public string state { get; set; }
    public string visibility { get; set; }
    public DateTime lastUpdateTime { get; set; }
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


public class CommitRef
{
    public string commitId { get; set; }
    public string url { get; set; }
}


public class GitPullRequestCompletionOptions
{
    public string mergeCommitMessage { get; set; }
    public bool deleteSourceBranch { get; set; }
    public bool squashMerge { get; set; }
    public string mergeStrategy { get; set; }
    //public object[] autoCompleteIgnoreConfigIds { get; set; }
}

public class GitPullRequestAutoCompleteSetBy
{
    public string displayName { get; set; }
    public string url { get; set; }
    public string id { get; set; }
    public string uniqueName { get; set; }
    public string imageUrl { get; set; }
    public string descriptor { get; set; }
}

public class GitPullRequestReviewer
{
    public string reviewerUrl { get; set; }
    public short vote { get; set; }
    public GitPullRequestVotedfor[] votedFor { get; set; }
    public bool hasDeclined { get; set; }
    public bool isFlagged { get; set; }
    public string displayName { get; set; }
    public string url { get; set; }
    public string id { get; set; }
    public string uniqueName { get; set; }
    public string imageUrl { get; set; }
    public bool isRequired { get; set; }
    public bool isContainer { get; set; }
}

public class GitPullRequestVotedfor
{
    public string reviewerUrl { get; set; }
    public short vote { get; set; }
    public string displayName { get; set; }
    public string url { get; set; }
    public string id { get; set; }
    public string uniqueName { get; set; }
    public string imageUrl { get; set; }
    public bool isContainer { get; set; }
}

public enum PullRequestStatus
{
    [EnumMember(Value = "abandoned")]
    Abandoned,
    [EnumMember(Value = "active")]
    Active,
    [EnumMember(Value = "all")]
    All,
    [EnumMember(Value = "completed")]
    Completed,
    [EnumMember(Value = "notSet")]
    NotSet
}
