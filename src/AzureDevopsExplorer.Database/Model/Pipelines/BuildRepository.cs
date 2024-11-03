using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

public class BuildRepository
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public System.Uri? Url { get; set; }
    public string? DefaultBranch { get; set; }
    public string? RootFolder { get; set; }
    public string? Clean { get; set; }
    public bool? CheckoutSubmodules { get; set; }
    //public IDictionary<string, string> Properties { get; set; }
}
