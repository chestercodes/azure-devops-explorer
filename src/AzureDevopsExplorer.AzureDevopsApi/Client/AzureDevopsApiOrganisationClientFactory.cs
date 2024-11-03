using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiOrganisationClientFactory
{
    private readonly string organisation;
    private readonly ILoggerFactory loggerFactory;
    private readonly IGetAuthHeader azureDevopsApiAuthHeader;
    private readonly CancellationToken cancellationToken;

    public AzureDevopsApiOrganisationClientFactory(string organisation, ILoggerFactory loggerFactory, IGetAuthHeader azureDevopsApiAuthHeader, CancellationToken cancellationToken)
    {
        this.organisation = organisation;
        this.loggerFactory = loggerFactory;
        this.azureDevopsApiAuthHeader = azureDevopsApiAuthHeader;
        this.cancellationToken = cancellationToken;
    }

    public AzureDevopsApiClient Dev()
    {
        var url = new ApiBase($"https://dev.azure.com/{organisation}/_apis");
        return new AzureDevopsApiClient(azureDevopsApiAuthHeader, url, loggerFactory, cancellationToken);
    }

    public AzureDevopsApiClient VsspsOrganisation()
    {
        var url = new ApiBase($"https://vssps.dev.azure.com/{organisation}/_apis");
        return new AzureDevopsApiClient(azureDevopsApiAuthHeader, url, loggerFactory, cancellationToken);
    }

    public AzureDevopsApiClient AuditOrganisation()
    {
        var url = new ApiBase($"https://auditservice.dev.azure.com/{organisation}/_apis");
        return new AzureDevopsApiClient(azureDevopsApiAuthHeader, url, loggerFactory, cancellationToken);
    }
}
