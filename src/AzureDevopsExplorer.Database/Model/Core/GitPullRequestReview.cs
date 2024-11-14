namespace AzureDevopsExplorer.Database.Model.Core;

public class GitPullRequestReview
{
    public int Id { get; set; }
    public int PullRequestId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid? VotedForId { get; set; }
    public short Vote { get; set; }
}
