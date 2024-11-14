using AzureDevopsExplorer.Application.Configuration;
using Microsoft.Graph.Models;
using Microsoft.Graph;
using AzureDevopsExplorer.Database.Model.Graph;
using AzureDevopsExplorer.Database;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.GraphData;
public class GraphGroupsImport
{
    private readonly GraphServiceClient graphClient;
    private readonly ICreateDataContexts dataContextFactory;
    private ILogger logger;

    public GraphGroupsImport(GraphServiceClient graphClient, ILoggerFactory loggerFactory, ICreateDataContexts dataContextFactory)
    {
        this.graphClient = graphClient;
        this.dataContextFactory = dataContextFactory;
        logger = loggerFactory.Create(this);
    }

    public async Task Run(ImportConfig config)
    {
        if (config.GraphAddEntraGroups)
        {
            await Import();
        }
    }

    public async Task Import()
    {
        try
        {
            var groups = await GetAllGroups();
            if (groups.Count == 0)
            {
                return;
            }

            var groupsFromApi = MapGroups(groups);

            await WriteToDatabase(groupsFromApi);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed somehow");
        }
    }

    private async Task WriteToDatabase(List<EntraGroup> groupsFromApi)
    {
        var db = dataContextFactory.Create();
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

