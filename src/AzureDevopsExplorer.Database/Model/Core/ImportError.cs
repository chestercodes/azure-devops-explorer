using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Core;

public class ImportError
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    [StringLength(32)]
    public string Hash { get; set; }
    public string? Error { get; set; }
}