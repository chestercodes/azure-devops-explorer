namespace AzureDevopsExplorer.AzureDevopsApi.ServiceEndpoints;

public class ServiceEndpoint
{
    public Dictionary<string, string>? data { get; set; }
    public string? id { get; set; }
    public string? name { get; set; }
    public string? type { get; set; }
    public string? url { get; set; }
    public ServiceEndpointIdentityRef? createdBy { get; set; }
    public string? description { get; set; }
    public Authorization? authorization { get; set; }
    public bool? isShared { get; set; }
    public bool? isOutdated { get; set; }
    public bool? isReady { get; set; }
    public string? owner { get; set; }
    public Serviceendpointprojectreference[]? serviceEndpointProjectReferences { get; set; }
    public Operationstatus? operationStatus { get; set; }
}

public class ServiceEndpointIdentityRef
{
    public string? displayName { get; set; }
    public string? url { get; set; }
    //[JsonProperty("XXXXXXX")]
    //public _Links _links { get; set; }
    public string id { get; set; }
    public string? uniqueName { get; set; }
    public string? imageUrl { get; set; }
    public string? descriptor { get; set; }
}

//public class _Links
//{
//    public Avatar avatar { get; set; }
//}

//public class Avatar
//{
//    public string href { get; set; }
//}

public class Authorization
{
    public Dictionary<string, string>? parameters { get; set; }
    public string? scheme { get; set; }
}

public class Operationstatus
{
    public string state { get; set; }
    public string statusMessage { get; set; }
}

public class Serviceendpointprojectreference
{
    public Projectreference? projectReference { get; set; }
    public string? name { get; set; }
    public string? description { get; set; }
}

public class Projectreference
{
    public string? id { get; set; }
    public string? name { get; set; }
}
