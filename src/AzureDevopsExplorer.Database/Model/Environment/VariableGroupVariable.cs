namespace AzureDevopsExplorer.Database.Model.Environment;

public class VariableGroupVariable
{
    public long Id { get; set; }
    public int VariableGroupId { get; set; }
    public string Name { get; set; }
    public string? Value { get; set; }
    public bool? IsSecret { get; set; }
}
