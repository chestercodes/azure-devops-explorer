namespace AzureDevopsExplorer.AzureDevopsApi.Environments;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<ListResponse<PipelineEnvironment>>> GetPipelineEnvironments()
    {
        return await httpClient.DevProject().GetJson<ListResponse<PipelineEnvironment>>($"pipelines/environments");
    }
}
