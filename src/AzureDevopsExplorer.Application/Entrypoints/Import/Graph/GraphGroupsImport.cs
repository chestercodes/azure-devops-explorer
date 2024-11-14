using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database.Mappers;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Graph;

public class GraphGroupsImport
{
    private readonly Mappers mapper;
    private readonly ILogger logger;
    private readonly AzureDevopsOrganisationDataContext orgDataContext;

    public GraphGroupsImport(AzureDevopsOrganisationDataContext orgDataContext)
    {
        logger = orgDataContext.LoggerFactory.Create(this);
        mapper = new Mappers();
        this.orgDataContext = orgDataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.GraphAddGroups)
        {
            await AddGraphGroups();
            await AddGraphGroupsToIdentityImport();
        }
    }

    public async Task AddGraphGroups()
    {
        logger.LogInformation($"Running graph groups import");

        var graphGroupsResult = await orgDataContext.Queries.Graph.GetGroups();
        if (graphGroupsResult.IsT1)
        {
            logger.LogError(graphGroupsResult.AsT1.AsError);
            return;
        }

        var graphGroups = graphGroupsResult.AsT0;
        var graphGroupsFromApi = graphGroups
            .Select(x =>
            {
                var mapped = mapper.MapGraphGroup(x);
                return mapped;
            })
            .ToList();

        using var db = orgDataContext.DataContextFactory.Create();
        db.GraphGroup.RemoveRange(db.GraphGroup);
        db.GraphGroup.AddRange(graphGroupsFromApi);
        db.SaveChanges();
    }

    public async Task AddGraphGroupsToIdentityImport()
    {
        using var db = orgDataContext.DataContextFactory.Create();

        var subjectDescriptorsToAdd = db.GraphGroup
            .Where(c => db.IdentityImport
                .Select(b => b.SubjectDescriptor)
                .Contains(c.Descriptor) == false
            )
            .Select(x => x.Descriptor);

        db.IdentityImport.AddRange(
            subjectDescriptorsToAdd.Select(
                subjectDescriptor =>
                new Database.Model.Core.IdentityImport
                {
                    SubjectDescriptor = subjectDescriptor,
                }
            )
        );

        db.SaveChanges();
    }
}
