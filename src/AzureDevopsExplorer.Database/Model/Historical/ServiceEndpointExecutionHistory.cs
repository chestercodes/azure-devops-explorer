using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Historical;
/// <summary>
/// The api doesn't return the pipeline rev
/// so can't be totally sure which one it is to normalise the pipeline name
/// </summary>
[PrimaryKey(nameof(Id))]
public class ServiceEndpointExecutionHistory
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }
    public Guid EndpointId { get; set; }
    [StringLength(Constants.PlanTypeLength)]
    public string? PlanType { get; set; }
    public int? DefinitionId { get; set; }
    [StringLength(Constants.PipelineNameLength)]
    public string? DefinitionName { get; set; }
    public int? OwnerId { get; set; }
    [StringLength(Constants.BuildNameLength)]
    public string? OwnerName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? FinishTime { get; set; }
    public ServiceConnectionResult? Result { get; set; }
    public string? OwnerDetails { get; set; }
}

public enum ServiceConnectionResult
{
    Abandoned,
    Canceled,
    Failed,
    Skipped,
    Succeeded,
    SucceededWithIssues
}

//public class Definition
//{
//    public _Links _links { get; set; }
//    public int id { get; set; }
//    public string name { get; set; }
//}

//public class _Links
//{
//    public Web web { get; set; }
//    public Self self { get; set; }
//}

//public class Web
//{
//    public string href { get; set; }
//}

//public class Self
//{
//    public string href { get; set; }
//}

//public class Owner
//{
//    public _Links1 _links { get; set; }
//    public int id { get; set; }
//    public string name { get; set; }
//}

//public class _Links1
//{
//    public Web1 web { get; set; }
//    public Self1 self { get; set; }
//}

//public class Web1
//{
//    public string href { get; set; }
//}

//public class Self1
//{
//    public string href { get; set; }
//}
