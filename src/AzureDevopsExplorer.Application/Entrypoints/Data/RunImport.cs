using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class RunImport
{
    public RunImport()
    {
        using (var db = new DataContext())
        {
            if (DatabaseConnection.DbType == DatabaseConnection.DatabaseType.Sqlite)
            {
                db.Database.EnsureCreated();
            }
        }
    }

    public async Task RunOrganisationEntityImport(DataConfig config, AzureDevopsOrganisationDataContext dataContext)
    {
        var agentPoolImport = new AgentPoolImport(dataContext);
        await agentPoolImport.Run(config);

        var securityNamespaceImport = new SecurityNamespacesImport(dataContext);
        await securityNamespaceImport.Run(config);

        var aclImport = new AccessControlListImport(dataContext);
        await aclImport.Run(config);

        var identityImport = new IdentityImportCmd(dataContext);
        await identityImport.Run(config);
    }

    public async Task RunProjectEntityImport(DataConfig config, AzureDevopsProjectDataContext dataContext)
    {
        var addLatestBuilds = new BuildsLatestDefaultFromPipeline(dataContext);
        await addLatestBuilds.Run(config);

        var addAllBuilds = new BuildsAllCompletedFromStart(dataContext);
        await addAllBuilds.Run(config);

        var buildEntitiesImport = new BuildEntitiesImport(dataContext);
        await buildEntitiesImport.Run(config);

        var buildYamlAnalysis = new BuildYamlAnalysis(dataContext);
        await buildYamlAnalysis.Run(config);

        var gitEntitiesImport = new GitEntitiesImport(dataContext);
        await gitEntitiesImport.Run(config);

        var pipelineImportCmd = new PipelineImportCmd(dataContext);
        await pipelineImportCmd.Run(config);

        var latestPipelineTemplate = new LatestPipelineTemplate(dataContext);
        await latestPipelineTemplate.Run(config);

        var serviceEndpointImport = new ServiceEndpointImport(dataContext);
        await serviceEndpointImport.Run(config);

        var secureFileImport = new SecureFileImport(dataContext);
        await secureFileImport.Run(config);

        var variableGroupImport = new VariableGroupImport(dataContext);
        await variableGroupImport.Run(config);

        var pipelineEnvironmentImport = new PipelineEnvironmentImport(dataContext);
        await pipelineEnvironmentImport.Run(config);

        var checkConfigurationImport = new CheckConfigurationImport(dataContext);
        await checkConfigurationImport.Run(config);

        var pipelinePermissionsImport = new PipelinePermissionsImport(dataContext);
        await pipelinePermissionsImport.Run(config);

        var codeSearchImport = new CodeSearchImport(dataContext);
        await codeSearchImport.Run(config);
    }
}
