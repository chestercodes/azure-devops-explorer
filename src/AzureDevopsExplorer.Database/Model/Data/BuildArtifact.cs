using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(BuildId), nameof(Name))]
public class BuildArtifact
{
    public int BuildId { get; set; }
    public long Id { get; set; }
    // seen lengths up to 72
    [StringLength(512)]
    public string Name { get; set; }
    public Guid? Source { get; set; }
    // seems to be one of SymbolStore, Container, PipelineArtifact
    [StringLength(50)]
    public string? ResourceType { get; set; }
    // bit of a guess
    [StringLength(512)]
    public string? ResourceData { get; set; }
    //public Dictionary<string, string> Properties { get; set; }
    // not worth storing example is 
    // https://dev.azure.com/someorg/proj-guid/_apis/build/builds/1234/artifacts?artifactName=somezipfile&api-version=7.0
    //public string? ResourceUrl { get; set; }
    // ResourceDownloadUrl bit of a guess, seen up to 401 chars
    [StringLength(1000)]
    public string? ResourceDownloadUrl { get; set; }

}
