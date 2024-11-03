namespace AzureDevopsExplorer.Database.Model.Pipelines;

public class GitPullRequestReview
{
    public int Id { get; set; }
    public int PullRequestId { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid? VotedForId { get; set; }
    public short Vote { get; set; }
}
