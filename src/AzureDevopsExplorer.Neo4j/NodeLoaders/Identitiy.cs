using AzureDevopsExplorer.Database;

namespace AzureDevopsExplorer.Neo4j.NodeLoaders;

//public async Task LoadGroups(Neo4jLoader loader, DataContext db)
//{
//    var groups = db.Group.Select(x => new { x.Descriptor, x.DisplayName, x.PrincipalName }).ToList();
//    var groupsVals = groups.Select(x => new Dictionary<string, string> {
//            { Group.Props.Descriptor, x.Descriptor },
//            { Group.Props.PrincipalName, x.PrincipalName },
//            { Group.Props.DisplayName, x.DisplayName }
//        });
//    await loader.DeleteThenLoadNodes(Group.Name, groupsVals.ToArray());
//}

public class Identity : ILoadNodesToNeo4J
{
    public const string Name = "Identity";
    public class Props
    {
        public const string Id = "id";
        public const string Descriptor = "descriptor";
        public const string SubjectDescriptor = "subjectDescriptor";
        public const string CustomDisplayName = "customDisplayName";
        public const string ProviderDisplayName = "providerDisplayName";
    }

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        var toAdd = db.Identity
            .ToList()
            .Select(x => new Dictionary<string, string> {
                { Props.Id, x.Id.ToString() },
                { Props.Descriptor, x.Descriptor },
                { Props.SubjectDescriptor, x.SubjectDescriptor },
                { Props.ProviderDisplayName, x.ProviderDisplayName },
                { Props.CustomDisplayName, x.CustomDisplayName }
            })
            .ToArray();

        await loader.DeleteThenLoadNodes(Name, toAdd);
    }
}
