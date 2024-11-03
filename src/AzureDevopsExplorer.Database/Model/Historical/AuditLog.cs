using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Historical;

public class AuditLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; }

    public string? CorrelationId { get; set; }
    public string? ActivityId { get; set; }
    public string? ActorCUID { get; set; }
    public string? ActorUserId { get; set; }
    public string? ActorClientId { get; set; }
    public string? ActorUPN { get; set; }
    public string? AuthenticationMechanism { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ScopeType { get; set; }
    public string? ScopeDisplayName { get; set; }
    public string? ScopeId { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? ActionId { get; set; }
    public string Data { get; set; }
    public string? Details { get; set; }
    public string? Area { get; set; }
    public string? Category { get; set; }
    public string? CategoryDisplayName { get; set; }
    public string? ActorDisplayName { get; set; }
    public string? ActorImageUrl { get; set; }
}
