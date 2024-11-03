namespace AzureDevopsExplorer.AzureDevopsApi;

using AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;
    private readonly string projectName;

    public AzureDevopsApiProjectQueries(AzureDevopsApiProjectClientFactory httpClient, string projectName)
    {
        this.httpClient = httpClient;
        this.projectName = projectName;
    }

    public ApprovalsAndChecks.ProjectQueries ApprovalsAndChecks => new ApprovalsAndChecks.ProjectQueries(httpClient);
    public Build.ProjectQueries Build => new Build.ProjectQueries(httpClient);
    public Core.ProjectQueries Core => new Core.ProjectQueries(httpClient);
    public DistributedTask.ProjectQueries DistributedTask => new DistributedTask.ProjectQueries(httpClient);
    public Environments.ProjectQueries Environments => new Environments.ProjectQueries(httpClient);
    public Git.ProjectQueries Git => new Git.ProjectQueries(httpClient);
    public Pipelines.ProjectQueries Pipelines => new Pipelines.ProjectQueries(httpClient);
    public Search.ProjectQueries Search => new Search.ProjectQueries(httpClient, projectName);
    public Security.ProjectQueries Security => new Security.ProjectQueries(httpClient);
    public ServiceEndpoints.ProjectQueries ServiceEndpoints => new ServiceEndpoints.ProjectQueries(httpClient);
}
