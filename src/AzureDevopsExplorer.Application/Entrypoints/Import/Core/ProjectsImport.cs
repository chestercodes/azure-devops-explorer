using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database.Mappers;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Core;

public class ProjectsImport
{
    private readonly Mappers mapper;
    private readonly ILogger logger;
    private readonly AzureDevopsOrganisationDataContext dataContext;

    public ProjectsImport(AzureDevopsOrganisationDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.ProjectsImport)
        {
            await AddProjects();
        }
    }

    public async Task AddProjects()
    {
        logger.LogInformation($"Running projects import");

        var importTime = DateTime.UtcNow;
        var projectsResult = await dataContext.Queries.Core.GetProjects();
        if (projectsResult.IsT1)
        {
            logger.LogError(projectsResult.AsT1.AsError);
            return;
        }

        var projectsFromApi = projectsResult.AsT0.value
            .Select(x =>
            {
                var mapped = mapper.MapProject(x);
                return mapped;
            })
            .ToList();

        using (var db = dataContext.DataContextFactory.Create())
        {
            db.Project.RemoveRange(db.Project);
            db.Project.AddRange(projectsFromApi);
            await db.SaveChangesAsync();
        }
    }
}
