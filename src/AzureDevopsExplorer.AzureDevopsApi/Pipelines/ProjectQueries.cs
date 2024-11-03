namespace AzureDevopsExplorer.AzureDevopsApi.Pipelines;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using System.Net.Http.Json;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<PipelineRun>> GetPipelineRun(int pipelineId, int runId)
    {
        return await httpClient.DevProject().GetJson<PipelineRun>($"pipelines/{pipelineId}/runs/{runId}");
    }

    public async Task<AzureDevopsApiResult<PipelineYaml>> GetPipeline(int pipelineId)
    {
        return await httpClient.DevProject().GetJson<PipelineYaml>($"pipelines/{pipelineId}");
    }

    public async Task<AzureDevopsApiResult<PipelineYaml>> GetPipeline(int pipelineId, int pipelineVersion)
    {
        return await httpClient.DevProject().GetJson<PipelineYaml>($"pipelines/{pipelineId}?pipelineVersion={pipelineVersion}");
    }

    public async Task<AzureDevopsApiResult<List<PipelineRef>>> GetPipelines()
    {
        var toReturn = new List<PipelineRef>();

        var carryOn = true;
        string continuationToken = null;
        var client1 = httpClient.DevProject();
        var client = client1.GetClient();
        while (carryOn)
        {
            var conQuery = continuationToken != null ? $"&continuationToken={continuationToken}" : "";
            var url = $"{client1.ApiBase}/pipelines?$top=1000{conQuery}";
            try
            {
                var resp = await client.GetAsync(url);
                var data = await resp.Content.ReadFromJsonAsync<ListResponse<PipelineRef>>();

                if (resp.Headers.Contains("x-ms-continuationtoken"))
                {
                    continuationToken = resp.Headers.SingleOrDefault(x => x.Key == "x-ms-continuationtoken").Value.First();
                }
                else
                {
                    carryOn = false;
                }

                toReturn.AddRange(data.value);
            }
            catch (HttpRequestException ex)
            {
                return new AzureDevopsApiError(ex);
            }
        }

        return toReturn;
    }
}
