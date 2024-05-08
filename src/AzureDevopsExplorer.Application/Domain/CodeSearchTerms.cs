namespace AzureDevopsExplorer.Application.Domain;

public record CodeSearchTerm(string SearchKey, string SearchTerm);

/// <summary>
/// Initial values, probably best to replace these with databases which are relavant to your software estate
/// </summary>
public class CodeSearchTerms
{
    public static List<CodeSearchTerm> All()
    {
        List<string> databaseNames = [
            "Accounts",
            "Authentication"
            ];

        List<CodeSearchTerm> databaseTerms = databaseNames
            .Select(x =>
            {
                var key = $"DbConfig{x}";
                var searchTerm = $"\"Database={x}\" OR \"Initial Catalog={x}\"";
                return new CodeSearchTerm(key, searchTerm);
            })
            .ToList();

        return databaseTerms;
    }
}