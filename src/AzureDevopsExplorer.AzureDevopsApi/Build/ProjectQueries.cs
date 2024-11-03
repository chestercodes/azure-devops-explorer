namespace AzureDevopsExplorer.AzureDevopsApi.Build;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<ListResponse<BuildDto>>> GetBuildsFrom(DateTime from, List<int>? pipelineIds = null)
    {
        var definitionsArg = (pipelineIds ?? new List<int>()).Count == 0 ? "" : "&definitions=" + string.Join(",", pipelineIds!);
        var minTime = "&minTime=" + from.ToString("o");
        return await httpClient.DevProject().GetJson<ListResponse<BuildDto>>($"build/builds?queryOrder=queueTimeAscending{minTime}{definitionsArg}");
    }

    public async Task<AzureDevopsApiResult<ListResponse<BuildDto>>> GetPipelineBuildsQueueDescending(int pipelineId)
    {
        return await httpClient.DevProject().GetJson<ListResponse<BuildDto>>($"build/builds?queryOrder=queueTimeDescending&definitions={pipelineId}");
    }

    public async Task<AzureDevopsApiResult<ListResponse<BuildArtifact>>> GetArtifacts(int buildId)
    {
        return await httpClient.DevProject().GetJson<ListResponse<BuildArtifact>>($"build/builds/{buildId}/Artifacts");
    }

    public async Task<AzureDevopsApiResult<string>> GetExpandedYaml(int buildId)
    {
        return await httpClient.DevProject().GetString($"build/builds/{buildId}/logs/1");
    }

    public async Task<AzureDevopsApiResult<BuildTimeline>> GetTimeline(int buildId)
    {
        return await httpClient.DevProject().GetJson<BuildTimeline>($"build/builds/{buildId}/Timeline");
    }
}
