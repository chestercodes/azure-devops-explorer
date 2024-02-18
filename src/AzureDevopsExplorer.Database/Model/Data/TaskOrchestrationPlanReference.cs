using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Data;

public class TaskOrchestrationPlanReference
{
    [Key]
    public int Id { get; set; }
    public int BuildId { get; set; }
    public Guid PlanId { get; set; }
    public int? OrchestrationType { get; set; }
}
