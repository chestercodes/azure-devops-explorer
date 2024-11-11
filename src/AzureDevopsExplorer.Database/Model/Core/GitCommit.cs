using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

public class GitCommit
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string CommitId { get; set; }
    public Guid RepositoryId { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorEmail { get; set; }
    public DateTime? AuthorDate { get; set; }
    public string? CommitterName { get; set; }
    public string? CommitterEmail { get; set; }
    public DateTime? CommitterDate { get; set; }
    public string? Comment { get; set; }
    public bool? CommentTruncated { get; set; }
    public int? ChangeCountsAdd { get; set; }
    public int? ChangeCountsEdit { get; set; }
    public int? ChangeCountsDelete { get; set; }
    public string? RemoteUrl { get; set; }
}
