using AzureDevopsExplorer.Application.Configuration;
using Microsoft.Graph.Models;
using Microsoft.Graph;
using AzureDevopsExplorer.Database.Model.Graph;
using AzureDevopsExplorer.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.GraphData;
public class GraphApplicationsImport
{
    private readonly GraphServiceClient graphClient;
    private readonly ICreateDataContexts dataContextFactory;
    private ILogger logger;

    public GraphApplicationsImport(GraphServiceClient graphClient, ILoggerFactory loggerFactory, ICreateDataContexts dataContextFactory)
    {
        this.graphClient = graphClient;
        this.dataContextFactory = dataContextFactory;
        logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task Run(ImportConfig config)
    {
        if (config.GraphAddEntraApplications)
        {
            await Import();
        }
    }

    public async Task Import()
    {
        try
        {
            var applications = await GetAllApplications();
            if (applications.Count == 0)
            {
                return;
            }

            var applicationsFromApi = MapApplications(applications);

            await WriteToDatabase(applicationsFromApi);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed somehow");
        }
    }

    private async Task WriteToDatabase(List<EntraApplication> applicationsFromApi)
    {
        var db = dataContextFactory.Create();
        var existing = db.EntraApplication.Include(x => x.AppRoles);
        db.EntraApplication.RemoveRange(existing);

        db.EntraApplication.AddRange(applicationsFromApi);
        await db.SaveChangesAsync();
    }

    private static List<EntraApplication> MapApplications(List<Microsoft.Graph.Models.Application> applications)
    {
        var apps = new List<EntraApplication>();
        foreach (var app in applications)
        {
            var appRoles = app.AppRoles
                .Select(x => new GraphAppRole
                {
                    AppRoleId = x.Id,
                    Description = x.Description,
                    DisplayName = x.DisplayName,
                    Origin = x.Origin,
                    IsEnabled = x.IsEnabled,
                    Value = x.Value
                })
                .ToList();

            var entraApp = new EntraApplication
            {
                Id = Guid.Parse(app.Id),
                AppId = Guid.Parse(app.AppId),
                DisplayName = app.DisplayName,
                UniqueName = app.UniqueName,
                AppRoles = appRoles
            };
            apps.Add(entraApp);
        }

        return apps;
    }

    private async Task<List<Microsoft.Graph.Models.Application>> GetAllApplications()
    {
        var applicationsRequest = await graphClient.Applications.GetAsync();

        if (applicationsRequest == null)
        {
            return new List<Microsoft.Graph.Models.Application>();
        }
        var applications = new List<Microsoft.Graph.Models.Application>();

        var pageIterator = PageIterator<Microsoft.Graph.Models.Application, ApplicationCollectionResponse>
            .CreatePageIterator(
                graphClient,
                applicationsRequest,
                (msg) =>
                {
                    applications.Add(msg);
                    return true;
                });

        await pageIterator.IterateAsync();
        return applications;
    }

}

