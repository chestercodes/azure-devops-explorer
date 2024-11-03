using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Neo4j.NodeLoaders;
using ResourceType = AzureDevopsExplorer.Database.Model.Security.SecurityNamespacePermissionResourceType;

namespace AzureDevopsExplorer.Neo4j.RelationshipLoaders;

public class HasPermissionRelationships : ILoadRelationshipsToNeo4J
{
    public string Name => "HAS_PERMISSION";

    public class Props
    {
        public const string NamespaceId = "namespaceId";
        public const string AllowOrDeny = "allowOrDeny";
        public const string Name = "name";
        public const string DisplayName = "displayName";
        public const string Bit = "bit";
    }

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        List<Relationship> relationships = new();
        foreach (var permission in db.SecurityNamespaceResourcePermission)
        {
            if (AddPermission(permission.ResourceType) == false)
            {
                continue;
            }

            relationships.Add(new Relationship
            {
                RelationshipName = Name,
                RelationshipProps = new List<RelationshipProperty>
                {
                    new RelationshipProperty(Props.NamespaceId, permission.NamespaceId.ToString()),
                    new RelationshipProperty(Props.AllowOrDeny, permission.AllowOrDeny.ToString()),
                    new RelationshipProperty(Props.Name, permission.Name),
                    new RelationshipProperty(Props.DisplayName, permission.DisplayName),
                    new RelationshipProperty(Props.Bit, permission.Bit.ToString()),
                },
                SourceNodeType = NodeLoaders.Identity.Name,
                SourceMatches = new List<RelationshipMatchPair> {
                    new RelationshipMatchPair { MatchName = NodeLoaders.Identity.Props.Descriptor, MatchValue = permission.Descriptor }
                },
                DestNodeType = ResourceTypeToSourceNodeType(permission.ResourceType),
                DestMatches = new List<RelationshipMatchPair> {
                    new RelationshipMatchPair { MatchName = ResourceTypeToSourceNodeId(permission.ResourceType), MatchValue = permission.ResourceId.ToString() }
                }
            });
        }
        await loader.LoadRelationships(relationships);
    }

    private static bool AddPermission(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Build:
                return false;
            case ResourceType.Organisation:
                return false;
            case ResourceType.Server:
                return false;

            default: return true;
        }
    }
    private static string ResourceTypeToSourceNodeType(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Environment: return PipelineEnvironment.Name;
            case ResourceType.GitRepository: return Repository.Name;
            case ResourceType.Identity: return Identity.Name;
            case ResourceType.Project: return Project.Name;
            case ResourceType.SecureFile: return SecureFile.Name;
            case ResourceType.ServiceEndpoint: return ServiceEndpoint.Name;
            case ResourceType.VariableGroup: return VariableGroup.Name;
            default: throw new NotImplementedException($"Count not find resource type '{resourceType}'");
        }
    }

    private static string ResourceTypeToSourceNodeId(ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceType.Environment: return PipelineEnvironment.Props.Id;
            case ResourceType.GitRepository: return Repository.Props.Id;
            case ResourceType.Identity: return Identity.Props.Id;
            case ResourceType.Project: return Project.Props.Id;
            case ResourceType.SecureFile: return SecureFile.Props.Id;
            case ResourceType.ServiceEndpoint: return ServiceEndpoint.Props.Id;
            case ResourceType.VariableGroup: return VariableGroup.Props.Id;
            default: throw new NotImplementedException($"Count not find resource type id for '{resourceType}'");
        }
    }
}
