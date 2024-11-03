namespace AzureDevopsExplorer.AzureDevopsApi.Build;

public class BuildArtifact
{
    public int id { get; set; }
    public string name { get; set; }
    public string source { get; set; }
    public Resource resource { get; set; }
}

public class Resource
{
    public string type { get; set; }
    public string data { get; set; }
    public Dictionary<string, string> properties { get; set; }
    public string url { get; set; }
    public string downloadUrl { get; set; }
}
