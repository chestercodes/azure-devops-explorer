using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

public class GitPullRequest
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PullRequestId { get; set; }
    //public GitRepository Repository { get; set; }
    public Guid RepositoryId { get; set; }
    public int? CodeReviewId { get; set; }
    public PullRequestStatus Status { get; set; }
    //public IdentityRef CreatedBy { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string SourceRefName { get; set; }
    public string TargetRefName { get; set; }
    public PullRequestAsyncStatus? MergeStatus { get; set; }
    public PullRequestMergeFailureType? MergeFailureType { get; set; }
    public string? MergeFailureMessage { get; set; }
    public bool? IsDraft { get; set; }
    public bool? HasMultipleMergeBases { get; set; }
    public Guid? MergeId { get; set; }
    [StringLength(40)]
    public string? LastMergeSourceCommitId { get; set; }
    [StringLength(40)]
    public string? LastMergeTargetCommitId { get; set; }
    [StringLength(40)]
    public string? LastMergeCommitId { get; set; }
    //public IdentityRefWithVote[]? Reviewers { get; set; }
    //public WebApiTagDefinition[]? Labels { get; set; }
    //public GitCommitRef[] Commits { get; set; }
    public string? Url { get; set; }
    public string? RemoteUrl { get; set; }
    //public ReferenceLinks Links { get; set; }
    //public GitPullRequestCompletionOptions? CompletionOptions { get; set; }
    public string? CompletionOptionsMergeCommitMessage { get; set; }
    public bool? CompletionOptionsDeleteSourceBranch { get; set; }
    public bool? CompletionOptionsSquashMerge { get; set; }
    public GitPullRequestMergeStrategy? CompletionOptionsMergeStrategy { get; set; }
    public bool? CompletionOptionsBypassPolicy { get; set; }
    public string? CompletionOptionsBypassReason { get; set; }
    public bool? CompletionOptionsTransitionWorkItems { get; set; }
    public bool? CompletionOptionsTriggeredByAutoComplete { get; set; }


    //public GitPullRequestMergeOptions? MergeOptions { get; set; }
    public bool? MergeOptionsDisableRenames { get; set; }
    public bool? MergeOptionsDetectRenameFalsePositives { get; set; }
    public bool? MergeOptionsConflictAuthorshipCommits { get; set; }
    public bool? SupportsIterations { get; set; }
    //public ResourceRef[]? WorkItemRefs { get; set; }
    public DateTime? CompletionQueueTime { get; set; }
    //public IdentityRef? ClosedBy { get; set; }
    public Guid? ClosedById { get; set; }
    //public IdentityRef? AutoCompleteSetBy { get; set; }
    public Guid? AutoCompleteSetById { get; set; }
    public string? ArtifactId { get; set; }
    //public GitForkRef? ForkSource { get; set; }
}

public enum PullRequestStatus
{
    NotSet,
    Active,
    Abandoned,
    Completed,
    All
}

public enum PullRequestAsyncStatus
{
    NotSet,
    Queued,
    Conflicts,
    Succeeded,
    RejectedByPolicy,
    Failure,
}
public enum PullRequestMergeFailureType
{
    None,
    Unknown,
    CaseSensitive,
    ObjectTooLarge,
}

public enum GitPullRequestMergeStrategy
{
    NoFastForward,
    Squash,
    Rebase,
    RebaseMerge,
}

//public class GitForkRef
//{
//    public GitRepository Repository { get; set; }
//    public string Name { get; set; }
//    public string ObjectId { get; set; }
//    public IdentityRef IsLockedBy { get; set; }
//    public bool IsLocked { get; set; }
//    public IdentityRef Creator { get; set; }
//    public string Url { get; set; }
//    public string PeeledObjectId { get; set; }
//    public IEnumerable<GitStatus> Statuses { get; set; }
//    public ReferenceLinks Links { get; set; }
//}


public class GitCommitRef
{
    //public string CommitId { get; set; }
    //public GitUserDate Author { get; set; }
    //public GitUserDate Committer { get; set; }
    //public string Comment { get; set; }
    //public bool CommentTruncated { get; set; }
    //public List<ChangeCount> ChangeCounts { get; set; }
    //public IEnumerable<GitChange> Changes { get; set; }
    //public IEnumerable<string> Parents { get; set; }
    //public string Url { get; set; }
    //public string RemoteUrl { get; set; }
    //public ReferenceLinks Links { get; set; }
    //public IList<GitStatus> Statuses { get; set; }
    //public IList<ResourceRef> WorkItems { get; set; }
    //public GitPushRef Push { get; set; }
}



public class ChangeCount
{
    public string CommitId { get; set; }
    public VersionControlChangeType ChangeType { get; set; }
    public int Count { get; set; }
}

public enum VersionControlChangeType
{
    None,
    Add,
    Edit,
    Encoding,
    Rename,
    Delete,
    Undelete,
    Branch,
    Merge,
    Lock,
    Rollback,
    SourceRename,
    TargetRename,
    Property,
    All,
}