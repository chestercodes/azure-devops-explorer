namespace AzureDevopsExplorer.AzureDevopsApi.Search;

using AzureDevopsExplorer.AzureDevopsApi.Client;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;
    private readonly string ProjectName;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClient, string projectName)
    {
        this.httpClient = httpClient;
        ProjectName = projectName;
    }

    public async Task<AzureDevopsApiResult<CodeSearchResponse>> CodeSearch(string searchTerm)
    {
        var body = new CodeSearchRequestBody(ProjectName)
        {
            searchText = searchTerm
        };
        return await httpClient.SearchProject().PostJson<CodeSearchResponse>("search/codesearchresults?api-version=7.2-preview.1", body);
    }
}
