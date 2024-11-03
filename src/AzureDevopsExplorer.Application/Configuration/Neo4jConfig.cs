namespace AzureDevopsExplorer.Application.Configuration;

public class Neo4jConfig
{
    public bool LoadData { get; set; }
    public string? Url { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
