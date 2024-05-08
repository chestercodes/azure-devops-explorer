using AzureDevopsExplorer.Database;
using Microsoft.EntityFrameworkCore;
using AzureDevopsExplorer.Neo4j.NodeLoaders;

namespace AzureDevopsExplorer.Neo4j.RelationshipLoaders;

public class PipelineConsumesRelationships : ILoadRelationshipsToNeo4J
{
    public string Name => "PIPELINE_CONSUMES";

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        await AddPipelineToPipelineRelationships(loader, db);
        await AddPipelineToRepoRelationships(loader, db);
    }
    private async Task AddPipelineToPipelineRelationships(Neo4jLoader loader, DataContext db)
    {
        var pipelineRuns = db.PipelineRun.Include(x => x.ResourcesPipelines).ToList();
        List<Relationship> relationships = new();
        pipelineRuns.ForEach(pr =>
        {
            pr.ResourcesPipelines.ForEach(prp =>
            {
                relationships.Add(new Relationship
                {
                    RelationshipName = Name,
                    SourceNodeType = Pipeline.Name,
                    SourceMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = Pipeline.Props.Id, MatchValue = pr.PipelineId.ToString() }
                    },
                    DestNodeType = Pipeline.Name,
                    DestMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = Pipeline.Props.Name, MatchValue = prp.Name },
                        new RelationshipMatchPair { MatchName = Pipeline.Props.Folder, MatchValue = prp.Folder }
                    }
                });
            });
        });
        await loader.LoadRelationships(relationships);
    }

    private async Task AddPipelineToRepoRelationships(Neo4jLoader loader, DataContext db)
    {
        var pipelineRuns = db.PipelineRun.Include(x => x.ResourcesRepositories).ToList();
        List<Relationship> relationships = new();
        pipelineRuns.ForEach(pr =>
        {
            pr.ResourcesRepositories.ForEach(prr =>
            {
                if (prr.RepositoryId == null)
                {
                    // can happen if using non azureGit repos
                    return;
                }
                relationships.Add(new Relationship
                {
                    RelationshipName = Name,
                    SourceNodeType = Pipeline.Name,
                    SourceMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = Pipeline.Props.Id, MatchValue = pr.PipelineId.ToString() }
                    },
                    DestNodeType = Repository.Name,
                    DestMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = Repository.Props.Id, MatchValue = prr.RepositoryId }
                    }
                });
            });
        });
        await loader.LoadRelationships(relationships);
    }
}
