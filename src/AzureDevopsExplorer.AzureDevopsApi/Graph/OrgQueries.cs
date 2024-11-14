namespace AzureDevopsExplorer.AzureDevopsApi.Graph;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

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
        return await client1.GetJsonWithContinuationTokenFromHeader<GraphUser>("graph/users?api-version=7.2-preview.1");
    }

    public async Task<AzureDevopsApiResult<List<GraphGroup>>> GetGroups()
    {
        var client1 = httpClient.VsspsOrganisation();
        return await client1.GetJsonWithContinuationTokenFromHeader<GraphGroup>("graph/groups?api-version=7.2-preview.1");
    }

    public async Task<AzureDevopsApiResult<ListResponse<GraphGroupMembership>>> GetGroupMemberships(string descriptor)
    {
        var client1 = httpClient.VsspsOrganisation();
        return await client1.GetJson<ListResponse<GraphGroupMembership>>($"graph/Memberships/{descriptor}?direction=Down&api-version=7.2-preview.1");
    }

    public async Task<AzureDevopsApiResult<List<GraphServicePrincipal>>> GetServicePrincipals()
    {
        var client1 = httpClient.VsspsOrganisation();
        return await client1.GetJsonWithContinuationTokenFromHeader<GraphServicePrincipal>("graph/serviceprincipals?api-version=7.2-preview.1");
    }
}
