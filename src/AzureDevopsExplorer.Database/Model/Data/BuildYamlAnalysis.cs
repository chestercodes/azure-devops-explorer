﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(BuildRunId))]
public class BuildYamlAnalysis
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int BuildRunId { get; set; }
    public int PipelineId { get; set; }
    public int? PipelineRevision { get; set; }
    //public string? PipelineName { get; set; }
    public BuildYamlAnalysisState State { get; set; }
    [StringLength(32)]
    public string? BuildYamlHash { get; set; }
}

public enum BuildYamlAnalysisState
{
    Initial,
    GotYaml,
    ErrorFromApi
}