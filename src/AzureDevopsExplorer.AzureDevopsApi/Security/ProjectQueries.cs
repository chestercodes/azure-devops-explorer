namespace AzureDevopsExplorer.AzureDevopsApi.Security;

using AzureDevopsExplorer.AzureDevopsApi.Client;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }
}
