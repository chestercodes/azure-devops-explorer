using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;

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
        variableGroupResult.Switch(variableGroups =>
        {
            var importTime = DateTime.UtcNow;
            var variableGroupIdsToAdd = variableGroups.Value.Select(x => (Id: x.Id, ImportTime: importTime));

            foreach (var pair in variableGroupIdsToAdd)
            {
                AddVariableGroup(pair);
            }
        },
        err =>
        {
            Console.WriteLine(err.AsError);
        });
    }

    private void AddVariableGroup((int Id, DateTime ImportTime) pair)
    {
        // todo, only change if diff and log differences
        var (id, importTime) = pair;
        var queries = new AzureDevopsApiProjectQueries(httpClient);
        using var db = new DataContext();

        db.Variable.RemoveRange(db.Variable.Where(x => x.VariableGroupId == id));
        db.VariableGroup.RemoveRange(db.VariableGroup.Where(x => x.Id == id));
        db.VariableGroupProjectReference.RemoveRange(db.VariableGroupProjectReference.Where(x => x.VariableGroupId == id));

        var variableGroupResult = queries.GetVariableGroup(id).Result;
        variableGroupResult.Switch(vg =>
        {
            var m = mapper.MapVariableGroup(vg);
            m.LastImport = importTime;

            Guid? createdBy = null;
            if (vg.CreatedBy != null)
            {
                var id = Guid.Parse(vg.CreatedBy.Id);
                createdBy = id;
                AddToIdentityImport(db, id);
            }

            if (vg.ModifiedBy != null)
            {
                var id = Guid.Parse(vg.ModifiedBy.Id);
                if (createdBy == null || createdBy.Value != id)
                {
                    AddToIdentityImport(db, id);
                }
            }

            if (vg.Variables != null)
            {
                m.Variables.Clear();
                foreach (var v in vg.Variables)
                {
                    m.Variables.Add(new Database.Model.Data.Variable
                    {
                        VariableGroupId = vg.Id,
                        Name = v.Key,
                        Value = v.Value.Value,
                        IsSecret = v.Value.IsSecret
                    });
                }

                m.VariableGroupProjectReferences.Clear();
                if (vg.VariableGroupProjectReferences != null)
                {
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
                }
            }

            db.VariableGroup.Add(m);
            db.SaveChanges();
        }, err =>
        {
            Console.WriteLine(err.AsError);
        });
    }

    private static void AddToIdentityImport(DataContext db, Guid id)
    {
        if (id == Guid.Empty)
        {
            return;
        }

        var exists = db.IdentityImport.Any(x => x.Id == id);
        if (!exists)
        {
            db.IdentityImport.Add(new IdentityImport
            {
                Id = id,
                LastImport = null
            });
        }
    }
}
