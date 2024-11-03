namespace AzureDevopsExplorer.Database.Model.Environment;

public class VariableGroupProjectReference
{
    public long Id { get; set; }
    public int VariableGroupId { get; set; }
    public string Name { get; set; }
    public string ProjectReferenceName { get; set; }
    public Guid ProjectReferenceId { get; set; }
}
