using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.Database.Model.Data;

// Microsoft.TeamFoundation.SourceControl.WebApi.GitRepository
public class GitRepository
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public Guid? ProjectReferenceId { get; set; }
    public string DefaultBranch { get; set; }
    public long? Size { get; set; }
    public string RemoteUrl { get; set; }
    public string SshUrl { get; set; }
    public string WebUrl { get; set; }
    //public string[] ValidRemoteUrls { get; set; }
    public bool IsFork { get; set; }
    public Guid? ParentRepositoryId { get; set; }
    //public ReferenceLinks Links { get; set; }
    public bool? IsDisabled { get; set; }
}
