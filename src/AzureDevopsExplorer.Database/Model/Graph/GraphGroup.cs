using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Graph;

public class GraphGroup
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Descriptor { get; set; }

    public string? SubjectKind { get; set; }
    public string? Description { get; set; }
    public bool? IsCrossProject { get; set; }
    public string? Domain { get; set; }
    public string PrincipalName { get; set; }
    public string? MailAddress { get; set; }
    public string? Origin { get; set; }
    public string? OriginId { get; set; }
    public string? DisplayName { get; set; }
    //public string Url { get; set; }
}
