﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureDevopsExplorer.Database.Model.Data;

[PrimaryKey(nameof(ServiceEndpointId), nameof(Name))]
public class ServiceEndpointData
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid ServiceEndpointId { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }
}
