using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database.Mappers;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Graph;

public class GraphServicePrincipalImport
{
    private readonly Mappers mapper;
    private readonly ILogger logger;
    private readonly AzureDevopsOrganisationDataContext orgDataContext;

    public GraphServicePrincipalImport(AzureDevopsOrganisationDataContext orgDataContext)
    {
        logger = orgDataContext.LoggerFactory.Create(this);
        mapper = new Mappers();
        this.orgDataContext = orgDataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.GraphAddServicePrincipals)
        {
            await AddGraphServicePrincipals();
            await AddGraphServicePrincipalsToIdentityImport();
        }
    }

    public async Task AddGraphServicePrincipals()
    {
        logger.LogInformation($"Running graph service principals import");

        var graphServicePrincipalsResult = await orgDataContext.Queries.Graph.GetServicePrincipals();
        if (graphServicePrincipalsResult.IsT1)
        {
            logger.LogError(graphServicePrincipalsResult.AsT1.AsError);
            return;
        }

        var graphServicePrincipals = graphServicePrincipalsResult.AsT0;
        var graphServicePrincipalsFromApi = graphServicePrincipals
            .Select(x =>
            {
                var mapped = mapper.MapGraphServicePrincipal(x);
                return mapped;
            })
            .ToList();

        using var db = orgDataContext.DataContextFactory.Create();
        db.GraphServicePrincipal.RemoveRange(db.GraphServicePrincipal);
        db.GraphServicePrincipal.AddRange(graphServicePrincipalsFromApi);
        db.SaveChanges();
    }

    public async Task AddGraphServicePrincipalsToIdentityImport()
    {
        using var db = orgDataContext.DataContextFactory.Create();

        var subjectDescriptorsToAdd = db.GraphServicePrincipal
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
