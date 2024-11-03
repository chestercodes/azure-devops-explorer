namespace AzureDevopsExplorer.AzureDevopsApi.ServiceEndpoints;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using System.Net.Http.Json;
using GetServiceEndpointExecutionHistoryResult = Client.AzureDevopsApiResult<(List<ServiceEndpointExecutionHistory>, string?)>;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<ListResponse<ServiceEndpoint>>> GetServiceEndpoints()
    {
        return await httpClient.DevProject().GetJson<ListResponse<ServiceEndpoint>>($"serviceendpoint/endpoints");
    }

    public async Task<GetServiceEndpointExecutionHistoryResult> GetNext(Guid serviceEndpointId, string? continuationToken = null)
    {
        var conQuery = continuationToken != null ? $"?continuationToken={continuationToken}" : "";
        var path = $"serviceendpoint/{serviceEndpointId}/executionhistory{conQuery}";
        try
        {
            var client = httpClient.DevProject().GetClient();
            var resp = await client.GetAsync(path);
            var data = await resp.Content.ReadFromJsonAsync<ListResponse<ServiceEndpointExecutionHistory>>();

            string nextToken = null;
            if (resp.Headers.Contains("x-ms-continuationtoken"))
            {
                nextToken = resp.Headers.SingleOrDefault(x => x.Key.ToLower() == "x-ms-continuationtoken").Value.First();
            }

            var history = data.value.ToList();
            var dataAndToken = (history, nextToken);
            return GetServiceEndpointExecutionHistoryResult.Ok(dataAndToken);
        }
        catch (HttpRequestException ex)
        {
            return new AzureDevopsApiError(ex);
        }
    }
}
