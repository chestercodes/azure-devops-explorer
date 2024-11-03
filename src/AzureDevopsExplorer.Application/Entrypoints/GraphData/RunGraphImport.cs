using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Entrypoints.GraphData;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.GraphApi.Client;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import;
public class RunGraphImport
{
    private ILoggerFactory getLogger;
    private readonly ICreateDataContexts dataContext;
    private readonly CancellationToken cancellationToken;

    public RunGraphImport(ILoggerFactory getLogger, ICreateDataContexts dataContext, CancellationToken cancellationToken)
    {
        this.getLogger = getLogger;
        this.dataContext = dataContext;
        this.cancellationToken = cancellationToken;
    }

    public async Task Run(ImportConfig config)
    {
        var microsoftClient = new MicrosoftGraphApiClient().Value;

        var graphApplicationsImport = new GraphApplicationsImport(microsoftClient, getLogger, dataContext);
        await graphApplicationsImport.Run(config);

        var graphGroupsImport = new GraphGroupsImport(microsoftClient, getLogger, dataContext);
        await graphGroupsImport.Run(config);

    }
}
