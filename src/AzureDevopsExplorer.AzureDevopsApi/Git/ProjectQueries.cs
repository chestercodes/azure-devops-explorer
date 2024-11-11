namespace AzureDevopsExplorer.AzureDevopsApi.Git;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClientFactory;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<AzureDevopsApiResult<ListResponse<GitRepository>>> GetRepositories()
    {
        return await httpClientFactory.DevProject().GetJson<ListResponse<GitRepository>>($"git/repositories");
    }

    public async Task<AzureDevopsApiResult<ListResponse<GitPullRequest>>> GetAllPullRequestsForRepository(string repo)
    {
        return await httpClientFactory.DevProject().GetJson<ListResponse<GitPullRequest>>($"git/repositories/{repo}/pullrequests?searchCriteria.status=all");
    }

    public async Task<AzureDevopsApiResult<ListResponse<GitCommit>>> GetBranchCommitsForRepository(string repo, string branch)
    {
        var branchName = branch.Replace("refs/heads/", "");
        return await httpClientFactory.DevProject().GetJson<ListResponse<GitCommit>>($"git/repositories/{repo}/commits?searchCriteria.itemVersion.version={branchName}");
    }

    public async Task<AzureDevopsApiResult<string>> GetFile(string repositoryId, string path)
    {
        return await httpClientFactory.DevProject().GetString($"git/repositories/{repositoryId}/items?path={path}&download=true");
    }
}
