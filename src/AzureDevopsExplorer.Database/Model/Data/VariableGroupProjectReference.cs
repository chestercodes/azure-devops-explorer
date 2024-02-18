﻿namespace AzureDevopsExplorer.Database.Model.Data;

public class VariableGroupProjectReference
{
    public int Id { get; set; }
    public int VariableGroupId { get; set; }
    public string Name { get; set; }
    public string ProjectReferenceName { get; set; }
    public Guid ProjectReferenceId { get; set; }
}
