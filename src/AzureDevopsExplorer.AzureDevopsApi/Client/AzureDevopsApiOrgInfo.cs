namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiOrgInfo
{
    public string? BearerToken { get; set; }
    public string OrgName { get; set; }

    public string ApiUrl => $"https://dev.azure.com/{OrgName}/_apis";
    public string AuditApiUrl => $"https://auditservice.dev.azure.com/{OrgName}/_apis";
    public string VsspsApiUrl => $"https://vssps.dev.azure.com/{OrgName}/_apis";

    public AzureDevopsApiProjectInfo AsProjectInfo(string projectName, Guid projectId)
    {
        return new AzureDevopsApiProjectInfo
        {
            BearerToken = this.BearerToken,
            OrgName = this.OrgName,
            ProjectName = projectName,
            ProjectId = projectId
        };
    }
}
