namespace AzureDevopsExplorer.AzureDevopsApi.Dtos;

public class AuditLogResponse
{
    public DecoratedAuditLogEntry[] decoratedAuditLogEntries { get; set; }
    public string continuationToken { get; set; }
    public bool hasMore { get; set; }
}

public class DecoratedAuditLogEntry
{
    public string id { get; set; }
    public string? correlationId { get; set; }
    public string? activityId { get; set; }
    public string? actorCUID { get; set; }
    public string? actorUserId { get; set; }
    public string? actorClientId { get; set; }
    public string? actorUPN { get; set; }
    public string? authenticationMechanism { get; set; }
    public DateTime? timestamp { get; set; }
    public string? scopeType { get; set; }
    public string? scopeDisplayName { get; set; }
    public string? scopeId { get; set; }
    public string? projectId { get; set; }
    public string? projectName { get; set; }
    public string? ipAddress { get; set; }
    public string? userAgent { get; set; }
    public string? actionId { get; set; }
    public object? data { get; set; }
    public string? details { get; set; }
    public string? area { get; set; }
    public string? category { get; set; }
    public string? categoryDisplayName { get; set; }
    public string? actorDisplayName { get; set; }
    public string? actorImageUrl { get; set; }
}

