using Flurl.Http;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiOrgClient
{
    public AzureDevopsApiOrgInfo Info { get; private set; }

    public AzureDevopsApiOrgClient(AzureDevopsApiOrgInfo azureDevopsApiInfo)
    {
        this.Info = azureDevopsApiInfo;
    }

    public FlurlClient GetClient()
    {
        var client = new FlurlClient();

        if (Info.BearerToken != null)
        {
            client.WithOAuthBearerToken(Info.BearerToken);
        }

        return client;
    }

    public async Task<AzureDevopsApiResult<TJson>> GetJson<TJson>(string path)
    {
        try
        {
            var client = GetClient();
            var url = $"{Info.ApiUrl}/{path}";
            var req = client.Request(url);
            var data = await req.GetJsonAsync<TJson>();
            return data;
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
                var thing = await ex.Call.Response.GetStringAsync();
                return AzureDevopsApiError.FromEx(unEx);
            }
        }
    }
}
