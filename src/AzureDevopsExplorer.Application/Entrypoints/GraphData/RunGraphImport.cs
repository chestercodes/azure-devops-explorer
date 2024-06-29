using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Entrypoints.GraphData;
using AzureDevopsExplorer.GraphApi.Client;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class RunGraphImport
{
    public async Task Run(DataConfig config)
    {
        var microsoftClient = new MicrosoftGraphApiClient().Value;

        var graphApplicationsImport = new GraphApplicationsImport(microsoftClient);
        await graphApplicationsImport.Run(config);

        var graphGroupsImport = new GraphGroupsImport(microsoftClient);
        await graphGroupsImport.Run(config);

    }
}
