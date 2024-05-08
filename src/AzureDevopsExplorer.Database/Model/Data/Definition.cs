using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace AzureDevopsExplorer.Database.Model.Data;

// Microsoft.TeamFoundation.Build.WebApi.DefinitionReference
[PrimaryKey(nameof(Id), nameof(Revision))]
public class Definition
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public int Revision { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; }
    public string? Url { get; set; }
    public Uri? Uri { get; set; }
    public string Path { get; set; }
    public DefinitionType Type { get; set; }
    public DefinitionQueueStatus QueueStatus { get; set; }
    public DateTime CreatedDate { get; set; }
}

public enum DefinitionType
{
    [EnumMember]
    Xaml = 1,
    [EnumMember]
    Build
}

public enum DefinitionQueueStatus
{
    Enabled,
    Paused,
    Disabled,
}