using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Graph;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Graph;

public class GraphUserImport
{
    private readonly Mappers mapper;
    private readonly ILogger logger;
    private readonly AzureDevopsOrganisationDataContext orgDataContext;

    public GraphUserImport(AzureDevopsOrganisationDataContext orgDataContext)
    {
        logger = orgDataContext.LoggerFactory.CreateLogger(GetType());
        mapper = new Mappers();
        this.orgDataContext = orgDataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.GraphAddUsers)
        {
            await AddGraphUsers();
            await AddGraphUsersToIdentityImport();
        }
    }

    public async Task AddGraphUsers()
    {
        var graphUsersResult = await orgDataContext.Queries.Graph.GetUsers();
        if (graphUsersResult.IsT1)
        {
            logger.LogError(graphUsersResult.AsT1.AsError);
            return;
        }

        var graphUsers = graphUsersResult.AsT0;
        var graphUsersFromApi = graphUsers
            .Select(x =>
            {
                var mapped = mapper.MapGraphUser(x);
                return mapped;
            })
            .ToList();

        using var db = orgDataContext.DataContextFactory.Create();
        db.GraphUser.RemoveRange(db.GraphUser);
        db.GraphUser.AddRange(graphUsersFromApi);
        db.SaveChanges();
    }

    public async Task AddGraphUsersToIdentityImport()
    {
        using var db = orgDataContext.DataContextFactory.Create();

        var subjectDescriptorsToAdd = db.GraphUser
            .Where(c => db.IdentityImport
                .Select(b => b.SubjectDescriptor)
                .Contains(c.Descriptor) == false
            )
            .Select(x => x.Descriptor);

        db.IdentityImport.AddRange(
            subjectDescriptorsToAdd.Select(
                subjectDescriptor =>
                new Database.Model.Pipelines.IdentityImport
                {
                    SubjectDescriptor = subjectDescriptor,
                }
            )
        );

        db.SaveChanges();
    }
}
