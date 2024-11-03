using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiProjectClientFactory
{
    private readonly string organisation;
    private readonly string projectName;
    private readonly ILoggerFactory loggerFactory;
    private readonly IGetAuthHeader azureDevopsApiAuthHeader;
    private readonly CancellationToken cancellationToken;

    public AzureDevopsApiProjectClientFactory(string organisation, string projectName, ILoggerFactory loggerFactory, IGetAuthHeader azureDevopsApiAuthHeader, CancellationToken cancellationToken)
    {
        this.organisation = organisation;
        this.projectName = projectName;
        this.loggerFactory = loggerFactory;
        this.azureDevopsApiAuthHeader = azureDevopsApiAuthHeader;
        this.cancellationToken = cancellationToken;
    }

    public AzureDevopsApiClient DevProject()
    {
        var url = new ApiBase($"https://dev.azure.com/{organisation}/{projectName}/_apis");
        return new AzureDevopsApiClient(azureDevopsApiAuthHeader, url, loggerFactory, cancellationToken);
    }

    public AzureDevopsApiClient SearchProject()
    {
        var url = new ApiBase($"https://almsearch.dev.azure.com/{organisation}/{projectName}/_apis");
        return new AzureDevopsApiClient(azureDevopsApiAuthHeader, url, loggerFactory, cancellationToken);
    }
}
