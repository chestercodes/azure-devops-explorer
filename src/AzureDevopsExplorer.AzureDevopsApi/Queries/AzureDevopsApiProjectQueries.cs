namespace AzureDevopsExplorer.AzureDevopsApi;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class AzureDevopsApiProjectQueries
{
    private readonly AzureDevopsApiProjectClient httpClient;

    public AzureDevopsApiProjectQueries(AzureDevopsApiProjectClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<PipelineRun>> GetPipelineRun(int pipelineId, int runId)
    {
        return await httpClient.GetJson<PipelineRun>($"pipelines/{pipelineId}/runs/{runId}");
    }

    public async Task<AzureDevopsApiResult<PipelineYaml>> GetPipeline(int pipelineId)
    {
        return await httpClient.GetJson<PipelineYaml>($"pipelines/{pipelineId}");
    }

    public async Task<AzureDevopsApiResult<PipelineYaml>> GetPipeline(int pipelineId, int pipelineVersion)
    {
        return await httpClient.GetJson<PipelineYaml>($"pipelines/{pipelineId}?pipelineVersion={pipelineVersion}");
    }

    public async Task<AzureDevopsApiResult<ListResponse<VariableGroup>>> GetVariableGroups()
    {
        return await httpClient.GetJson<ListResponse<VariableGroup>>($"distributedtask/variablegroups");
    }

    public async Task<AzureDevopsApiResult<VariableGroup>> GetVariableGroup(int id)
    {
        return await httpClient.GetJson<VariableGroup>($"distributedtask/variablegroups/{id}");
    }

    public async Task<AzureDevopsApiResult<ListResponse<ServiceEndpoint>>> GetServiceEndpoints()
    {
        return await httpClient.GetJson<ListResponse<ServiceEndpoint>>($"serviceendpoint/endpoints");
    }
}
