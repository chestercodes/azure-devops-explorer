namespace AzureDevopsExplorer.AzureDevopsApi.Core;

using AzureDevopsExplorer.AzureDevopsApi.Client;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClientFactory;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }
}
