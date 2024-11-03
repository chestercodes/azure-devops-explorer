namespace AzureDevopsExplorer.AzureDevopsApi.Audit;

using AzureDevopsExplorer.AzureDevopsApi.Client;

public class OrgQueries
{
    private readonly AzureDevopsApiOrganisationClientFactory httpClient;

    public OrgQueries(AzureDevopsApiOrganisationClientFactory httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AzureDevopsApiResult<AuditLogResponse>> GetAuditLog(string startTime, string endTime, string continuationToken = null)
    {
        var url = $"audit/auditlog?api-version=7.2-preview.1&startTime={startTime}&endTime={endTime}";
        if (continuationToken != null)
        {
            url = url + $"&continuationToken={continuationToken}";
        }
        return await httpClient.AuditOrganisation().GetJson<AuditLogResponse>(url);
    }
}
