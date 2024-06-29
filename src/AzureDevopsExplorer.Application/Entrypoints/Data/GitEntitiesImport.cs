using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class GitEntitiesImport
{
    private readonly ILogger logger;
    private readonly Guid projectId;
    private readonly Mappers mapper;
    FindPullRequestsAndEntities prQueries;

    public GitEntitiesImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.GetLogger();
        this.projectId = dataContext.Project.ProjectId;
        prQueries = new FindPullRequestsAndEntities(dataContext.VssConnection.Value, dataContext.Project.ProjectName);
        mapper = new Mappers();
    }

    public async Task Run(DataConfig config)
    {
        if (config.GitAddRepositories)
        {
            await RunAddGitRepositories();
        }
        if (config.GitAddPullRequests)
        {
            await RunAddPullRequests();
        }
    }

    public async Task RunAddGitRepositories()
    {
        var importTime = DateTime.UtcNow;
        using var db = new DataContext();

        var repos = await prQueries.GetRepositories();
        var reposFromApi = repos.Select(x =>
        {
            var m = mapper.MapGitRepository(x);
            m.LastImport = importTime;
            return m;
        });

        var compareLogic = new CompareLogic(new ComparisonConfig
        {
            MembersToIgnore = new List<string>
                {
                    nameof(Database.Model.Data.GitRepository.LastImport),
                    // size is not exciting enough to track changes
                    nameof(Database.Model.Data.GitRepository.Size)
                },
            MaxDifferences = 1000
        });

        var existingReposForProject = db.GitRepository.Where(x => x.ProjectReferenceId == projectId).ToList();

        foreach (var repo in reposFromApi)
        {
            var existingRepo = existingReposForProject.SingleOrDefault(x => x.Id == repo.Id);
            if (existingRepo != null)
            {
                var comparison = compareLogic.CompareSameType(existingRepo, repo);
                if (comparison.AreEqual == false)
                {
                    mapper.MapGitRepository(repo, existingRepo);

                    db.GitRepositoryChange.Add(new GitRepositoryChange
                    {
                        RepositoryId = repo.Id,
                        PreviousImport = existingRepo.LastImport,
                        NextImport = importTime,
                        Difference = comparison.DifferencesString
                    });
                }
                else
                {
                    // has not changed
                }
            }
            else
            {
                db.GitRepository.Add(repo);

                db.GitRepositoryChange.Add(new GitRepositoryChange
                {
                    RepositoryId = repo.Id,
                    PreviousImport = null,
                    NextImport = importTime,
                    Difference = $"First time or added repo {repo.Id}"
                });
            }
        }
        await db.SaveChangesAsync();
    }

    public async Task RunAddPullRequests()
    {
        using var db = new DataContext();
        var existingRepoIds = db.GitRepository
            .Select(x => x.Id)
            .ToList();

        List<Guid> addedIdentityIds = new();

        foreach (var repoId in existingRepoIds)
        {
            var prs = await prQueries.GetPullRequests(repoId);
            foreach (var pullRequest in prs)
            {
                var existing = db.GitPullRequest.SingleOrDefault(x => x.PullRequestId == pullRequest.PullRequestId);
                if (existing != null)
                {
                    var existingStatus = existing.Status;
                    var existingStatusTerminalStates = new[] { PullRequestStatus.Completed, PullRequestStatus.Abandoned };
                    if (existingStatusTerminalStates.Contains(existingStatus))
                    {
                        continue;
                    }

                    var newStatus = pullRequest.Status;
                    var newStatusTerminalStates = new[] {
                        Microsoft.TeamFoundation.SourceControl.WebApi.PullRequestStatus.Completed,
                        Microsoft.TeamFoundation.SourceControl.WebApi.PullRequestStatus.Abandoned
                    };
                    if (newStatusTerminalStates.Contains(newStatus) == false)
                    {
                        continue;
                    }

                    db.GitPullRequestReview.RemoveRange(
                        db.GitPullRequestReview.Where(x => x.PullRequestId == pullRequest.PullRequestId)
                        );
                    db.GitPullRequest.Remove(existing);
                }

                var pr = mapper.MapGitPullRequest(pullRequest);
                List<GitPullRequestReview> reviews = new();
                foreach (var review in pullRequest.Reviewers)
                {
                    AddIdentityToImportTable(db, Guid.Parse(review.Id), addedIdentityIds);

                    reviews.Add(new GitPullRequestReview
                    {
                        PullRequestId = pullRequest.PullRequestId,
                        ReviewerId = Guid.Parse(review.Id),
                        VotedForId = null,
                        Vote = review.Vote
                    });

                    if (review.VotedFor != null)
                    {
                        foreach (var votedFor in review.VotedFor)
                        {
                            AddIdentityToImportTable(db, Guid.Parse(votedFor.Id), addedIdentityIds);

                            reviews.Add(new GitPullRequestReview
                            {
                                PullRequestId = pullRequest.PullRequestId,
                                ReviewerId = Guid.Parse(review.Id),
                                VotedForId = Guid.Parse(votedFor.Id),
                                Vote = votedFor.Vote
                            });
                        }
                    }
                }
                db.GitPullRequest.Add(pr);
                db.GitPullRequestReview.AddRange(reviews);
            }
            await db.SaveChangesAsync();
        }
    }

    private static void AddIdentityToImportTable(DataContext db, Guid id, List<Guid> addedIdentityIds)
    {
        if (addedIdentityIds.Contains(id))
        {
            return;
        }

        if (db.IdentityImport.Any(x => x.IdentityId == id) == false)
        {
            addedIdentityIds.Add(id);
            db.IdentityImport.Add(new IdentityImport
            {
                IdentityId = id,
                LastImport = null
            });
        }
    }
}
