namespace AzureDevopsExplorer.Database.Model.Environment;

public class SecureFileChange
{
    public int Id { get; set; }
    public DateTime? PreviousImport { get; set; }
    public DateTime? NextImport { get; set; }
    public Guid SecureFileId { get; set; }
    public string Difference { get; set; }
}
