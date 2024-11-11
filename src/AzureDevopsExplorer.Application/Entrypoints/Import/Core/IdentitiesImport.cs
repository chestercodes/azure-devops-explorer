using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Pipelines;
using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Core;
public class IdentitiesImport
{
    private readonly ILogger logger;
    private readonly Mappers mapper = new Mappers();
    private readonly AzureDevopsOrganisationDataContext dataContext;

    public IdentitiesImport(AzureDevopsOrganisationDataContext dataContext)
    {
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
        var subjectDescriptors = await FindSubjectDescriptorsToImport();
        // fails with a 404 if the url is too long
        var batchSize = 30;
        foreach (var subjectDescriptorBatch in subjectDescriptors.Chunk(batchSize))
        {
            await RunBatch(subjectDescriptorBatch, lastImport);
        }
    }

    private async Task<List<string>> FindSubjectDescriptorsToImport()
    {
        var limit = DateTime.UtcNow.AddDays(-7);

        List<string> subjectDescriptors = new();

        using (var db = dataContext.DataContextFactory.Create())
        {
            subjectDescriptors = await db.IdentityImport
                .Where(x => x.LastImport == null || x.LastImport < limit)
                .Select(x => x.SubjectDescriptor)
                .ToListAsync();
        }

        return subjectDescriptors;
    }

    private async Task RunBatch(string[] subjectDescriptors, DateTime lastImport)
    {
        var identityFromApiResult = await dataContext.Queries.Core.GetIdentitiesBySubjectDescriptor(subjectDescriptors.ToList());
        if (identityFromApiResult.IsT1)
        {
            logger.LogError(identityFromApiResult.AsT1.AsError);
            return;
        }

        foreach (var identityFromApi in identityFromApiResult.AsT0.value)
        {
            await RunForIdentity(identityFromApi, lastImport);
        }

        var identityFromApiExpandedResult = await dataContext.Queries.Core.GetIdentitiesBySubjectDescriptor(subjectDescriptors.ToList(), AzureDevopsApi.Core.OrgQueries.IdentitiesQueryMembership.Expanded);
        if (identityFromApiExpandedResult.IsT1)
        {
            logger.LogError(identityFromApiExpandedResult.AsT1.AsError);
            return;
        }

        foreach (var identityFromApi in identityFromApiExpandedResult.AsT0.value)
        {
            await RunForIdentityExpanded(identityFromApi);
        }
    }

    private async Task RunForIdentityExpanded(AzureDevopsApi.Core.Identity identityFromApi)
    {
        using var db = dataContext.DataContextFactory.Create();

        var subjectDescriptor = identityFromApi.subjectDescriptor;
        var identityId = Guid.Parse(identityFromApi.id);


        db.IdentityMemberExpanded.RemoveRange(db.IdentityMemberExpanded.Where(x => x.IdentityId == identityId));
        db.IdentityMemberOfExpanded.RemoveRange(db.IdentityMemberOfExpanded.Where(x => x.IdentityId == identityId));

        foreach (var m in identityFromApi.members)
        {
            db.IdentityMemberExpanded.Add(new IdentityExpandedMember
            {
                IdentityId = Guid.Parse(identityFromApi.id),
                Descriptor = m,
            });
        }

        foreach (var m in identityFromApi.memberOf)
        {
            db.IdentityMemberOfExpanded.Add(new IdentityExpandedMemberOf
            {
                IdentityId = Guid.Parse(identityFromApi.id),
                Descriptor = m,
            });
        }

        db.SaveChanges();
    }

    private async Task RunForIdentity(AzureDevopsApi.Core.Identity identityFromApi, DateTime lastImport)
    {
        using var db = dataContext.DataContextFactory.Create();

        var subjectDescriptor = identityFromApi.subjectDescriptor;
        var import = db.IdentityImport.SingleOrDefault(x => x.SubjectDescriptor == subjectDescriptor);
        if (import == null)
        {
            logger.LogError($"Could not find subject descriptor '{subjectDescriptor}', something has gone wrong");
            return;
        }

        var identityObj = mapper.MapIdentity(identityFromApi);
        identityObj.LastImport = lastImport;

        var identityId = Guid.Parse(identityFromApi.id);

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
                    //nameof(Identity.Id),
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
}
