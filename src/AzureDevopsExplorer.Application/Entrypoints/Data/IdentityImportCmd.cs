using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class IdentityImportCmd
{
    private readonly AzureDevopsApiOrgQueries queries;
    private readonly Mappers mapper = new Mappers();

    public IdentityImportCmd(AzureDevopsApiOrgClient httpClient)
    {
        this.queries = new AzureDevopsApiOrgQueries(httpClient);
    }

    public async Task Run(DataConfig config)
    {
        var lastImport = DateTime.UtcNow;

        if (config.IdentityImport)
        {
            await RunForOlderAndMissing(lastImport);
        }
    }

    public async Task RunForOlderAndMissing(DateTime lastImport)
    {
        while (true)
        {
            var identities = await FindIdentitiesToImport();

            foreach (var identity in identities)
            {
                await RunForIdentity(identity, lastImport);
            }

            var addedIdentities = await AddMissingGroupsIfExist();

            if (addedIdentities == false)
            {
                break;
            }
        }
    }

    private static async Task<List<IdentityImport>> FindIdentitiesToImport()
    {
        List<IdentityImport> identities = new();

        using (var db = new DataContext())
        {
            identities = await db.IdentityImport
                .Where(x => x.LastImport == null || x.LastImport < DateTime.UtcNow.AddMonths(-1))
                .ToListAsync();
        }

        return identities;
    }

    private async Task<bool> AddMissingGroupsIfExist()
    {
        var addedIdentities = false;
        // in theory, if the group descriptors are missing from the Identity table, they are also missing from 
        using (var db = new DataContext())
        {
            var memberOfDescriptorsNotImported = (
                from imo in db.IdentityMemberOf
                join i in db.Identity on imo.Descriptor equals i.Descriptor into identityGroup
                from i in identityGroup.DefaultIfEmpty() // This is equivalent to a LEFT JOIN
                where i.Id == null
                select imo.Descriptor
                )
                .Distinct()
                .ToList();

            var memberDescriptorsNotImported = (
                from imo in db.IdentityMember
                join i in db.Identity on imo.Descriptor equals i.Descriptor into identityGroup
                from i in identityGroup.DefaultIfEmpty() // This is equivalent to a LEFT JOIN
                where i.Id == null
                select imo.Descriptor
                )
                .Distinct()
                .ToList();

            var allDescriptors = new List<string>();
            allDescriptors.AddRange(memberDescriptorsNotImported);
            allDescriptors.AddRange(memberOfDescriptorsNotImported);
            var descriptorsToAdd = allDescriptors.Distinct().ToList();

            foreach (var descriptor in descriptorsToAdd)
            {
                if (db.IdentityImport.Any(x => x.Descriptor == descriptor) == false)
                {
                    addedIdentities = true;
                    db.IdentityImport.Add(new IdentityImport
                    {
                        Descriptor = descriptor,
                        LastImport = null
                    });
                }
            }

            db.SaveChanges();
        }

        return addedIdentities;
    }

    private async Task RunForIdentity(IdentityImport existing, DateTime lastImport)
    {
        using var db = new DataContext();

        var import = await GetIdentityReference(existing, db);
        var getIdentity = IdentityRetriever(existing);

        var identityFromApiResult = await getIdentity();
        if (identityFromApiResult.IsT1)
        {
            Console.WriteLine(identityFromApiResult.AsT1.AsError);
            return;
        }

        var data = identityFromApiResult.AsT0;
        var identityFromApi = data.Value.First();
        if (identityFromApi == null)
        {
            Console.WriteLine($"Identity is null {JsonConvert.SerializeObject(existing)}");
            return;
        }
        var identityObj = mapper.MapIdentity(identityFromApi);
        identityObj.LastImport = lastImport;

        var identityId = Guid.Parse(identityFromApi.Id);

        if (import.SubjectDescriptor == null)
        {
            import.SubjectDescriptor = identityFromApi.SubjectDescriptor;
        }
        if (import.Descriptor == null)
        {
            import.Descriptor = identityFromApi.Descriptor;
        }
        if (import.IdentityId == null)
        {
            import.IdentityId = identityId;
        }
        import.LastImport = lastImport;

        identityObj.MemberIds.Clear();
        if (identityFromApi.MemberIds != null)
        {
            foreach (var m in identityFromApi.MemberIds)
            {
                identityObj.MemberIds.Add(new Database.Model.Data.IdentityMemberId
                {
                    IdentityId = Guid.Parse(identityFromApi.Id),
                    MemberId = Guid.Parse(m),
                });
            }
        }

        identityObj.Members.Clear();
        if (identityFromApi.Members != null)
        {
            foreach (var m in identityFromApi.Members)
            {
                identityObj.Members.Add(new Database.Model.Data.IdentityMember
                {
                    IdentityId = Guid.Parse(identityFromApi.Id),
                    Descriptor = m
                });
            }
        }

        identityObj.MemberOf.Clear();
        if (identityFromApi.MemberOf != null)
        {
            foreach (var m in identityFromApi.MemberOf)
            {
                identityObj.MemberOf.Add(new Database.Model.Data.IdentityMemberOf
                {
                    IdentityId = Guid.Parse(identityFromApi.Id),
                    Descriptor = m,
                });
            }
        }

        identityObj.Properties.Clear();
        if (identityFromApi.Properties != null)
        {
            foreach (var property in identityFromApi.Properties)
            {
                identityObj.Properties.Add(new Database.Model.Data.IdentityProperty
                {
                    IdentityId = Guid.Parse(identityFromApi.Id),
                    Name = property.Key,
                    Value = property.Value.Value,
                    Type = property.Value.Type
                });
            }
        }

        var currentIdentity = db.Identity
            .Include(x => x.MemberIds)
            .Include(x => x.MemberOf)
            .Include(x => x.Members)
            .Include(x => x.Properties)
            .SingleOrDefault(x => x.Id == identityId);

        if (currentIdentity != null)
        {
            var compareLogic = new CompareLogic(new ComparisonConfig
            {
                MembersToIgnore = new List<string> {
                        nameof(Database.Model.Data.Identity.Id),
                        nameof(Database.Model.Data.Identity.LastImport),
                    },
                IgnoreCollectionOrder = true,
                MaxDifferences = 1000
            });

            var comparison = compareLogic.CompareSameType(currentIdentity, identityObj);

            if (comparison.AreEqual == false)
            {
                db.Identity.Remove(currentIdentity);

                db.IdentityChange.Add(new IdentityChange
                {
                    IdentityId = identityId,
                    PreviousImport = currentIdentity.LastImport,
                    NextImport = lastImport,
                    Difference = comparison.DifferencesString
                });

                db.Identity.Add(identityObj);

                await db.SaveChangesAsync();
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
            db.Identity.Add(identityObj);

            db.IdentityChange.Add(new IdentityChange
            {
                IdentityId = identityId,
                PreviousImport = null,
                NextImport = lastImport,
                Difference = $"First time seeing identity"
            });
            await db.SaveChangesAsync();
            return;
        }
    }

    private static async Task<IdentityImport> GetIdentityReference(IdentityImport existing, DataContext db)
    {
        if (existing.SubjectDescriptor != null)
        {
            return await db.IdentityImport
                    .Where(x => x.SubjectDescriptor == existing.SubjectDescriptor)
                    .SingleAsync();
        }

        if (existing.Descriptor != null)
        {
            return await db.IdentityImport
                    .Where(x => x.Descriptor == existing.Descriptor)
                    .SingleAsync();
        }

        var identities = await db.IdentityImport
            .Where(x => x.IdentityId == existing.IdentityId)
            .ToListAsync();

        if (identities.Count == 1)
        {
            return identities.Single();
        }

        // dedup
        var lastId = identities.Select(x => x.Id).Max();
        var toDelete = identities.Where(x => x.Id != lastId);
        db.IdentityImport.RemoveRange(toDelete);
        await db.SaveChangesAsync();

        return await db.IdentityImport
            .Where(x => x.IdentityId == existing.IdentityId)
            .SingleAsync();
    }

    private Func<Task<AzureDevopsApiResult<ListResponse<AzureDevopsApi.Dtos.Identity>>>> IdentityRetriever(IdentityImport identity)
    {
        if (identity.Descriptor != null)
        {
            return () => queries.GetIdentityByDescriptor(identity.Descriptor);
        }

        if (identity.SubjectDescriptor != null)
        {
            return () => queries.GetIdentityBySubjectDescriptor(identity.SubjectDescriptor);
        }

        return () => queries.GetIdentityById(identity.IdentityId.Value);
    }
}
