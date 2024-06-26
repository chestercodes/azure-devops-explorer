﻿using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Polly.Wrap;
using System.Text.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiProjectClient
{
    public AzureDevopsApiProjectInfo Info { get; private set; }

    private readonly ILogger logger;
    private readonly AsyncPolicyWrap policy;

    public AzureDevopsApiProjectClient(AzureDevopsApiProjectInfo azureDevopsApiInfo, Func<ILogger> getLogger)
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

    public async Task<AzureDevopsApiResult<TJson>> PostJson<TJson>(string path, object body)
    {
        try
        {
            var client = GetClient();
            var url = $"{Info.ApiUrl}/{path}";
            var req = client.Request(url);
            req.WithHeader("Content-Type", "application/json");
            var bodyJson = JsonSerializer.Serialize(body);

            var data = await policy.ExecuteAsync(
                async () =>
                {
                    var resp = await req.PostStringAsync(bodyJson);
                    return await resp.GetJsonAsync<TJson>();
                });

            return data;
        }
        catch (Exception unEx)
        {
            return AzureDevopsApiError.FromEx(unEx);
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
            var data = await policy.ExecuteAsync(async () =>
            {
                var res = await req.GetAsync();
                var content = await res.ResponseMessage.Content.ReadAsStringAsync();
                return content;
            });

            return data;
        }
        catch (Exception unEx)
        {
            return AzureDevopsApiError.FromEx(unEx);
        }
    }

    public async Task<AzureDevopsApiResult<SearchResponse>> PostSearch(string searchTerm)
    {
        try
        {
            var client = GetClient();
            var url = $"{Info.SearchApiUrl}/search/codesearchresults?api-version=7.2-preview.1";
            var req = client.Request(url);
            req.WithHeader("Content-Type", "application/json");
            var body = new SearchRequestBody(this.Info.ProjectName)
            {
                searchText = searchTerm
            };
            var bodyJson = JsonSerializer.Serialize(body);

            var data = await policy.ExecuteAsync(async () =>
            {
                var resp = await req.PostStringAsync(bodyJson);
                var data = await resp.GetJsonAsync<SearchResponse>();
                return data;
            });

            return data;
        }
        catch (FlurlHttpException ex)
        {
            return AzureDevopsApiError.FromEx(ex);
        }
    }
}
