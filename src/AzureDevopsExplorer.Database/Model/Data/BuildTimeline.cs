using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

public class BuildTimeline
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int BuildId { get; set; }
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }
    public Guid? LastChangedBy { get; set; }
    public DateTime? LastChangedOn { get; set; }
    public List<BuildTimelineRecord> Records { get; set; }
    public int? ChangeId { get; set; }
    // don't save to db, example is https://dev.azure.com/someorg/some-guid/_apis/build/builds/1234/Timeline/some-guid
    //public string? Url { get; set; }
}
