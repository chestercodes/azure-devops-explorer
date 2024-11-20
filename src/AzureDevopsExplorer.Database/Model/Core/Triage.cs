using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Core;

public class Triage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string TriageKey { get; set; }
    public string? Category { get; set; }
    public string Message { get; set; }
    public string? Jusitification { get; set; }
    public TriageState State { get; set; }
}

public enum TriageState
{
    New,
    Action,
    Ignore
}
