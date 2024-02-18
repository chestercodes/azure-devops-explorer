using System.Runtime.Serialization;

namespace AzureDevopsExplorer.Database.Model.Data;

public class TeamProjectReference
{
    public Guid Id { get; set; }
    public string? Abbreviation { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public ProjectState? State { get; set; }
    public long? Revision { get; set; }
    public ProjectVisibility? Visibility { get; set; }
    public string? DefaultTeamImageUrl { get; set; }
    public DateTime? LastUpdateTime { get; set; }
}

public enum ProjectVisibility
{
    Private,
    Organization,
    Public
}

public enum ProjectState
{
    [EnumMember]
    Deleting = 2,
    [EnumMember]
    New = 0,
    [EnumMember]
    WellFormed = 1,
    [EnumMember]
    CreatePending = 3,
    [EnumMember]
    All = -1,
    [EnumMember]
    Unchanged = -2,
    [EnumMember]
    Deleted = 4
}
