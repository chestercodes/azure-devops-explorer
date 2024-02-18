using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(Id), nameof(Revision))]
public class Pipeline
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public int Revision { get; set; }
    [StringLength(Constants.PipelineNameLength)]
    public string? Name { get; set; }
    [StringLength(Constants.PipelineNameLength)]
    public string? Folder { get; set; }
    [StringLength(1000)]
    public string? ConfigurationPath { get; set; }
    [StringLength(50)]
    public string? ConfigurationType { get; set; }
    [StringLength(1000)]
    public Guid? ConfigurationRepositoryId { get; set; }
    [StringLength(50)]
    public string? ConfigurationRepositoryType { get; set; }
    //public Dictionary<string, Link> Links { get; set; }
    // not that interesting to store https://dev.azure.com/someorg/proj-guid/_apis/pipelines/23?revision=20
    // public string? Url { get; set; }
}
