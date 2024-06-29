using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Data;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class CodeSearchImport
{
    private readonly ILogger logger;
    private readonly AzureDevopsApiProjectQueries queries;
    private readonly AzureDevopsProject project;

    public CodeSearchImport(AzureDevopsProjectDataContext dataContext)
    {
        this.queries = dataContext.Queries.Value;
        this.project = dataContext.Project;
        logger = dataContext.GetLogger();
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
        using var db = new DataContext();

        await EnsureTheresAtLeastSomeKeywordsToSearchFor(db);

        foreach (var term in db.CodeSearchKeyword.Where(x => x.ProjectId == null || x.ProjectId == project.ProjectId).ToList())
        {
            var result = await queries.CodeSearch(term.SearchTerm);
            if (result.IsT1)
            {
                Console.WriteLine(result.AsT1.Value.ToString());
                continue;
            }

            var existingForTerm = db.CodeSearchKeywordUsage.Where(x => x.SearchKey == term.SearchKey);
            db.CodeSearchKeywordUsage.RemoveRange(existingForTerm);

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
                    ProjectId = null
                });
            }

            await db.SaveChangesAsync();
        }
    }
}
