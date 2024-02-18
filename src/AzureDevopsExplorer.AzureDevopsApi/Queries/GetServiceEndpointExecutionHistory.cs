namespace AzureDevopsExplorer.AzureDevopsApi;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using Flurl.Http;
using GetServiceEndpointExecutionHistoryResult = Client.AzureDevopsApiResult<(List<Dtos.ServiceEndpointExecutionHistory>, string?)>;

public class GetServiceEndpointExecutionHistory
{
    private readonly AzureDevopsApiProjectClient httpClient;

    public GetServiceEndpointExecutionHistory(AzureDevopsApiProjectClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<GetServiceEndpointExecutionHistoryResult> GetNext(string serviceEndpointId, string? continuationToken = null)
    {

        var conQuery = continuationToken != null ? $"?continuationToken={continuationToken}" : "";
        var url = $"{httpClient.Info.ApiUrl}/serviceendpoint/{serviceEndpointId}/executionhistory{conQuery}";
        try
        {
            var client = httpClient.GetClient();
            var req = client.Request(url);
            var resp = await req.GetAsync();
            var data = await resp.GetJsonAsync<ListResponse<ServiceEndpointExecutionHistory>>();

            string nextToken = null;
            if (resp.Headers.Contains("x-ms-continuationtoken"))
            {
                nextToken = resp.Headers.SingleOrDefault(x => x.Name.ToLower() == "x-ms-continuationtoken").Value;
            }

            var history = data.Value.ToList();
            var dataAndToken = (history, nextToken);
            return GetServiceEndpointExecutionHistoryResult.Ok(dataAndToken);
        }
        catch (FlurlHttpException ex)
        {
            try
            {
                var err = await ex.GetResponseJsonAsync<ErrorResponse>();
                return AzureDevopsApiError.FromError(err, ex);
            }
            catch (Exception unEx)
            {
                return AzureDevopsApiError.FromEx(unEx);
            }
        }
    }
}
