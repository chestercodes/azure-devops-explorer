namespace AzureDevopsExplorer.AzureDevopsApi.Policy;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<ListResponse<PolicyConfiguration>>> GetAllPolicyConfigurations()
    {
        return await httpClient.DevProject().GetJson<ListResponse<PolicyConfiguration>>($"policy/configurations?api-version=7.2-preview.1");
    }
}
