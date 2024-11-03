using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.AzureDevopsApi.Dtos;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Pipelines;
using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Core;
public class IdentityImportCmd
{
    private readonly AzureDevopsApiOrgQueries queries;
    private readonly ILogger logger;
    private readonly Mappers mapper = new Mappers();
    private readonly AzureDevopsOrganisationDataContext dataContext;

    public IdentityImportCmd(AzureDevopsOrganisationDataContext dataContext)
    {
        queries = dataContext.Queries;
        logger = dataContext.LoggerFactory.CreateLogger(GetType());
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
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

    private async Task<List<IdentityImport>> FindIdentitiesToImport()
    {
        List<IdentityImport> identities = new();

        using (var db = dataContext.DataContextFactory.Create())
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
        using (var db = dataContext.DataContextFactory.Create())
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
        using var db = dataContext.DataContextFactory.Create();

        var import = await GetIdentityReference(existing, db);
        var getIdentity = IdentityRetriever(existing);

        var identityFromApiResult = await getIdentity();
        if (identityFromApiResult.IsT1)
        {
            logger.LogError(identityFromApiResult.AsT1.AsError);
            return;
        }

        var data = identityFromApiResult.AsT0;
        var identityFromApi = data.value.First();
        if (identityFromApi == null)
        {
            logger.LogError($"Identity is null {JsonConvert.SerializeObject(existing)}");
            return;
        }
        var identityObj = mapper.MapIdentity(identityFromApi);
        identityObj.LastImport = lastImport;

        var identityId = Guid.Parse(identityFromApi.id);

        if (import.SubjectDescriptor == null)
        {
            import.SubjectDescriptor = identityFromApi.subjectDescriptor;
        }
        if (import.Descriptor == null)
        {
            import.Descriptor = identityFromApi.descriptor;
        }
        if (import.IdentityId == null)
        {
            import.IdentityId = identityId;
        }
        import.LastImport = lastImport;

        identityObj.MemberIds.Clear();
        if (identityFromApi.memberIds != null)
        {
            foreach (var m in identityFromApi.memberIds)
            {
                identityObj.MemberIds.Add(new IdentityMemberId
                {
                    IdentityId = Guid.Parse(identityFromApi.id),
                    MemberId = Guid.Parse(m),
                });
            }
        }

        identityObj.Members.Clear();
        if (identityFromApi.members != null)
        {
            foreach (var m in identityFromApi.members)
            {
                identityObj.Members.Add(new IdentityMember
                {
                    IdentityId = Guid.Parse(identityFromApi.id),
                    Descriptor = m
                });
            }
        }

        identityObj.MemberOf.Clear();
        if (identityFromApi.memberOf != null)
        {
            foreach (var m in identityFromApi.memberOf)
            {
                identityObj.MemberOf.Add(new IdentityMemberOf
                {
                    IdentityId = Guid.Parse(identityFromApi.id),
                    Descriptor = m,
                });
            }
        }

        identityObj.Properties.Clear();
        if (identityFromApi.properties != null)
        {
            foreach (var property in identityFromApi.properties)
            {
                identityObj.Properties.Add(new IdentityProperty
                {
                    IdentityId = Guid.Parse(identityFromApi.id),
                    Name = property.Key,
                    Value = property.Value.value,
                    Type = property.Value.type
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
                        nameof(Identity.Id),
                        nameof(Identity.LastImport),
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

    private Func<Task<AzureDevopsApiResult<ListResponse<AzureDevopsApi.Core.Identity>>>> IdentityRetriever(IdentityImport identity)
    {
        if (identity.Descriptor != null)
        {
            return () => queries.Core.GetIdentityByDescriptor(identity.Descriptor);
        }

        if (identity.SubjectDescriptor != null)
        {
            return () => queries.Core.GetIdentityBySubjectDescriptor(identity.SubjectDescriptor);
        }

        return () => queries.Core.GetIdentityById(identity.IdentityId.Value);
    }
}
