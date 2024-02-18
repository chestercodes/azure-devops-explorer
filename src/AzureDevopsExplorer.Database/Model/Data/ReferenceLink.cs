using Microsoft.EntityFrameworkCore;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(ProjectId), nameof(SourceType), nameof(SourceId), nameof(Name))]
public class ReferenceLink
{
    public long Id { get; set; }
    public Guid ProjectId { get; set; }
    // Needs to be more generic than int in case it's a guid etc
    public string SourceId { get; set; }
    public ReferenceLinkSourceType SourceType { get; set; }
    public string Name { get; set; }
    public Uri Href { get; set; }
}

public enum ReferenceLinkSourceType
{
    Build,
    //BuildController,
    //IdentityRef,
    PipelineRun
}