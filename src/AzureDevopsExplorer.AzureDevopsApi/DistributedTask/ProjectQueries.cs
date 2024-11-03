namespace AzureDevopsExplorer.AzureDevopsApi.DistributedTask;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<ListResponse<VariableGroup>>> GetVariableGroups()
    {
        return await httpClient.DevProject().GetJson<ListResponse<VariableGroup>>($"distributedtask/variablegroups");
    }

    public async Task<AzureDevopsApiResult<VariableGroup>> GetVariableGroup(int id)
    {
        return await httpClient.DevProject().GetJson<VariableGroup>($"distributedtask/variablegroups/{id}");
    }

    public async Task<AzureDevopsApiResult<ListResponse<SecureFile>>> GetSecureFiles()
    {
        return await httpClient.DevProject().GetJson<ListResponse<SecureFile>>($"distributedtask/securefiles");
    }
}
