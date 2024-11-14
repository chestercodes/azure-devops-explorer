namespace AzureDevopsExplorer.Database.Model.Core;

public class GitRepositoryChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public Guid RepositoryId { get; set; }
    public string Difference { get; set; }
}
