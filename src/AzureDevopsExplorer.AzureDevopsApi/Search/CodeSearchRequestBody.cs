using System.Text.Json.Serialization;

namespace AzureDevopsExplorer.AzureDevopsApi.Search;

public class CodeSearchRequestBody
{
    public CodeSearchRequestBody(string projectName)
    {
        filters = new Dictionary<string, string[]>
        {
            ["Project"] = new string[] { projectName }
        };
    }
    public string searchText { get; set; }
    [JsonPropertyName("$skip")]
    public int skip { get; set; } = 0;
    [JsonPropertyName("$top")]
    public int top { get; set; } = 1000;
    public Dictionary<string, string[]> filters { get; set; }
    public bool includeFacets { get; set; } = false;
    public bool includeSnippet { get; set; } = false;
}
