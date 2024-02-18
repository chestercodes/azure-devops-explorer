using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class IdentityImportCmd
{
    private readonly FindIdentity findIdentity;
    private readonly Mappers mapper = new Mappers();

    public IdentityImportCmd(VssConnection connection, string projectName)
    {
        findIdentity = new FindIdentity(connection, projectName);
    }

    public async Task Run(DataConfig config)
    {
        if (config.IdentityImport)
        {
            await RunForOlderAndMissing();
        }
    }

    public async Task RunForOlderAndMissing()
    {
        List<Guid> identityIds = new();

        using (var db = new DataContext())
        {
            identityIds = await db.IdentityImport
                .Where(x => x.LastImport == null || x.LastImport < DateTime.UtcNow.AddMonths(-1))
                .Select(x => x.Id)
                .ToListAsync();
        }

        var lastImport = DateTime.UtcNow;
        foreach (var identityId in identityIds)
        {
            await RunForIdentity(identityId, lastImport);
        }
    }

    private async Task RunForIdentity(Guid identityId, DateTime lastImport)
    {
        using var db = new DataContext();

        var import = await db.IdentityImport
            .Where(x => x.Id == identityId)
            .SingleAsync();

        db.Identity.RemoveRange(db.Identity.Where(x => x.Id == identityId));
        db.IdentityProperty.RemoveRange(db.IdentityProperty.Where(x => x.IdentityId == identityId));

        import.LastImport = lastImport;

        var identityFromApi = await findIdentity.GetId(identityId);
        var identityObj = mapper.MapIdentity(identityFromApi);
        //identityObj.Properties.Clear();
        foreach (var property in identityFromApi.Properties)
        {
            db.IdentityProperty.Add(new Database.Model.Data.IdentityProperty
            {
                IdentityId = identityId,
                Key = property.Key,
                Value = property.Value.ToString()
            });
        }

        db.Identity.Add(identityObj);
        await db.SaveChangesAsync();
    }
}
