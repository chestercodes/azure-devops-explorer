namespace AzureDevopsExplorer.AzureDevopsApi;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

public class FindPullRequestsAndEntities
{
    private string projectName;
    private VssConnection connection;

    public FindPullRequestsAndEntities(VssConnection connection, string projectName)
    {
        this.connection = connection;
        this.projectName = projectName;
    }

    public async Task<IEnumerable<GitRepository>> GetRepositories()
    {
        var client = new GitHttpClient(connection.Uri, connection.Credentials);
        var repos = await client.GetRepositoriesAsync(projectName);
        return repos;
    }

    public async Task<IEnumerable<GitPullRequest>> GetPullRequests(Guid repositoryId)
    {
        var client = new GitHttpClient(connection.Uri, connection.Credentials);
        var repos = await client.GetPullRequestsAsync(repositoryId, new GitPullRequestSearchCriteria { RepositoryId = repositoryId, Status = PullRequestStatus.All });
        return repos;
    }
}
