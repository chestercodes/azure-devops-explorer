namespace AzureDevopsExplorer.AzureDevopsApi.ApprovalsAndChecks;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class ProjectQueries
{
    private readonly AzureDevopsApiProjectClientFactory httpClient;

    public ProjectQueries(AzureDevopsApiProjectClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public record CheckConfigurationsQueryResource(string id, string name, string type);
    public async Task<AzureDevopsApiResult<ListResponse<CheckConfiguration>>> CheckConfigurationsQuery(CheckConfigurationsQueryResource[] resources)
    {
        return await httpClient.DevProject().PostJson<ListResponse<CheckConfiguration>>($"pipelines/checks/queryconfigurations?$expand=settings&api-version=7.2-preview.1", resources);
    }

    public async Task<AzureDevopsApiResult<PipelineResourceApproval>> GetPipelineApprovedServiceEndpoints(Guid serviceEndpointId)
    {
        return await httpClient.DevProject().GetJson<PipelineResourceApproval>($"pipelines/pipelinePermissions/endpoint/{serviceEndpointId}");
    }

    public async Task<AzureDevopsApiResult<PipelineResourceApproval>> GetPipelineApprovedEnvironments(int environmentId)
    {
        return await httpClient.DevProject().GetJson<PipelineResourceApproval>($"pipelines/pipelinePermissions/environment/{environmentId}");
    }

    public async Task<AzureDevopsApiResult<PipelineResourceApproval>> GetPipelineApprovedVariableGroup(int variableGroupId)
    {
        return await httpClient.DevProject().GetJson<PipelineResourceApproval>($"pipelines/pipelinePermissions/variablegroup/{variableGroupId}");
    }
}
