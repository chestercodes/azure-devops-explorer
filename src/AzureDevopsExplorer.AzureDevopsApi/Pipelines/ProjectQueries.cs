namespace AzureDevopsExplorer.AzureDevopsApi.Pipelines;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using OneOf;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<PipelineRun>> GetPipelineRun(int pipelineId, int runId)
    {
        return await httpClient.DevProject().GetJson<PipelineRun>($"pipelines/{pipelineId}/runs/{runId}?api-version=7.2-preview.1");
    }

    public async Task<AzureDevopsApiResult<OneOf<PipelineYaml, PipelineSimple>>> GetPipeline(int pipelineId)
    {
        return await httpClient.DevProject().GetJsonComplexOrSimple<PipelineYaml, PipelineSimple>($"pipelines/{pipelineId}?api-version=7.2-preview.1");
    }

    public async Task<AzureDevopsApiResult<OneOf<PipelineYaml, PipelineSimple>>> GetPipeline(int pipelineId, int pipelineVersion)
    {
        return await httpClient.DevProject().GetJsonComplexOrSimple<PipelineYaml, PipelineSimple>($"pipelines/{pipelineId}?pipelineVersion={pipelineVersion}&api-version=7.2-preview.1");
    }

    public async Task<AzureDevopsApiResult<List<PipelineRef>>> GetPipelines()
    {
        var client1 = httpClient.DevProject();
        return await client1.GetJsonWithContinuationTokenFromHeader<PipelineRef>("pipelines");
    }
}
