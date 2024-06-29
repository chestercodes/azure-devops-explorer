using System.Text.RegularExpressions;

namespace AzureDevopsExplorer.Application.Domain.AccessControlEvaluation;

public class AccessControlTokenParser
{
    public List<Regex> IgnoreTokenPatterns { get; private set; }
    public Regex? OrganisationPattern { get; private set; }
    public Regex? ProjectPattern { get; private set; }
    public Regex? ProjectAndObjectPattern { get; private set; }
    public Regex? CollectionObjectPattern { get; private set; }

    public AccessControlTokenParser(List<Regex> ignoreTokenPatterns, string? organisationPattern, string? projectPattern, string? projectAndObjectPattern, string? collectionObjectPattern)
    {
        IgnoreTokenPatterns = ignoreTokenPatterns;
        if (organisationPattern != null)
        {
            OrganisationPattern = new Regex($"^{organisationPattern}$", RegexOptions.IgnoreCase);
        }
        if (projectPattern != null)
        {
            ProjectPattern = new Regex($"^{projectPattern}$", RegexOptions.IgnoreCase);
        }
        if (projectAndObjectPattern != null)
        {
            ProjectAndObjectPattern = new Regex($"^{projectAndObjectPattern}$", RegexOptions.IgnoreCase);
        }
        if (collectionObjectPattern != null)
        {
            CollectionObjectPattern = new Regex($"^{collectionObjectPattern}$", RegexOptions.IgnoreCase);
        }
    }

    public AccessControlTokenParseResult? Parse(string token)
    {
        if (IgnoreTokenPatterns.Any(r => r.IsMatch(token)))
        {
            return null;
        }
        if (OrganisationPattern != null && OrganisationPattern.IsMatch(token))
        {
            return new AccessControlTokenParseResult(AccessControlTokenParseResultType.OrganisationLevel, null, null);
        }

        if (ProjectPattern != null && ProjectPattern.IsMatch(token))
        {
            var match = ProjectPattern.Match(token);
            var projectId = Guid.Parse(match.Groups["ProjectId"].Value);
            return new AccessControlTokenParseResult(AccessControlTokenParseResultType.ProjectLevel, projectId, null);
        }

        if (ProjectAndObjectPattern != null && ProjectAndObjectPattern.IsMatch(token))
        {
            var match = ProjectAndObjectPattern.Match(token);
            var projectId = Guid.Parse(match.Groups["ProjectId"].Value);
            var objectId = match.Groups["ObjectId"].Value;
            return new AccessControlTokenParseResult(AccessControlTokenParseResultType.ProjectObjectLevel, projectId, objectId);
        }

        if (CollectionObjectPattern != null && CollectionObjectPattern.IsMatch(token))
        {
            var match = CollectionObjectPattern.Match(token);
            var objectId = match.Groups["ObjectId"].Value;
            return new AccessControlTokenParseResult(AccessControlTokenParseResultType.CollectionObjectLevel, null, objectId);
        }

        throw new Exception(token);
    }
}
