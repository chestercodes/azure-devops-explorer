namespace AzureDevopsExplorer.AzureDevopsApi.DistributedTask;

public class SecureFile
{
    public Guid id { get; set; }
    public string? name { get; set; }
    public SecureFileIdentity? createdBy { get; set; }
    public DateTime? createdOn { get; set; }
    public SecureFileIdentity? modifiedBy { get; set; }
    public DateTime? modifiedOn { get; set; }
    public Dictionary<string, string> properties { get; set; } = new Dictionary<string, string>();
}

public class SecureFileIdentity
{
    //public string? displayName { get; set; }
    //public string url { get; set; }
    public Guid id { get; set; }
    //public string? uniqueName { get; set; }
    //public string? imageUrl { get; set; }
    //public string? descriptor { get; set; }
}
