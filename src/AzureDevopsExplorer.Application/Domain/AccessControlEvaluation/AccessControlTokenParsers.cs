using System.Text.RegularExpressions;

namespace AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;

public record SecurityNamespaceId(Guid Value);
public record SecurityNamespaceToken(string Value);

public record AccessControlTokenParseConfig(
    SecurityNamespaceId NamespaceId,
    AccessControlTokenParser Parser,
    SecurityNamespacePermissionResourceType? ResourceType);

public enum SecurityNamespacePermissionResourceType
{
    ServiceEndpoint,
    VariableGroup,
    GitRepository,
    Identity,
    Build,
    AgentPool,
    Environment,
    SecureFile,

    Organisation,
    Project
}

public record SecurityNamespacePermissionResourceId(string Value);
public record ProjectId(Guid? Value);
public record SecurityNamespacePermissionResourceInfo(SecurityNamespacePermissionResourceId? Id, SecurityNamespacePermissionResourceType? Type, ProjectId? ProjectId, AccessControlTokenParseResultType ParseResultType);
public record SecurityNamespacePermissionResourceData(SecurityNamespacePermissionResourceInfo Info, string ResourceName);
public record SecurityNamespacePermissionIdentityData(Guid AzDoId, string IdentityName, string IdentityDisplayName, bool IsGroup, int? NumberMembers);

public class AccessControlTokenParsers
{
    private const string ProjectIdGuid = "(?<ProjectId>[0-9A-Fa-f]{8}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{12})";
    private const string ObjectIdGuid = "(?<ObjectId>[0-9A-Fa-f]{8}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{4}[-]?[0-9A-Fa-f]{12})";
    private const string ObjectIdInt = "(?<ObjectId>[0-9]+)";
    private const string ObjectIdOther = "(?<ObjectId>.+)";

    public readonly static AccessControlTokenParseConfig AgentPool = new AccessControlTokenParseConfig(
        SecurityNamespaceIds.DistributedTask,
        new AccessControlTokenParser(
            [
                new Regex("AgentQueues.*"),
                new Regex("MachineGroups.*"),
                new Regex("DeploymentPools.*")
            ],
            "AgentPools",
            null,
            null,
            $"AgentPools/{ObjectIdInt}"
        ),
        SecurityNamespacePermissionResourceType.AgentPool);

    public readonly static AccessControlTokenParseConfig Build = new AccessControlTokenParseConfig(
        SecurityNamespaceIds.Build,
        new AccessControlTokenParser(
            [],
            null,
            $"{ProjectIdGuid}",
            $"{ProjectIdGuid}/{ObjectIdOther}", // .* because folders will have their name in token
            null
        ),
        SecurityNamespacePermissionResourceType.Build);

    public readonly static AccessControlTokenParseConfig Collection = new AccessControlTokenParseConfig(
        SecurityNamespaceIds.Collection,
        new AccessControlTokenParser(
            [],
            "NAMESPACE$",
            null,
            null,
            null
        ),
        null);

    public readonly static AccessControlTokenParseConfig Environment = new AccessControlTokenParseConfig(
        SecurityNamespaceIds.Environment,
        new AccessControlTokenParser(
            [],
            null,
            $"Environments/{ProjectIdGuid}",
            $"Environments/{ProjectIdGuid}/{ObjectIdInt}",
            $"Environments/{ObjectIdInt}"
        ),
        SecurityNamespacePermissionResourceType.Environment);

    public readonly static AccessControlTokenParseConfig GitRepositories = new AccessControlTokenParseConfig(
        SecurityNamespaceIds.GitRepositories,
        new AccessControlTokenParser(
            [
                new Regex(".*/refs/heads/.*") // ignore branch policies
            ],
            null,
            $"repoV2/{ProjectIdGuid}",
            $"repoV2/{ProjectIdGuid}/{ObjectIdGuid}",
            null
        ),
        SecurityNamespacePermissionResourceType.GitRepository);

    public readonly static AccessControlTokenParseConfig Identity = new AccessControlTokenParseConfig(
        SecurityNamespaceIds.Identity,
        new AccessControlTokenParser(
            [],
            null,
            $"{ProjectIdGuid}",
            $"{ProjectIdGuid}\\\\{ObjectIdGuid}",
            null
        ),
        SecurityNamespacePermissionResourceType.Identity);

    public readonly static AccessControlTokenParseConfig Project = new AccessControlTokenParseConfig(
        SecurityNamespaceIds.Project,
        new AccessControlTokenParser(
            [],
            null,
            $"\\$PROJECT:vstfs:///Classification/TeamProject/{ProjectIdGuid}",
            null,
            null
        ),
        null);

    public readonly static AccessControlTokenParseConfig SecureFile = new AccessControlTokenParseConfig(
        SecurityNamespaceIds.Library,
        new AccessControlTokenParser(
            [
                new Regex($"Library/{ProjectIdGuid}$"),
                new Regex($"Library/{ProjectIdGuid}/VariableGroup.*"),
                new Regex($"Library/Collection/OAuthConfiguration"),
                new Regex($"Library/Collection/VariableGroup.*")
            ],
            null,
            null,
            $"Library/{ProjectIdGuid}/SecureFile/{ObjectIdGuid}",
            null
        ),
        SecurityNamespacePermissionResourceType.SecureFile);

    public readonly static AccessControlTokenParseConfig ServiceEndpoints = new AccessControlTokenParseConfig(
        SecurityNamespaceIds.ServiceEndpoints,
        new AccessControlTokenParser(
            [

            ],
            null,
            $"endpoints/{ProjectIdGuid}",
            $"endpoints/{ProjectIdGuid}/{ObjectIdGuid}",
            $"endpoints/Collection/{ProjectIdGuid}"
        ),
        SecurityNamespacePermissionResourceType.ServiceEndpoint);

    public readonly static AccessControlTokenParseConfig VariableGroup = new AccessControlTokenParseConfig(
        SecurityNamespaceIds.Library,
        new AccessControlTokenParser(
            [
                new Regex($".*/SecureFile/.*"),
                new Regex($".*OAuth.*"),
            ],
            null,
            $"Library/{ProjectIdGuid}",
            $"Library/{ProjectIdGuid}/VariableGroup/{ObjectIdInt}",
            $"Library/Collection/VariableGroup/{ObjectIdInt}"
        ),
        SecurityNamespacePermissionResourceType.VariableGroup);

    public readonly static List<AccessControlTokenParseConfig> All = new List<AccessControlTokenParseConfig>
    {
        AgentPool,
        Build,
        Collection,
        Environment,
        GitRepositories,
        Identity,
        Project,
        SecureFile,
        ServiceEndpoints,
        VariableGroup
    };

    public readonly static Dictionary<SecurityNamespaceId, List<AccessControlTokenParseConfig>> AllByNamespaceId =
        All.GroupBy(x => x.NamespaceId).ToDictionary(x => x.Key, x => x.ToList());

    public static SecurityNamespacePermissionResourceInfo? Parse(SecurityNamespaceId namespaceId, string token)
    {
        if (AllByNamespaceId.ContainsKey(namespaceId) == false)
        {
            return null;
        }

        var parserConfigs = AllByNamespaceId[namespaceId];

        foreach (var parser in parserConfigs)
        {
            var parseResult = parser.Parser.Parse(token);
            if (parseResult != null)
            {
                return new SecurityNamespacePermissionResourceInfo(
                    new SecurityNamespacePermissionResourceId(parseResult.ObjectId),
                    parser.ResourceType,
                    new ProjectId(parseResult.ProjectId),
                    parseResult.Type
                    );
            }
        }

        return null;
    }
}