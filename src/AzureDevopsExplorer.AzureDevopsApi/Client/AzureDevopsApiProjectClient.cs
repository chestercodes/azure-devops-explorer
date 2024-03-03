using Flurl.Http;
using System.Text.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiProjectClient
{
    public AzureDevopsApiProjectInfo Info { get; private set; }

    public AzureDevopsApiProjectClient(AzureDevopsApiProjectInfo azureDevopsApiInfo)
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

    public async Task<AzureDevopsApiResult<TJson>> PostJson<TJson>(string path, object body)
    {
        try
        {
            var client = GetClient();
            var url = $"{Info.ApiUrl}/{path}";
            var req = client.Request(url);
            req.WithHeader("Content-Type", "application/json");

            //var data = await req.PostJsonAsync<TJson>(body);

            var bodyJson = JsonSerializer.Serialize(body);
            var resp = await req.PostStringAsync(bodyJson);
            var data = await resp.GetJsonAsync<TJson>();

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

    public async Task<AzureDevopsApiResult<string>> GetFile(Guid repositoryId, string path)
    {
        try
        {
            var client = GetClient();
            // GET https://dev.azure.com/{organization}/{project}/_apis/git/repositories/{repositoryId}/items?path={path}&api-version=6.1-preview.1
            var url = $"{Info.ApiUrl}/git/repositories/{repositoryId}/items?path={path}&download=true";
            var req = client.Request(url);
            var res = await req.GetAsync();
            var content = await res.ResponseMessage.Content.ReadAsStringAsync();
            return content;
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
