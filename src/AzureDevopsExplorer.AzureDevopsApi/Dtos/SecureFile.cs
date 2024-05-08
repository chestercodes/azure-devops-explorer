using Newtonsoft.Json;

namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class SecureFile
{
    [JsonProperty("id")]
    public Guid Id { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
    [JsonProperty("createdBy")]
    public SecureFileIdentity? CreatedBy { get; set; }
    [JsonProperty("createdOn")]
    public DateTime? CreatedOn { get; set; }
    [JsonProperty("modifiedBy")]
    public SecureFileIdentity? ModifiedBy { get; set; }
    [JsonProperty("modifiedOn")]
    public DateTime? ModifiedOn { get; set; }
    [JsonProperty("properties")]
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}

public class SecureFileIdentity
{
    //public string? displayName { get; set; }
    //public string url { get; set; }
    [JsonProperty("id")]
    public Guid Id { get; set; }
    //public string? uniqueName { get; set; }
    //public string? imageUrl { get; set; }
    //public string? descriptor { get; set; }
}
