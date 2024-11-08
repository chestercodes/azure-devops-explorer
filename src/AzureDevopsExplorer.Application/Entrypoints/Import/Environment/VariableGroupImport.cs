﻿using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi.DistributedTask;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Pipelines;
using AzureDevopsExplorer.Database.Model.Environment;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Environment;
public class VariableGroupImport
{
    private readonly ILogger logger;
    private readonly Mappers mapper;
    private readonly AzureDevopsProjectDataContext dataContext;

    public VariableGroupImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.VariableGroupAddLatest)
        {
            await ImportVariableGroups();
        }
    }

    public async Task ImportVariableGroups()
    {
        // TODO? figure out whether to remove values which are not in API response
        // or whether we can't be sure that it's not being used by another project
        var variableGroupResult = await dataContext.Queries.DistributedTask.GetVariableGroups();
        if (variableGroupResult.IsT1)
        {
            logger.LogError(variableGroupResult.AsT1.AsError);
            return;
        }

        var variableGroups = variableGroupResult.AsT0.value;
        var importTime = DateTime.UtcNow;
        var variableGroupIdsToAdd = variableGroups.Select(x => x.id);

        foreach (var vgId in variableGroupIdsToAdd)
        {
            await AddVariableGroup(vgId, importTime);
        }
    }

    private async Task AddVariableGroup(int id, DateTime importTime)
    {
        using var db = dataContext.DataContextFactory.Create();

        var currentVariables = db.VariableGroupVariable.Where(x => x.VariableGroupId == id).ToList();
        var currentVariableGroup = db.VariableGroup.SingleOrDefault(x => x.Id == id);
        var currentVariableGroupProjectReference = db.VariableGroupProjectReference.Where(x => x.VariableGroupId == id).ToList();

        var variableGroupResult = await dataContext.Queries.DistributedTask.GetVariableGroup(id);
        if (variableGroupResult.IsT1)
        {
            logger.LogError(variableGroupResult.AsT1.AsError);
            return;
        }

        var identityIds = new HashSet<Guid>();
        var variableGroupFromApi = VariableGroupFromApi(variableGroupResult.AsT0, identityIds);

        if (currentVariableGroup != null)
        {
            currentVariableGroup.Variables = currentVariables;
            currentVariableGroup.VariableGroupProjectReferences = currentVariableGroupProjectReference;

            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string> {
                    nameof(VariableGroupVariable.Id),
                    nameof(Database.Model.Environment.VariableGroup.LastImport)
                },
                IgnoreCollectionOrder = true,
                MaxDifferences = 1000
            });

            var variableGroupComparison = compareLogic.CompareSameType(currentVariableGroup, variableGroupFromApi);
            if (variableGroupComparison.AreEqual == false)
            {
                db.VariableGroupVariable.RemoveRange(currentVariables);
                db.VariableGroup.Remove(currentVariableGroup);
                db.VariableGroupProjectReference.RemoveRange(currentVariableGroupProjectReference);

                db.VariableGroupChange.Add(new VariableGroupChange
                {
                    VariableGroupId = currentVariableGroup.Id,
                    PreviousImport = currentVariableGroup.LastImport,
                    NextImport = importTime,
                    Difference = variableGroupComparison.DifferencesString
                });

                variableGroupFromApi.LastImport = importTime;
                db.VariableGroup.Add(variableGroupFromApi);
                db.SaveChanges();
                return;
            }
            else
            {
                // has not changed
                return;
            }
        }
        else
        {
            // not seen before, might have been added or is first import
            variableGroupFromApi.LastImport = importTime;
            db.VariableGroup.Add(variableGroupFromApi);

            db.VariableGroupChange.Add(new VariableGroupChange
            {
                VariableGroupId = variableGroupFromApi.Id,
                PreviousImport = null,
                NextImport = importTime,
                Difference = $"First time seeing variable group {variableGroupFromApi.Id}"
            });
            db.SaveChanges();
            return;
        }

    }

    private Database.Model.Environment.VariableGroup VariableGroupFromApi(AzureDevopsApi.DistributedTask.VariableGroup vg, HashSet<Guid> identityIds)
    {
        var m = mapper.MapVariableGroup(vg);
        if (vg.variables == null)
        {
            vg.variables = new Dictionary<string, Variable>();
        }
        m.Variables.Clear();

        if (vg.variableGroupProjectReferences == null)
        {
            vg.variableGroupProjectReferences = new List<AzureDevopsApi.DistributedTask.VariableGroupProjectReference>();
        }
        m.VariableGroupProjectReferences.Clear();

        AddIdentityIfExists(identityIds, vg.createdBy);
        AddIdentityIfExists(identityIds, vg.modifiedBy);

        foreach (var v in vg.variables)
        {
            m.Variables.Add(new VariableGroupVariable
            {
                VariableGroupId = vg.id,
                Name = v.Key,
                Value = v.Value.value,
                IsSecret = v.Value.isSecret
            });
        }

        foreach (var pr in vg.variableGroupProjectReferences)
        {
            m.VariableGroupProjectReferences.Add(new Database.Model.Environment.VariableGroupProjectReference
            {
                VariableGroupId = vg.id,
                ProjectReferenceId = Guid.Parse(pr.projectReference.id),
                ProjectReferenceName = pr.projectReference.name,
                Name = pr.name
            });
        }

        return m;
    }

    private static void AddIdentityIfExists(HashSet<Guid> identityIds, VariableGroupIdentity? variableGroupId)
    {
        if (variableGroupId != null)
        {
            var id = Guid.Parse(variableGroupId.id);
            if (id != Guid.Empty)
            {
                identityIds.Add(id);
            }
        }
    }

    private static void AddToIdentityImport(DataContext db, Guid id)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        var exists = db.IdentityImport.Any(x => x.IdentityId == id);
        if (!exists)
        {
            db.IdentityImport.Add(new IdentityImport
            {
                IdentityId = id,
                LastImport = null
            });
        }
    }
}
