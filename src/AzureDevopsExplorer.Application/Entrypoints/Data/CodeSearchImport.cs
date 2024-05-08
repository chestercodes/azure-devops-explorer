using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.AzureDevopsApi.Client;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Data;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class CodeSearchImport
{
    private readonly AzureDevopsApiProjectClient httpClient;

    public CodeSearchImport(AzureDevopsApiProjectClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task Run(DataConfig config)
    {
        if (config.CodeSearchImport)
        {
            await RunCodeSearchImport();
        }
    }

    public async Task RunCodeSearchImport()
    {
        var queries = new AzureDevopsApiProjectQueries(httpClient);

        using var db = new DataContext();

        await EnsureTheresAtLeastSomeKeywordsToSearchFor(db);

        foreach (var term in db.CodeSearchKeyword.ToList())
        {
            var result = await queries.CodeSearch(term.SearchTerm);
            if (result.IsT1)
            {
                Console.WriteLine(result.AsT1.Value.ToString());
                continue;
            }

            var response = result.AsT0;
            var azureReposResults = response.results.Where(x => x.repository.id != null);
            foreach (var res in azureReposResults)
            {
                db.CodeSearchKeywordUsage.Add(new CodeSearchKeywordUsage
                {
                    RepositoryId = Guid.Parse(res.repository.id),
                    RepositoryName = res.repository.name,
                    SearchKey = term.SearchKey,
                    FilePath = res.path
                });
            }
        }

        await db.SaveChangesAsync();
    }

    private async Task EnsureTheresAtLeastSomeKeywordsToSearchFor(DataContext db)
    {
        if (db.CodeSearchKeyword.Any() == false)
        {
            foreach (var (searchKey, searchTerm) in CodeSearchTerms.All())
            {
                db.CodeSearchKeyword.Add(new CodeSearchKeyword
                {
                    SearchKey = searchKey,
                    SearchTerm = searchTerm,
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
