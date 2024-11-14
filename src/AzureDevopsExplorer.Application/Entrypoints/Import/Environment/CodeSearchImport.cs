using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Environment;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Environment;
public class CodeSearchImport
{
    private readonly ILogger logger;
    private readonly AzureDevopsApiProjectQueries queries;
    private readonly AzureDevopsProject project;
    private readonly AzureDevopsProjectDataContext dataContext;

    public CodeSearchImport(AzureDevopsProjectDataContext dataContext)
    {
        queries = dataContext.Queries;
        project = dataContext.Project;
        logger = dataContext.LoggerFactory.Create(this);
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.CodeSearchImport)
        {
            await RunCodeSearchImport();
        }
    }

    public async Task RunCodeSearchImport()
    {
        using var db = dataContext.DataContextFactory.Create();

        await EnsureTheresAtLeastSomeKeywordsToSearchFor(db);

        foreach (var term in db.CodeSearchKeyword.Where(x => x.ProjectId == null || x.ProjectId == project.ProjectId).ToList())
        {
            var result = await queries.Search.CodeSearch(term.SearchTerm);
            if (result.IsT1)
            {
                logger.LogError(result.AsT1.AsError);
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
