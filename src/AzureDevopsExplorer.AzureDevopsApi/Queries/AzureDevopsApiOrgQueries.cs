namespace AzureDevopsExplorer.AzureDevopsApi;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class AzureDevopsApiOrgQueries
{
    private readonly AzureDevopsApiOrgClient httpClient;

    public AzureDevopsApiOrgQueries(AzureDevopsApiOrgClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<ListResponse<Project>>> GetProjects()
    {
        return await httpClient.GetJson<ListResponse<Project>>($"projects");
    }
}
