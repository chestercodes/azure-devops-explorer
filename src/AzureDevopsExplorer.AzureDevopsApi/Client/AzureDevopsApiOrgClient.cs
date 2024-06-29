using Flurl.Http;
using Microsoft.Extensions.Logging;
using Polly.Wrap;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiOrgClient
{
    public AzureDevopsApiOrgInfo Info { get; private set; }
    private readonly ILogger logger;
    private readonly AsyncPolicyWrap policy;

    public AzureDevopsApiOrgClient(AzureDevopsApiOrgInfo azureDevopsApiInfo, Func<ILogger> getLogger)
    {
        this.Info = azureDevopsApiInfo;
        logger = getLogger();
        policy = AzureDevopsApiRetry.GetPolicy(logger);
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
            var data = await policy.ExecuteAsync(() => req.GetJsonAsync<TJson>());
            return data;
        }
        catch (Exception unEx)
        {
            return AzureDevopsApiError.FromEx(unEx);
        }
    }

    public async Task<AzureDevopsApiResult<TJson>> GetAuditJson<TJson>(string path)
    {
        try
        {
            var client = GetClient();
            var url = $"{Info.AuditApiUrl}/{path}";
            var req = client.Request(url);
            var data = await policy.ExecuteAsync(() => req.GetJsonAsync<TJson>());
            return data;
        }
        catch (Exception unEx)
        {
            return AzureDevopsApiError.FromEx(unEx);
        }
    }

    public async Task<AzureDevopsApiResult<TJson>> GetJsonFromVssps<TJson>(string path)
    {
        try
        {
            var client = GetClient();
            var url = $"{Info.VsspsApiUrl}/{path}";
            var req = client.Request(url);

            var data = await policy.ExecuteAsync(() => req.GetJsonAsync<TJson>());
            return data;
        }
        catch (Exception unEx)
        {
            return AzureDevopsApiError.FromEx(unEx);
        }
    }
}
