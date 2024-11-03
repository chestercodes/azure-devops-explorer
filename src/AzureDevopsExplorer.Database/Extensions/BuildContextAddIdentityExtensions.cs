namespace AzureDevopsExplorer.Database.Extensions;

public static class BuildContextAddIdentityExtensions
{
    //public static DataContext AddIdentities(this DataContext db, IEnumerable<Microsoft.VisualStudio.Services.WebApi.IdentityRef> identities)
    //{
    //    var identitiesToAdd = identities
    //        .Where(x => x != null)
    //        .DistinctBy(x => x.Id);
    //    foreach (var identity in identitiesToAdd)
    //    {
    //        EnsureIdentityExists(db, identity);
    //    }


    //    return db;
    //}

    //public static void EnsureIdentityExists(DataContext db, Microsoft.VisualStudio.Services.WebApi.IdentityRef identity)
    //{
    //    if (identity == null)
    //    {
    //        return;
    //    }

    //    var id = Guid.Parse(identity.Id);

    //    if (id == Guid.Empty)
    //    {
    //        return;
    //    }

    //    var exists = db.IdentityImport.Any(x => x.IdentityId == id);
    //    if (!exists)
    //    {
    //        db.IdentityImport.Add(new IdentityImport
    //        {
    //            IdentityId = id,
    //            LastImport = null
    //        });
    //    }
    //}
}
