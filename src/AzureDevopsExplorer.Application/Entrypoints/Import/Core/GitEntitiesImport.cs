using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Core;
using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Core;
public class GitEntitiesImport
{
    private readonly ILogger logger;
    private readonly Guid projectId;
    private readonly Mappers mapper;
    private readonly AzureDevopsProjectDataContext dataContext;

    public GitEntitiesImport(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        projectId = dataContext.Project.ProjectId;
        mapper = new Mappers();
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.GitAddRepositories)
        {
            await RunAddGitRepositories();
        }
        if (config.GitAddPullRequests)
        {
            await RunAddPullRequests();
        }
        if (config.GitAddRepositoriesDefaultBranchCommits)
        {
            await RunAddDefaultBranchCommits();
        }
    }

    public async Task RunAddGitRepositories()
    {
        logger.LogInformation($"Running git repository import");

        var importTime = DateTime.UtcNow;
        using var db = dataContext.DataContextFactory.Create();

        var reposResult = await dataContext.Queries.Git.GetRepositories();
        if (reposResult.IsT1)
        {
            logger.LogError(reposResult.AsT1.AsError);
            return;
        }

        var repos = reposResult.AsT0.value;
        var reposFromApi = repos.Select(x =>
        {
            var m = mapper.MapGitRepository(x);
            m.ProjectReferenceId = projectId;
            m.LastImport = importTime;
            return m;
        });

        var compareLogic = new CompareLogic(new ComparisonConfig
        {
            MembersToIgnore = new List<string>
                {
                    nameof(GitRepository.LastImport),
                    // size is not exciting enough to track changes
                    nameof(GitRepository.Size)
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
        logger.LogInformation($"Running git pull requests import");
        using var db = dataContext.DataContextFactory.Create();
        var existingRepoIds = db.GitRepository
            .Where(x => x.ProjectReferenceId == projectId)
            .Select(x => x.Id)
            .ToList();

        foreach (var repoId in existingRepoIds)
        {
            logger.LogDebug($"Running git pull requests import " + repoId);

            var prsResult = await dataContext.Queries.Git.GetAllPullRequestsForRepository(repoId.ToString());
            if (prsResult.IsT1)
            {
                logger.LogError(prsResult.AsT1.AsError);
                continue;
            }
            var prs = prsResult.AsT0.value;

            foreach (var pullRequest in prs)
            {
                var existing = db.GitPullRequest.SingleOrDefault(x => x.PullRequestId == pullRequest.pullRequestId);
                if (existing != null)
                {
                    var existingStatus = existing.Status;
                    var existingStatusTerminalStates = new[] { PullRequestStatus.Completed, PullRequestStatus.Abandoned };
                    if (existingStatusTerminalStates.Contains(existingStatus))
                    {
                        continue;
                    }

                    var newStatus = pullRequest.status;
                    var newStatusTerminalStates = new[] {
                        AzureDevopsApi.Git.PullRequestStatus.Completed,
                        AzureDevopsApi.Git.PullRequestStatus.Abandoned
                    };
                    if (newStatusTerminalStates.Contains(newStatus) == false)
                    {
                        continue;
                    }

                    db.GitPullRequestReview.RemoveRange(
                        db.GitPullRequestReview.Where(x => x.PullRequestId == pullRequest.pullRequestId)
                        );
                    db.GitPullRequest.Remove(existing);
                }

                var pr = mapper.MapGitPullRequest(pullRequest);
                List<GitPullRequestReview> reviews = new();
                foreach (var review in pullRequest.reviewers)
                {
                    reviews.Add(new GitPullRequestReview
                    {
                        PullRequestId = pullRequest.pullRequestId,
                        ReviewerId = Guid.Parse(review.id),
                        VotedForId = null,
                        Vote = review.vote
                    });

                    if (review.votedFor != null)
                    {
                        foreach (var votedFor in review.votedFor)
                        {
                            reviews.Add(new GitPullRequestReview
                            {
                                PullRequestId = pullRequest.pullRequestId,
                                ReviewerId = Guid.Parse(review.id),
                                VotedForId = Guid.Parse(votedFor.id),
                                Vote = votedFor.vote
                            });
                        }
                    }
                }
                db.GitPullRequest.Add(pr);
                db.GitPullRequestReview.AddRange(reviews);

                await db.SaveChangesAsync();
            }
        }
    }

    public async Task RunAddDefaultBranchCommits()
    {
        using var db = dataContext.DataContextFactory.Create();
        var existingRepos = db.GitRepository
            .Where(x => x.ProjectReferenceId == projectId)
            .Select(x => new { x.Id, x.DefaultBranch })
            .ToList();

        foreach (var repo in existingRepos)
        {
            logger.LogDebug($"Running git default branch commits import " + repo.Id);

            var commitsResult = await dataContext.Queries.Git.GetBranchCommitsForRepository(repo.Id.ToString(), repo.DefaultBranch);
            if (commitsResult.IsT1)
            {
                logger.LogError(commitsResult.AsT1.AsError);
                return;
            }
            var commits = commitsResult.AsT0.value;

            foreach (var commit in commits)
            {
                var gitCommit = mapper.MapGitCommit(commit);
                gitCommit.RepositoryId = repo.Id;

                if (db.GitCommit.Any(x => x.CommitId == commit.commitId) == false)
                {
                    db.GitCommit.Add(gitCommit);
                }
            }
            await db.SaveChangesAsync();
        }
    }
}
