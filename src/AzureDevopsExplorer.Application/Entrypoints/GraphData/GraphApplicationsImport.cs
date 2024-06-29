using AzureDevopsExplorer.Application.Configuration;
using Microsoft.Graph.Models;
using Microsoft.Graph;
using AzureDevopsExplorer.Database.Model.Graph;
using AzureDevopsExplorer.Database;
using Microsoft.EntityFrameworkCore;

namespace AzureDevopsExplorer.Application.Entrypoints.GraphData;
public class GraphApplicationsImport
{
    private readonly GraphServiceClient graphClient;

    public GraphApplicationsImport(GraphServiceClient graphClient)
    {
        this.graphClient = graphClient;
    }

    public async Task Run(DataConfig config)
    {
        if (config.GraphAddApplications)
        {
            await Import();
        }
    }

    public async Task Import()
    {
        var applications = await GetAllApplications();
        if (applications.Count == 0)
        {
            return;
        }

        var applicationsFromApi = MapApplications(applications);

        await WriteToDatabase(applicationsFromApi);
    }

    private async Task WriteToDatabase(List<EntraApplication> applicationsFromApi)
    {
        var db = new DataContext();
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

