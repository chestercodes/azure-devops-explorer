using Microsoft.VisualStudio.Services.WebApi;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;



// Microsoft.TeamFoundation.SourceControl.WebApi.GitPullRequestStatus
public class GitPullRequestStatus
{
    public int? IterationId { get; set; }
    //public PropertiesCollection Properties { get; set; }
    public int Id { get; set; }
    public GitStatusState State { get; set; }
    public string Description { get; set; }
    public GitStatusContext Context { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public IdentityRef CreatedBy { get; set; }
    public string TargetUrl { get; set; }
    public ReferenceLinks Links { get; set; }
}

// Microsoft.TeamFoundation.SourceControl.WebApi.GitCommitRef
public class GitCommitRef
{
    public string CommitId { get; set; }
    public GitUserDate Author { get; set; }
    public GitUserDate Committer { get; set; }
    public string Comment { get; set; }
    public bool CommentTruncated { get; set; }
    public List<ChangeCount> ChangeCounts { get; set; }
    public IEnumerable<GitChange> Changes { get; set; }
    public IEnumerable<string> Parents { get; set; }
    public string Url { get; set; }
    public string RemoteUrl { get; set; }
    public ReferenceLinks Links { get; set; }
    public IList<GitStatus> Statuses { get; set; }
    public IList<ResourceRef> WorkItems { get; set; }
    public GitPushRef Push { get; set; }
}

// Microsoft.TeamFoundation.SourceControl.WebApi.GitPushRef
public class GitPushRef
{
    public IdentityRef PushedBy { get; set; }
    public int PushId { get; set; }
    public Guid PushCorrelationId { get; set; }
    public DateTime Date { get; set; }
    public string Url { get; set; }
    public ReferenceLinks Links { get; set; }
}

// Microsoft.TeamFoundation.SourceControl.WebApi.GitStatus
public class GitStatus
{
    public int Id { get; set; }
    public GitStatusState State { get; set; }
    public string Description { get; set; }
    public GitStatusContext Context { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public IdentityRef CreatedBy { get; set; }
    public string TargetUrl { get; set; }
    public ReferenceLinks Links { get; set; }
}

//// Microsoft.TeamFoundation.SourceControl.WebApi.ChangeCountDictionary
//public class ChangeCountDictionary
//{
//    public IEqualityComparer<VersionControlChangeType> Comparer { get; set; }
//    public int Count { get; set; }
//    public Dictionary<VersionControlChangeType, int> Keys { get; set; }
//    public Dictionary<VersionControlChangeType, int> Values { get; set; }
//    public int Item { get; set; }
//}

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

// Microsoft.TeamFoundation.SourceControl.WebApi.GitChange
public class GitChange
{
    public string OriginalPath { get; set; }
    public int ChangeId { get; set; }
    public GitTemplate NewContentTemplate { get; set; }
    public GitItem Item { get; set; }
    public string SourceServerItem { get; set; }
    public VersionControlChangeType ChangeType { get; set; }
    public ItemContent NewContent { get; set; }
    public string Url { get; set; }
}

// Microsoft.TeamFoundation.SourceControl.WebApi.ItemContent
public class ItemContent
{
    public string Content { get; set; }
    public ItemContentType ContentType { get; set; }
}

public enum ItemContentType
{
    RawText,
    Base64Encoded,
}

// Microsoft.TeamFoundation.SourceControl.WebApi.GitTemplate
public class GitTemplate
{
    public string Name { get; set; }
    public string Type { get; set; }
}

// Microsoft.TeamFoundation.SourceControl.WebApi.GitItem
public class GitItem
{
    public string ObjectId { get; set; }
    public string OriginalObjectId { get; set; }
    public GitObjectType GitObjectType { get; set; }
    public string CommitId { get; set; }
    public GitCommitRef LatestProcessedChange { get; set; }
    public string Path { get; set; }
    public bool IsFolder { get; set; }
    public string Content { get; set; }
    public FileContentMetadata ContentMetadata { get; set; }
    public bool IsSymbolicLink { get; set; }
    public string Url { get; set; }
    public ReferenceLinks Links { get; set; }
}

// Microsoft.TeamFoundation.SourceControl.WebApi.FileContentMetadata
public class FileContentMetadata
{
    public int Encoding { get; set; }
    public bool EncodingWithBom { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
    public string Extension { get; set; }
    public bool IsBinary { get; set; }
    public bool IsImage { get; set; }
    public string VisualStudioWebLink { get; set; }
}

public enum GitObjectType
{
    Bad,
    Commit,
    Tree,
    Blob,
    Tag,
    Ext2,
    OfsDelta,
    RefDelta,
}

public class WebApiTagDefinition
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool? Active { get; set; }
    public string Url { get; set; }
}

// Microsoft.TeamFoundation.SourceControl.WebApi.GitPullRequestCompletionOptions
public class GitPullRequestCompletionOptions
{
    public string MergeCommitMessage { get; set; }
    public bool DeleteSourceBranch { get; set; }
    public bool SquashMerge { get; set; }
    public GitPullRequestMergeStrategy? MergeStrategy { get; set; }
    public bool BypassPolicy { get; set; }
    public string BypassReason { get; set; }
    public bool TransitionWorkItems { get; set; }
    public bool TriggeredByAutoComplete { get; set; }
    public List<int> AutoCompleteIgnoreConfigIds { get; set; }
}

// Microsoft.TeamFoundation.SourceControl.WebApi.GitPullRequestMergeOptions
public class GitPullRequestMergeOptions
{
    public bool? DisableRenames { get; set; }
    public bool? DetectRenameFalsePositives { get; set; }
    public bool? ConflictAuthorshipCommits { get; set; }
}

// Microsoft.VisualStudio.Services.WebApi.ResourceRef
public class ResourceRef
{
    public string Id { get; set; }
    public string Url { get; set; }
}

// Microsoft.TeamFoundation.SourceControl.WebApi.GitRepositoryRef
public class GitRepositoryRef
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsFork { get; set; }
    public string Url { get; set; }
    public string RemoteUrl { get; set; }
    public string SshUrl { get; set; }
    public TeamProjectReference ProjectReference { get; set; }
    public TeamProjectCollectionReference Collection { get; set; }
}

// Microsoft.TeamFoundation.Core.WebApi.TeamProjectCollectionReference
public class TeamProjectCollectionReference
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
}

public enum GitStatusState
{
    NotSet,
    Pending,
    Succeeded,
    Failed,
    Error,
    NotApplicable,
}

// Microsoft.TeamFoundation.SourceControl.WebApi.GitStatusContext
public class GitStatusContext
{
    public string Name { get; set; }
    public string Genre { get; set; }
}

// Microsoft.TeamFoundation.SourceControl.WebApi.GitUserDate
public class GitUserDate
{
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime Date { get; set; }
    public string ImageUrl { get; set; }
}

public class GitForkRef
{
    public GitRepository Repository { get; set; }
    public string Name { get; set; }
    public string ObjectId { get; set; }
    public IdentityRef IsLockedBy { get; set; }
    public bool IsLocked { get; set; }
    public IdentityRef Creator { get; set; }
    public string Url { get; set; }
    public string PeeledObjectId { get; set; }
    public IEnumerable<GitStatus> Statuses { get; set; }
    public ReferenceLinks Links { get; set; }
}
