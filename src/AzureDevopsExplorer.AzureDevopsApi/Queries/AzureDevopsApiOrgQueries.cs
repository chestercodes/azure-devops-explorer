namespace AzureDevopsExplorer.AzureDevopsApi;

using AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiOrgQueries
{
    private readonly AzureDevopsApiOrganisationClientFactory httpClient;
    private readonly CancellationToken cancellationToken;

    public AzureDevopsApiOrgQueries(AzureDevopsApiOrganisationClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }
    public Audit.OrgQueries Audit => new Audit.OrgQueries(httpClient);
    public Core.OrgQueries Core => new Core.OrgQueries(httpClient);
    public DistributedTask.OrgQueries DistributedTask => new DistributedTask.OrgQueries(httpClient);
    public Graph.OrgQueries Graph => new Graph.OrgQueries(httpClient);
    public Security.OrgQueries Security => new Security.OrgQueries(httpClient);
}
