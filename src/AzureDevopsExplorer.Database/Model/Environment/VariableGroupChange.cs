namespace AzureDevopsExplorer.Database.Model.Environment;

public class VariableGroupChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public int VariableGroupId { get; set; }
    public string Difference { get; set; }
}
