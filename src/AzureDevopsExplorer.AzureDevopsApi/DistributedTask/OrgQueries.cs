namespace AzureDevopsExplorer.AzureDevopsApi.DistributedTask;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class OrgQueries
{
    private readonly AzureDevopsApiOrganisationClientFactory httpClient;

    public OrgQueries(AzureDevopsApiOrganisationClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<ListResponse<AgentPool>>> GetAgentPools()
    {
        return await httpClient.Dev().GetJson<ListResponse<AgentPool>>($"distributedtask/pools?api-version=7.2-preview.1");
    }
}
