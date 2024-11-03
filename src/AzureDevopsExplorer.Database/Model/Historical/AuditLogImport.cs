namespace AzureDevopsExplorer.Database.Model.Historical;

public class AuditLogImport
{
    public long Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? ImportError { get; set; }
    public AuditLogImportStatus Status { get; set; }
}

public enum AuditLogImportStatus
{
    Initial,
    Done,
    ErrorFromApi
}
