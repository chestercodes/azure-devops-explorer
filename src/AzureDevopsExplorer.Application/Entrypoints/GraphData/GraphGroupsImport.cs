using AzureDevopsExplorer.Application.Configuration;
using Microsoft.Graph.Models;
using Microsoft.Graph;
using AzureDevopsExplorer.Database.Model.Graph;
using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Application.Entrypoints.GraphData;
public class GraphGroupsImport
{
    private readonly GraphServiceClient graphClient;

    public GraphGroupsImport(GraphServiceClient graphClient)
    {
        this.graphClient = graphClient;
    }

    public async Task Run(DataConfig config)
    {
        if (config.GraphAddGroups)
        {
            await Import();
        }
    }

    public async Task Import()
    {
        var groups = await GetAllGroups();
        if (groups.Count == 0)
        {
            return;
        }

        var groupsFromApi = MapGroups(groups);

        await WriteToDatabase(groupsFromApi);
    }

    private async Task WriteToDatabase(List<EntraGroup> groupsFromApi)
    {
        var db = new DataContext();
        var existing = db.EntraGroup;
        db.EntraGroup.RemoveRange(existing);

        db.EntraGroup.AddRange(groupsFromApi);
        await db.SaveChangesAsync();
    }

    private static List<EntraGroup> MapGroups(List<Microsoft.Graph.Models.Group> groups)
    {
        var entraGroups = new List<EntraGroup>();
        foreach (var g in groups)
        {
            entraGroups.Add(new EntraGroup
            {
                Description = g.Description,
                DisplayName = g.DisplayName,
                Id = g.Id,
                SecurityIdentifier = g.SecurityIdentifier,
                UniqueName = g.UniqueName,
            });
            //var appRoles = app.AppRoles
            //    .Select(x => new GraphAppRole
            //    {
            //        AppRoleId = x.Id,
            //        Description = x.Description,
            //        DisplayName = x.DisplayName,
            //        Origin = x.Origin,
            //        IsEnabled = x.IsEnabled,
            //        Value = x.Value
            //    })
            //    .ToList();

            //var entraApp = new EntraApplication
            //{
            //    Id = Guid.Parse(app.Id),
            //    AppId = Guid.Parse(app.AppId),
            //    DisplayName = app.DisplayName,
            //    UniqueName = app.UniqueName,
            //    AppRoles = appRoles
            //};
            //apps.Add(entraApp);
        }

        return entraGroups;
    }

    private async Task<List<Microsoft.Graph.Models.Group>> GetAllGroups()
    {
        var entitiesRequest = await graphClient.Groups.GetAsync();

        if (entitiesRequest == null)
        {
            return new List<Microsoft.Graph.Models.Group>();
        }
        var entities = new List<Microsoft.Graph.Models.Group>();

        Func<Group, bool> addToReturnResult = entity =>
        {
            entities.Add(entity);
            return true;
        };
        var pageIterator = PageIterator<Microsoft.Graph.Models.Group, GroupCollectionResponse>
            .CreatePageIterator(
                graphClient,
                entitiesRequest,
                addToReturnResult);


        await pageIterator.IterateAsync();
        return entities;
    }

}

