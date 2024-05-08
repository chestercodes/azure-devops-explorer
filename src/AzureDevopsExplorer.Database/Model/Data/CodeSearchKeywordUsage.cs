namespace AzureDevopsExplorer.Database.Model.Data;

public class CodeSearchKeywordUsage
{
    public long Id { get; set; }
    public string SearchKey { get; set; }
    public string FilePath { get; set; }
    public string RepositoryName { get; set; }
    public Guid RepositoryId { get; set; }
}
