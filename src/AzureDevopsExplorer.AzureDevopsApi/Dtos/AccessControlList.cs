using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class AccessControlList
{
    [JsonProperty("inheritPermissions")]
    public bool InheritPermissions { get; set; }
    [JsonProperty("token")]
    public string Token { get; set; }
    [JsonProperty("acesDictionary")]
    public Dictionary<string, AccessControlListInfo> AcesDictionary { get; set; }
}


public class AccessControlListInfo
{
    [JsonProperty("descriptor")]
    public string Descriptor { get; set; }
    [JsonProperty("allow")]
    public int Allow { get; set; }
    [JsonProperty("deny")]
    public int Deny { get; set; }
}

