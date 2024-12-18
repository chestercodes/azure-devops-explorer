﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(PipelineId), nameof(PipelineRevision))]
public class PipelineConfigurationYamlTemplateImport
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PipelineId { get; set; }
    public int PipelineRevision { get; set; }
    public DateTime? LastImport { get; set; }
    public string? ImportError { get; set; }
}
