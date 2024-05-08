using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Entrypoints.Evaluate;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class RunImport
{
    private readonly AzureDevopsApiOrgClient httpOrgClient;
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly VssConnection connection;
    private readonly string projectName;

    public RunImport(AzureDevopsApiOrgClient httpOrgClient, VssConnection connection, string projectName)
    {
        this.httpClient = new AzureDevopsApiProjectClient(httpOrgClient.Info.AsProjectInfo(projectName));
        this.httpOrgClient = httpOrgClient;
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

        var secureFileImport = new SecureFileImport(httpClient);
        await secureFileImport.Run(config);

        var variableGroupImport = new VariableGroupImport(httpClient);
        await variableGroupImport.Run(config);

        var pipelineEnvironmentImport = new PipelineEnvironmentImport(httpClient);
        await pipelineEnvironmentImport.Run(config);

        var checkConfigurationImport = new CheckConfigurationImport(httpClient);
        await checkConfigurationImport.Run(config);

        var securityNamespaceImport = new SecurityNamespacesImport(httpOrgClient);
        await securityNamespaceImport.Run(config);

        var aclImport = new AccessControlListImport(httpOrgClient);
        await aclImport.Run(config);

        var identityImport = new IdentityImportCmd(httpOrgClient);
        await identityImport.Run(config);

        var codeSearchImport = new CodeSearchImport(httpClient);
        await codeSearchImport.Run(config);

        // todo, find a better place?
        var deriveResourcePermissions = new DeriveResourcePermissions();
        await deriveResourcePermissions.Run();
    }
}
