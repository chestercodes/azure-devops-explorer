using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Entrypoints.Import.Core;
using AzureDevopsExplorer.Application.Entrypoints.Import.Environment;
using AzureDevopsExplorer.Application.Entrypoints.Import.Graph;
using AzureDevopsExplorer.Application.Entrypoints.Import.Historical;
using AzureDevopsExplorer.Application.Entrypoints.Import.Pipelines;
using AzureDevopsExplorer.Application.Entrypoints.Import.Security;

namespace AzureDevopsExplorer.Application.Entrypoints.Import;
public class RunImport
{
    public async Task RunOrganisationEntityImport(ImportConfig config, AzureDevopsOrganisationDataContext dataContext)
    {
        var projectImport = new ProjectsImport(dataContext);
        await projectImport.Run(config);

        var agentPoolImport = new AgentPoolImport(dataContext);
        await agentPoolImport.Run(config);

        var securityNamespaceImport = new SecurityNamespacesImport(dataContext);
        await securityNamespaceImport.Run(config);

        var aclImport = new AccessControlListImport(dataContext);
        await aclImport.Run(config);

        var auditLogImport = new AuditLogImport(dataContext);
        await auditLogImport.Run(config);

        var graphGroupsImport = new GraphGroupsImport(dataContext);
        await graphGroupsImport.Run(config);

        var graphUsersImport = new GraphUserImport(dataContext);
        await graphUsersImport.Run(config);

        var graphSpImport = new GraphServicePrincipalImport(dataContext);
        await graphSpImport.Run(config);

        var identityImport = new IdentitiesImport(dataContext);
        await identityImport.Run(config);
    }

    public async Task RunProjectEntityImport(ImportConfig config, AzureDevopsProjectDataContext dataContext)
    {
        var policyConfigurationImport = new PolicyConfigurationImport(dataContext);
        await policyConfigurationImport.Run(config);

        var pipelinesImport = new CurrentPipelinesImport(dataContext);
        await pipelinesImport.Run(config);

        var addLatestBuilds = new BuildsLatestDefaultFromPipeline(dataContext);
        await addLatestBuilds.Run(config);

        var addAllBuilds = new BuildsAllCompletedFromStart(dataContext);
        await addAllBuilds.Run(config);

        var buildEntitiesImport = new BuildEntitiesImport(dataContext);
        await buildEntitiesImport.Run(config);

        var pipelineRunImport = new PipelineRunImport(dataContext);
        await pipelineRunImport.Run(config);

        var buildYamlAnalysis = new BuildPipelineRunExpandedYamlAnalysis(dataContext);
        await buildYamlAnalysis.Run(config);

        var gitEntitiesImport = new GitEntitiesImport(dataContext);
        await gitEntitiesImport.Run(config);

        var pipelineImportCmd = new PipelineImportCmd(dataContext);
        await pipelineImportCmd.Run(config);

        var latestPipelineTemplate = new PipelineConfigurationYamlImport(dataContext);
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
