using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(Id))]
public class LatestPipeline
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public int Revision { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
}
