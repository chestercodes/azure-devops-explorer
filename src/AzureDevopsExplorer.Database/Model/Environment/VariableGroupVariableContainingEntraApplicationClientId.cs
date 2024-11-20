namespace AzureDevopsExplorer.Database.Model.Environment;

public class VariableGroupVariableContainingEntraApplicationClientId
{
    public long Id { get; set; }
    public int VariableGroupId { get; set; }
    public string VariableName { get; set; }
    public Guid ApplicationId { get; set; }
    public Guid ApplicationClientId { get; set; }
    public string ApplicationDisplayName { get; set; }
}
