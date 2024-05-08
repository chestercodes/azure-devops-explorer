using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

public class SecurityNamespace
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid NamespaceId { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? SeparatorValue { get; set; }
    public int? ElementLength { get; set; }
    public int? WritePermission { get; set; }
    public int? ReadPermission { get; set; }
    public string? DataspaceCategory { get; set; }
    //public Action[] Actions { get; set; }
    public int? StructureValue { get; set; }
    public string? ExtensionType { get; set; }
    public bool? IsRemotable { get; set; }
    public bool? UseTokenTranslator { get; set; }
    public int? SystemBitMask { get; set; }
}
