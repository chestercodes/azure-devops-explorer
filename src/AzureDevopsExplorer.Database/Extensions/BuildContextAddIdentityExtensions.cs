using AzureDevopsExplorer.Database.Model.Data;

namespace AzureDevopsExplorer.Database.Extensions;

public static class BuildContextAddIdentityExtensions
{
    public static DataContext AddIdentities(this DataContext db, IEnumerable<Microsoft.VisualStudio.Services.WebApi.IdentityRef> identities)
    {
        var identitiesToAdd = identities
            .Where(x => x != null)
            .DistinctBy(x => x.Id);
        foreach (var identity in identitiesToAdd)
        {
            EnsureIdentityExists(db, identity);
        }


        return db;
    }

    public static void EnsureIdentityExists(DataContext db, Microsoft.VisualStudio.Services.WebApi.IdentityRef identity)
    {
        if (identity == null)
        {
            return;
        }

        var id = Guid.Parse(identity.Id);

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

    public static DataContext AddReferenceLinks(this DataContext db, Microsoft.VisualStudio.Services.WebApi.ReferenceLinks links, Guid projectId, ReferenceLinkSourceType sourceType, string sourceId)
    {
        foreach (var link in links.Links)
        {
            var refLink = link.Value as Microsoft.VisualStudio.Services.WebApi.ReferenceLink;
            if (refLink != null)
            {
                db.ReferenceLink.Add(new ReferenceLink
                {
                    ProjectId = projectId,
                    SourceType = sourceType,
                    SourceId = sourceId,
                    Name = link.Key,
                    Href = new Uri(refLink.Href)
                });
            }
        }

        return db;
    }
}
