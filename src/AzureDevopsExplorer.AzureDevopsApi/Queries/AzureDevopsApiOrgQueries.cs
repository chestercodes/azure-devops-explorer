namespace AzureDevopsExplorer.AzureDevopsApi;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class AzureDevopsApiOrgQueries
{
    private readonly AzureDevopsApiOrgClient httpClient;

    public AzureDevopsApiOrgQueries(AzureDevopsApiOrgClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<ListResponse<Project>>> GetProjects()
    {
        return await httpClient.GetJson<ListResponse<Project>>($"projects");
    }

    public async Task<AzureDevopsApiResult<ListResponse<SecurityNamespace>>> GetSecurityNamespaces()
    {
        return await httpClient.GetJson<ListResponse<SecurityNamespace>>($"securitynamespaces");
    }

    public async Task<AzureDevopsApiResult<ListResponse<AccessControlList>>> GetAclsForNamespace(Guid namespaceId)
    {
        return await httpClient.GetJson<ListResponse<AccessControlList>>($"accesscontrollists/{namespaceId}");
    }

    public async Task<AzureDevopsApiResult<ListResponse<Identity>>> GetIdentityById(Guid id)
    {
        return await httpClient.GetJsonFromVssps<ListResponse<Identity>>($"identities?querymembership=direct&api-version=7.2-preview.1&descriptors={id}");
    }

    public async Task<AzureDevopsApiResult<ListResponse<Identity>>> GetIdentityByDescriptor(string id)
    {
        return await httpClient.GetJsonFromVssps<ListResponse<Identity>>($"identities?querymembership=direct&api-version=7.2-preview.1&descriptors={id}");
    }

    public async Task<AzureDevopsApiResult<ListResponse<Identity>>> GetIdentityBySubjectDescriptor(string id)
    {
        return await httpClient.GetJsonFromVssps<ListResponse<Identity>>($"identities?querymembership=direct&api-version=7.2-preview.1&subjectDescriptors={id}");
    }

}
