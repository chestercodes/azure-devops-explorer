﻿namespace AzureDevopsExplorer.Database.Model.Data;

public class AgentPool
{
    public DateTime CreatedOn { get; set; }
    public bool AutoProvision { get; set; }
    public bool AutoUpdate { get; set; }
    public bool AutoSize { get; set; }
    public int? TargetSize { get; set; }
    public int? AgentCloudId { get; set; }
    public Guid CreatedById { get; set; }
    public Guid OwnerId { get; set; }
    public int Id { get; set; }
    public string Scope { get; set; }
    public string Name { get; set; }
    public bool? IsHosted { get; set; }
    public string PoolType { get; set; }
    public int Size { get; set; }
    public bool IsLegacy { get; set; }
    public string Options { get; set; }
    public DateTime? LastImport { get; set; }
}