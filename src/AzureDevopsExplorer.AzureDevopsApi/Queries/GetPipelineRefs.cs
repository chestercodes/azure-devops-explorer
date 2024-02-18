namespace AzureDevopsExplorer.AzureDevopsApi;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using Flurl.Http;

public class GetPipelineRefs
{
    private readonly AzureDevopsApiProjectClient httpClient;

    public GetPipelineRefs(AzureDevopsApiProjectClient httpClient)
    {
        this.httpClient = httpClient;
    }
    public async Task<AzureDevopsApiResult<List<PipelineRef>>> GetAll()
    {
        var toReturn = new List<PipelineRef>();

        var carryOn = true;
        string continuationToken = null;
        while (carryOn)
        {
            var conQuery = continuationToken != null ? $"&continuationToken={continuationToken}" : "";
            var url = $"{httpClient.Info.ApiUrl}/pipelines?$top=1000{conQuery}";
            try
            {
                var client = httpClient.GetClient();
                var req = client.Request(url);
                var resp = await req.GetAsync();
                var data = await resp.GetJsonAsync<ListResponse<PipelineRef>>();

                if (resp.Headers.Contains("x-ms-continuationtoken"))
                {
                    continuationToken = resp.Headers.SingleOrDefault(x => x.Name == "x-ms-continuationtoken").Value;
                }
                else
                {
                    carryOn = false;
                }

                toReturn.AddRange(data.Value);
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

        return toReturn;
    }
}
