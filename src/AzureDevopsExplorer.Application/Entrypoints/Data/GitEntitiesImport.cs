using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.AzureDevopsApi;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Mappers;
using AzureDevopsExplorer.Database.Model.Data;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class GitEntitiesImport
{
    private readonly VssConnection connection;
    private readonly string projectName;
    private readonly Mappers mapper;

    public GitEntitiesImport(VssConnection connection, string projectName)
    {
        this.connection = connection;
        this.projectName = projectName;
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
        using var db = new DataContext();
        Dictionary<Guid, string> existingReposById = new();
        db.GitRepository
            .Select(x => new { id = x.Id, summary = x.Url + x.Name + x.DefaultBranch + x.IsDisabled })
            .ToList()
            .ForEach(x =>
            {
                existingReposById[x.id] = x.summary;
            });

        var findQueries = new FindPullRequestsAndEntities(connection, projectName);
        var repos = await findQueries.GetRepositories();
        foreach (var repo in repos)
        {
            if (existingReposById.ContainsKey(repo.Id))
            {
                var combined = repo.Url + repo.Name + repo.DefaultBranch + (repo.IsDisabled == null ? 0 : (repo.IsDisabled.Value ? 1 : 0));
                var existingCombined = existingReposById[repo.Id];
                if (combined != existingCombined)
                {
                    var existing = db.GitRepository.Single(x => x.Id == repo.Id);
                    mapper.MapGitRepository(repo, existing);
                }
            }
            else
            {
                var obj = mapper.MapGitRepository(repo);
                db.GitRepository.Add(obj);
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

        var findQueries = new FindPullRequestsAndEntities(connection, projectName);
        List<Guid> addedIdentityIds = new();

        foreach (var repoId in existingRepoIds)
        {
            var prs = await findQueries.GetPullRequests(repoId);
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

        if (db.IdentityImport.Any(x => x.Id == id) == false)
        {
            addedIdentityIds.Add(id);
            db.IdentityImport.Add(new IdentityImport
            {
                Id = id,
                LastImport = null
            });
        }
    }
}
