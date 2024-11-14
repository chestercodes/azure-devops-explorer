using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Neo4j.NodeLoaders;

namespace AzureDevopsExplorer.Neo4j.RelationshipLoaders;
public class PipelineUsesRelationships : ILoadRelationshipsToNeo4J
{
    public string Name => "PIPELINE_USES";

    public async Task Load(Neo4jLoader loader, DataContext db)
    {
        await AddPipelineToPipelineEnvironmentRelationships(loader, db);
        await AddPipelineToServiceConnectionRelationships(loader, db);
        await AddPipelineToVariableGroupRelationships(loader, db);
    }

    private async Task AddPipelineToServiceConnectionRelationships(Neo4jLoader loader, DataContext db)
    {
        var result =
            db.PipelineLatestDefaultBranchRun
            .Join(db.PipelineLatest, lpr => lpr.PipelineId, lp => lp.Id, (lpr, lp) => new { lpr, lp })
            .Join(db.BuildRunExpandedYamlAnalysis, combined => combined.lpr.Id, bya => bya.BuildRunId, (combined, bya) => new { combined, bya })
            .Join(db.BuildRunExpandedYamlFile, combined => combined.bya.BuildYamlHash, byaf => byaf.Hash, (combined, byaf) => new { combined, byaf })
            .Join(db.BuildRunExpandedYamlServiceConnectionUsage, combined => combined.byaf.Hash, byasc => byasc.FileHash, (combined, byasc) => new { combined, byasc })
            .Join(db.BuildRunExpandedYamlServiceConnectionRef, combined => combined.byasc.ServiceConnectionRefId, byascr => byascr.Id, (combined, byascr) => new
            {
                PipelineId = combined.combined.combined.combined.lpr.PipelineId,
                ServiceConnectionName = byascr.Name
            })
            .ToList();

        List<Relationship> relationships = new();
        result.ForEach(x =>
        {
            relationships.Add(new Relationship
            {
                RelationshipName = Name,
                SourceNodeType = Pipeline.Name,
                SourceMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = Pipeline.Props.Id, MatchValue = x.PipelineId.ToString() }
                    },
                DestNodeType = ServiceEndpoint.Name,
                DestMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = ServiceEndpoint.Props.Name, MatchValue = x.ServiceConnectionName }
                    }
            });
        });
        await loader.LoadRelationships(relationships);
    }

    private async Task AddPipelineToVariableGroupRelationships(Neo4jLoader loader, DataContext db)
    {
        var result =
            db.PipelineLatestDefaultBranchRun
            .Join(db.PipelineLatest, lpr => lpr.PipelineId, lp => lp.Id, (lpr, lp) => new { lpr, lp })
            .Join(db.BuildRunExpandedYamlAnalysis, combined => combined.lpr.Id, bya => bya.BuildRunId, (combined, bya) => new { combined, bya })
            .Join(db.BuildRunExpandedYamlFile, combined => combined.bya.BuildYamlHash, byaf => byaf.Hash, (combined, byaf) => new { combined, byaf })
            .Join(db.BuildRunExpandedYamlVariableGroupUsage, combined => combined.byaf.Hash, byasc => byasc.FileHash, (combined, byasc) => new { combined, byasc })
            .Join(db.BuildRunExpandedYamlVariableGroupRef, combined => combined.byasc.VariableGroupRefId, byascr => byascr.Id, (combined, byascr) => new
            {
                PipelineId = combined.combined.combined.combined.lpr.PipelineId,
                VariableGroupName = byascr.Name
            })
            .ToList();

        List<Relationship> relationships = new();
        result.ForEach(x =>
        {
            relationships.Add(new Relationship
            {
                RelationshipName = Name,
                SourceNodeType = Pipeline.Name,
                SourceMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = Pipeline.Props.Id, MatchValue = x.PipelineId.ToString() }
                    },
                DestNodeType = VariableGroup.Name,
                DestMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = VariableGroup.Props.Name, MatchValue = x.VariableGroupName }
                    }
            });
        });
        await loader.LoadRelationships(relationships);
    }

    private async Task AddPipelineToPipelineEnvironmentRelationships(Neo4jLoader loader, DataContext db)
    {
        var result =
            db.PipelineLatestDefaultBranchRun
            .Join(db.PipelineLatest, lpr => lpr.PipelineId, lp => lp.Id, (lpr, lp) => new { lpr, lp })
            .Join(db.BuildRunExpandedYamlAnalysis, combined => combined.lpr.Id, bya => bya.BuildRunId, (combined, bya) => new { combined, bya })
            .Join(db.BuildRunExpandedYamlFile, combined => combined.bya.BuildYamlHash, byaf => byaf.Hash, (combined, byaf) => new { combined, byaf })
            .Join(db.BuildRunExpandedYamlEnvironmentUsage, combined => combined.byaf.Hash, byasc => byasc.FileHash, (combined, byasc) => new { combined, byasc })
            .Join(db.BuildRunExpandedYamlEnvironmentRef, combined => combined.byasc.PipelineEnvironmentRefId, byascr => byascr.Id, (combined, byascr) => new
            {
                PipelineId = combined.combined.combined.combined.lpr.PipelineId,
                PipelineEnvironmentId = byascr.Id,
                PipelineEnvironmentName = byascr.Name
            })
            .ToList();

        List<Relationship> relationships = new();
        result.ForEach(x =>
        {
            relationships.Add(new Relationship
            {
                RelationshipName = Name,
                SourceNodeType = Pipeline.Name,
                SourceMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = Pipeline.Props.Id, MatchValue = x.PipelineId.ToString() }
                    },
                DestNodeType = PipelineEnvironment.Name,
                DestMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = PipelineEnvironment.Props.Id, MatchValue = x.PipelineEnvironmentId.ToString() }
                    }
            });
        });
        await loader.LoadRelationships(relationships);
    }
}