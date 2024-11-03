namespace AzureDevopsExplorer.Database.Model.Environment;

public class CodeSearchKeyword
{
    public long Id { get; set; }
    public string SearchKey { get; set; }
    public string SearchTerm { get; set; }
    // null ProjectId means search all projects
    public Guid? ProjectId { get; set; }
}
