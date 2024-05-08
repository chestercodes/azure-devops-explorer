namespace AzureDevopsExplorer.AzureDevopsApi.Client;

public class AzureDevopsApiProjectInfo
{
    public string? BearerToken { get; set; }
    //public string? PatToken { get; set; }
    public string? ProjectName { get; set; }
    public string OrgName { get; set; }

    public string ApiUrl => $"https://dev.azure.com/{OrgName}/{ProjectName}/_apis";
    public string SearchApiUrl => $"https://almsearch.dev.azure.com/{OrgName}/{ProjectName}/_apis";
}
