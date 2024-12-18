﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Pipelines;

[PrimaryKey(nameof(PipelineId), nameof(PipelineRevision))]
public class PipelineConfigurationYamlTemplate
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PipelineId { get; set; }
    public int PipelineRevision { get; set; }
    public string? TemplateExtendsRepository { get; set; }
    public string? TemplateExtendsPath { get; set; }
    public string? TemplateExtendsRef { get; set; }
    public string? TemplateSchedules { get; set; }
}
