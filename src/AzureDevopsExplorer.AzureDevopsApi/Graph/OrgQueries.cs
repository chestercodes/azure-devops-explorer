﻿namespace AzureDevopsExplorer.AzureDevopsApi.Graph;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.AzureDevopsApi.Pipelines;
using System.Net.Http.Json;

public class OrgQueries
{
    private readonly AzureDevopsApiOrganisationClientFactory httpClient;

    public OrgQueries(AzureDevopsApiOrganisationClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<List<GraphUser>>> GetUsers()
    {
        var client1 = httpClient.VsspsOrganisation();
        return await GetPaginatedListResponse<GraphUser>("graph/users?api-version=7.2-preview.1", client1);
    }

    public async Task<AzureDevopsApiResult<List<GraphGroup>>> GetGroups()
    {
        var client1 = httpClient.VsspsOrganisation();
        return await GetPaginatedListResponse<GraphGroup>("graph/groups?api-version=7.2-preview.1", client1);
    }

    public async Task<AzureDevopsApiResult<ListResponse<GraphGroupMembership>>> GetGroupMemberships(string descriptor)
    {
        var client1 = httpClient.VsspsOrganisation();
        return await client1.GetJson<ListResponse<GraphGroupMembership>>($"graph/Memberships/{descriptor}?direction=Down&api-version=7.2-preview.1");
    }

    public async Task<AzureDevopsApiResult<List<GraphServicePrincipal>>> GetServicePrincipals()
    {
        var client1 = httpClient.VsspsOrganisation();
        return await GetPaginatedListResponse<GraphServicePrincipal>("graph/serviceprincipals?api-version=7.2-preview.1", client1);
    }

    private static async Task<AzureDevopsApiResult<List<TJson>>> GetPaginatedListResponse<TJson>(string path, AzureDevopsApiClient client1)
    {
        var client = client1.GetClient();
        var toReturn = new List<TJson>();
        var carryOn = true;
        string continuationToken = null;
        while (carryOn)
        {
            var conQuery = continuationToken != null ? $"&continuationToken={continuationToken}" : "";
            var url = $"{client1.ApiBase}/{path}{conQuery}";
            try
            {
                var resp = await client.GetAsync(url);
                var data = await resp.Content.ReadFromJsonAsync<ListResponse<TJson>>();

                if (resp.Headers.Contains("x-ms-continuationtoken"))
                {
                    var conTokenHeader = resp.Headers.SingleOrDefault(x => x.Key.ToLower() == "x-ms-continuationtoken");
                    continuationToken = conTokenHeader.Value.First();
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