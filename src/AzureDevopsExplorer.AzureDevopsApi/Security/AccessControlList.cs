namespace AzureDevopsExplorer.AzureDevopsApi.Security;

public class AccessControlList
{
    public bool inheritPermissions { get; set; }
    public string token { get; set; }
    public Dictionary<string, AccessControlListInfo> acesDictionary { get; set; }
}

public class AccessControlListInfo
{
    public string descriptor { get; set; }
    public int allow { get; set; }
    public int deny { get; set; }
}

