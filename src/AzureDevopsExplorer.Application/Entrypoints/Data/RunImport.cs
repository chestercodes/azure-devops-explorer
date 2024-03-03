using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class RunImport
{
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly VssConnection connection;
    private readonly string projectName;

    public RunImport(AzureDevopsApiProjectClient httpClient, VssConnection connection, string projectName)
    {
        this.httpClient = httpClient;
        this.connection = connection;
        this.projectName = projectName;
    }

    public async Task Run(DataConfig config)
    {
        using (var db = new DataContext())
        {
            if (DatabaseConnection.DbType == DatabaseConnection.DatabaseType.Sqlite)
            {
                db.Database.EnsureCreated();
            }
        }

        var addLatestBuilds = new BuildsLatestDefaultFromPipeline(connection, httpClient, projectName);
        await addLatestBuilds.Run(config);

        var addAllBuilds = new BuildsAllCompletedFromStart(connection, projectName);
        await addAllBuilds.Run(config);

        var buildEntitiesImport = new BuildEntitiesImport(connection, httpClient, projectName);
        await buildEntitiesImport.Run(config);

        var buildYamlAnalysis = new BuildYamlAnalysis(connection, projectName);
        await buildYamlAnalysis.Run(config);

        var gitEntitiesImport = new GitEntitiesImport(connection, projectName);
        await gitEntitiesImport.Run(config);

        var pipelineImportCmd = new PipelineImportCmd(httpClient);
        await pipelineImportCmd.Run(config);

        var latestPipelineTemplate = new LatestPipelineTemplate(httpClient);
        await latestPipelineTemplate.Run(config);

        var serviceEndpointImport = new ServiceEndpointImport(httpClient);
        await serviceEndpointImport.Run(config);

        var variableGroupImport = new VariableGroupImport(httpClient);
        await variableGroupImport.Run(config);

        var pipelineEnvironmentImport = new PipelineEnvironmentImport(httpClient);
        await pipelineEnvironmentImport.Run(config);

        var checkConfigurationImport = new CheckConfigurationImport(httpClient);
        await checkConfigurationImport.Run(config);

        var identityImport = new IdentityImportCmd(connection, projectName);
        await identityImport.Run(config);

    }
}
