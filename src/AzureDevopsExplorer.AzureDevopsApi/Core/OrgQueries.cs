namespace AzureDevopsExplorer.AzureDevopsApi.Core;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class OrgQueries
{
    private readonly AzureDevopsApiOrganisationClientFactory httpClientFactory;

    public OrgQueries(AzureDevopsApiOrganisationClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<AzureDevopsApiResult<ListResponse<Project>>> GetProjects()
    {
        return await httpClientFactory.Dev().GetJson<ListResponse<Project>>($"projects");
    }

    public async Task<AzureDevopsApiResult<ListResponse<Identity>>> GetIdentityById(Guid id)
    {
        return await httpClientFactory.VsspsOrganisation().GetJson<ListResponse<Identity>>($"identities?querymembership=direct&api-version=7.2-preview.1&descriptors={id}");
    }

    public async Task<AzureDevopsApiResult<ListResponse<Identity>>> GetIdentityByDescriptor(string id)
    {
        return await httpClientFactory.VsspsOrganisation().GetJson<ListResponse<Identity>>($"identities?querymembership=direct&api-version=7.2-preview.1&descriptors={id}");
    }

    public async Task<AzureDevopsApiResult<ListResponse<Identity>>> GetIdentityBySubjectDescriptor(string id)
    {
        return await httpClientFactory.VsspsOrganisation().GetJson<ListResponse<Identity>>($"identities?querymembership=direct&api-version=7.2-preview.1&subjectDescriptors={id}");
    }
}
