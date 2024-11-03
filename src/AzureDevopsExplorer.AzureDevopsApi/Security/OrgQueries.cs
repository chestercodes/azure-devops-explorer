namespace AzureDevopsExplorer.AzureDevopsApi.Security;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class OrgQueries
{
    private readonly AzureDevopsApiOrganisationClientFactory httpClient;

    public OrgQueries(AzureDevopsApiOrganisationClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<ListResponse<SecurityNamespace>>> GetSecurityNamespaces()
    {
        return await httpClient.Dev().GetJson<ListResponse<SecurityNamespace>>($"securitynamespaces");
    }

    public async Task<AzureDevopsApiResult<ListResponse<AccessControlList>>> GetAclsForNamespace(Guid namespaceId)
    {
        return await httpClient.Dev().GetJson<ListResponse<AccessControlList>>($"accesscontrollists/{namespaceId}");
    }
}
