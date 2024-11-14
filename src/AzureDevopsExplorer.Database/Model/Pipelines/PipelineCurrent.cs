using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(Id))]
public class PipelineCurrent
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public int Revision { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; }
    public string Folder { get; set; }
}
