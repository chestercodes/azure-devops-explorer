using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using KellermanSoftware.CompareNetObjects;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class VariableGroupImport
{
    private readonly AzureDevopsApiProjectClient httpClient;
    private readonly Mappers mapper;

    public VariableGroupImport(AzureDevopsApiProjectClient httpClient)
    {
        this.httpClient = httpClient;
        mapper = new Mappers();
    }

    public async Task Run(DataConfig config)
    {
        if (config.VariableGroupAddLatest)
        {
            await ImportVariableGroups();
        }
    }

    public async Task ImportVariableGroups()
    {
        var queries = new AzureDevopsApiProjectQueries(httpClient);
        var variableGroupResult = await queries.GetVariableGroups();
        if (variableGroupResult.IsT1)
        {
            Console.WriteLine(variableGroupResult.AsT1.AsError);
            return;
        }

        var variableGroups = variableGroupResult.AsT0.Value;
        var importTime = DateTime.UtcNow;
        var variableGroupIdsToAdd = variableGroups.Select(x => x.Id);

        foreach (var vgId in variableGroupIdsToAdd)
        {
            await AddVariableGroup(vgId, importTime);
        }
    }

    private async Task AddVariableGroup(int id, DateTime importTime)
    {
        var queries = new AzureDevopsApiProjectQueries(httpClient);
        using var db = new DataContext();

        var currentVariables = db.VariableGroupVariable.Where(x => x.VariableGroupId == id).ToList();
        var currentVariableGroup = db.VariableGroup.SingleOrDefault(x => x.Id == id);
        var currentVariableGroupProjectReference = db.VariableGroupProjectReference.Where(x => x.VariableGroupId == id).ToList();

        var variableGroupResult = await queries.GetVariableGroup(id);
        if (variableGroupResult.IsT1)
        {
            Console.WriteLine(variableGroupResult.AsT1.AsError);
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
                    nameof(VariableGroup.LastImport)
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

    private VariableGroup VariableGroupFromApi(AzureDevopsApi.Dtos.VariableGroup vg, HashSet<Guid> identityIds)
    {
        var m = mapper.MapVariableGroup(vg);
        if (vg.Variables == null)
        {
            vg.Variables = new Dictionary<string, AzureDevopsApi.Dtos.Variable>();
        }
        m.Variables.Clear();

        if (vg.VariableGroupProjectReferences == null)
        {
            vg.VariableGroupProjectReferences = new List<AzureDevopsApi.Dtos.VariableGroupProjectReference>();
        }
        m.VariableGroupProjectReferences.Clear();

        AddIdentityIfExists(identityIds, vg.CreatedBy);
        AddIdentityIfExists(identityIds, vg.ModifiedBy);

        foreach (var v in vg.Variables)
        {
            m.Variables.Add(new Database.Model.Data.VariableGroupVariable
            {
                VariableGroupId = vg.Id,
                Name = v.Key,
                Value = v.Value.Value,
                IsSecret = v.Value.IsSecret
            });
        }

        foreach (var pr in vg.VariableGroupProjectReferences)
        {
            m.VariableGroupProjectReferences.Add(new Database.Model.Data.VariableGroupProjectReference
            {
                VariableGroupId = vg.Id,
                ProjectReferenceId = Guid.Parse(pr.ProjectReference.Id),
                ProjectReferenceName = pr.ProjectReference.Name,
                Name = pr.Name
            });
        }

        return m;
    }

    private static void AddIdentityIfExists(HashSet<Guid> identityIds, AzureDevopsApi.Dtos.VariableGroupIdentity? variableGroupId)
    {
        if (variableGroupId != null)
        {
            var id = Guid.Parse(variableGroupId.Id);
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
