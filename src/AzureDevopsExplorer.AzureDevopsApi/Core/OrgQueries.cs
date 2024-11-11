namespace AzureDevopsExplorer.AzureDevopsApi.Core;

using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class OrgQueries
{
    public enum IdentitiesQueryMembership
    {
        Direct,
        Expanded
    }

    private readonly AzureDevopsApiOrganisationClientFactory httpClientFactory;

    public OrgQueries(AzureDevopsApiOrganisationClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<AzureDevopsApiResult<ListResponse<Project>>> GetProjects()
    {
        return await httpClientFactory.Dev().GetJson<ListResponse<Project>>($"projects");
    }

    public async Task<AzureDevopsApiResult<ListResponse<Identity>>> GetIdentitiesById(List<Guid> ids, IdentitiesQueryMembership membership = IdentitiesQueryMembership.Direct)
    {
        var asCsv = string.Join(",", ids);
        var membershipQueryParam = GetMembershipParam(membership);
        var path = $"identities?{membershipQueryParam}&api-version=7.2-preview.1&descriptors={asCsv}";
        return await httpClientFactory.VsspsOrganisation().GetJson<ListResponse<Identity>>(path);
    }

    public async Task<AzureDevopsApiResult<ListResponse<Identity>>> GetIdentitiesByDescriptor(List<string> ids, IdentitiesQueryMembership membership = IdentitiesQueryMembership.Direct)
    {
        var asCsv = string.Join(",", ids);
        var membershipQueryParam = GetMembershipParam(membership);
        var path = $"identities?{membershipQueryParam}&api-version=7.2-preview.1&descriptors={asCsv}";
        return await httpClientFactory.VsspsOrganisation().GetJson<ListResponse<Identity>>(path);
    }

    public async Task<AzureDevopsApiResult<ListResponse<Identity>>> GetIdentitiesBySubjectDescriptor(List<string> ids, IdentitiesQueryMembership membership = IdentitiesQueryMembership.Direct)
    {
        var asCsv = string.Join(",", ids);
        var membershipQueryParam = GetMembershipParam(membership);
        var path = $"identities?{membershipQueryParam}&api-version=7.2-preview.1&subjectDescriptors={asCsv}";
        return await httpClientFactory.VsspsOrganisation().GetJson<ListResponse<Identity>>(path);
    }

    private static string GetMembershipParam(IdentitiesQueryMembership membership)
    {
        if (membership == IdentitiesQueryMembership.Direct)
        {
            return $"queryMembership=direct";
        }
        if (membership == IdentitiesQueryMembership.Expanded)
        {
            return $"queryMembership=expanded";
        }

        throw new NotImplementedException("Membership type is not implemented " + membership.ToString());
    }
}
