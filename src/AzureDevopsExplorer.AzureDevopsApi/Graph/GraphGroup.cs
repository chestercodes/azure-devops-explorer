namespace AzureDevopsExplorer.AzureDevopsApi.Graph;

public class GraphGroup
{
    public string subjectKind { get; set; }
    public string description { get; set; }
    public bool isCrossProject { get; set; }
    public string domain { get; set; }
    public string principalName { get; set; }
    public string mailAddress { get; set; }
    public string origin { get; set; }
    public string originId { get; set; }
    public string displayName { get; set; }
    public string url { get; set; }
    public string descriptor { get; set; }
}
