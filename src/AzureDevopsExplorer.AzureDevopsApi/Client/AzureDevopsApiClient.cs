using Microsoft.Extensions.Logging;
using Polly.Wrap;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public record ApiBase(string Value);

public class AzureDevopsApiClient
{
    private readonly IGetAuthHeader azureDevopsApiAuthHeader;
    public string ApiBase { get; private set; }
    private readonly CancellationToken cancellationToken;
    private readonly ILogger logger;
    private readonly AsyncPolicyWrap policy;

    public AzureDevopsApiClient(IGetAuthHeader azureDevopsApiAuthHeader, ApiBase apiBase, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
    {
        this.azureDevopsApiAuthHeader = azureDevopsApiAuthHeader;
        ApiBase = apiBase.Value;
        this.cancellationToken = cancellationToken;
        logger = loggerFactory.CreateLogger(GetType());
        policy = AzureDevopsApiRetry.GetPolicy(logger);
    }

    public HttpClient GetClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", azureDevopsApiAuthHeader.Get());
        return client;
    }

    public async Task<AzureDevopsApiResult<TJson>> GetJson<TJson>(string path)
    {
        try
        {
            var client = GetClient();
            var url = $"{ApiBase}/{path}";
            var data = await policy.ExecuteAsync(() => client.GetFromJsonAsync<TJson>(url, cancellationToken));
            return data;
        }
        catch (HttpRequestException ex)
        {
            return new AzureDevopsApiError(ex);
        }
    }

    public async Task<AzureDevopsApiResult<string>> GetString(string path)
    {
        try
        {
            var client = GetClient();
            var url = $"{ApiBase}/{path}";
            var data = await policy.ExecuteAsync(() => client.GetStringAsync(url, cancellationToken));
            return data;
        }
        catch (HttpRequestException ex)
        {
            return new AzureDevopsApiError(ex);
        }
    }

    public async Task<AzureDevopsApiResult<TJson>> PostJson<TJson>(string path, object body)
    {
        try
        {
            var client = GetClient();
            var url = $"{ApiBase}/{path}";
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var bodyJson = JsonSerializer.Serialize(body);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            var data = await policy.ExecuteAsync(
                async () =>
                {
                    var resp = await client.SendAsync(request, cancellationToken);
                    resp.EnsureSuccessStatusCode();
                    return await resp.Content.ReadFromJsonAsync<TJson>(cancellationToken);
                });

            return data;
        }
        catch (HttpRequestException ex)
        {
            return new AzureDevopsApiError(ex);
        }
    }
}
