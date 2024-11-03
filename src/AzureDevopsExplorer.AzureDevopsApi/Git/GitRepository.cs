namespace AzureDevopsExplorer.AzureDevopsApi.Git;

public class GitRepository
{
    public string id { get; set; }
    public string name { get; set; }
    public string url { get; set; }
    public GitRepositoryProject project { get; set; }
    public string defaultBranch { get; set; }
    public long size { get; set; }
    public string remoteUrl { get; set; }
    public string sshUrl { get; set; }
    public string webUrl { get; set; }
    public bool isDisabled { get; set; }
    public bool isInMaintenance { get; set; }
    public bool isFork { get; set; }
}

public class GitRepositoryProject
{
    public string id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string url { get; set; }
    public string state { get; set; }
    public int revision { get; set; }
    public string visibility { get; set; }
    public DateTime lastUpdateTime { get; set; }
}
