using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Neo4j;
using Microsoft.EntityFrameworkCore;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;

public class LoadLatest
{
    public async Task Run()
    {
        using var driver = new Neo4jDriver();
        var loader = new Loader(driver);
        using (var db = new DataContext())
        {
            await DeleteRelationships(loader);
            await LoadRepositories(loader, db);
            await LoadPipelines(loader, db);
            await LoadServiceConnections(loader, db);
            await LoadPipelineEnvironment(loader, db);
            await LoadVariableGroups(loader, db);
            await LoadCheckConfigurations(loader, db);
            await AddCheckConfigurationRelationships(loader, db);
            await AddPipelineToPipelineRelationships(loader, db);
            await AddPipelineToRepoRelationships(loader, db);
            await AddPipelineToPipelineEnvironmentRelationships(loader, db);
            await AddPipelineToServiceConnectionRelationships(loader, db);
            await AddPipelineToVariableGroupRelationships(loader, db);
        }
    }

    private static async Task DeleteRelationships(Loader loader)
    {
        await loader.DeleteAllRelationships([
            "PIPELINE_CONSUMES",
            "PIPELINE_USES",
            "HAS_CHECK_CONFIGURATION"
            ]);
    }

    private static async Task LoadRepositories(Loader loader, DataContext db)
    {
        var repos = db.GitRepository.Select(x => new { x.Id, x.Name, x.ProjectReferenceId }).ToList();
        var repoVals = repos.Select(x => new Dictionary<string, string> {
                { "id", x.Id.ToString() },
                { "name", x.Name },
                { "projectId", x.ProjectReferenceId.Value.ToString() }
            });
        await loader.DeleteThenLoadNodes("Repository", repoVals.ToArray());
    }

    private static async Task LoadPipelines(Loader loader, DataContext db)
    {
        var pipelines = db.PipelineRun.Select(x => new { x.PipelineName, x.PipelineId, x.PipelineFolder }).ToList();
        var pipelineVals = pipelines.Select(x => new Dictionary<string, string> {
                { "id", x.PipelineId.ToString() },
                { "name", x.PipelineName },
                { "folder", x.PipelineFolder }
            });
        await loader.DeleteThenLoadNodes("Pipeline", pipelineVals.ToArray());
    }

    private static async Task LoadServiceConnections(Loader loader, DataContext db)
    {
        var serviceConnections = db.ServiceEndpoint.Select(x => new { x.Name, x.Id }).ToList();
        var serviceConnectionsVals = serviceConnections.Select(x => new Dictionary<string, string> {
                { "id", x.Id.ToString() },
                { "name", x.Name }
            });
        await loader.DeleteThenLoadNodes("ServiceEndpoint", serviceConnectionsVals.ToArray());
    }

    private static async Task LoadPipelineEnvironment(Loader loader, DataContext db)
    {
        var pipelineEnvironments = db.PipelineEnvironment.Select(x => new { x.Name, x.Id, x.ProjectId }).ToList();
        var pipelineEnvironmentsVals = pipelineEnvironments.Select(x => new Dictionary<string, string> {
                { "id", x.Id.ToString() },
                { "name", x.Name },
                { "projectId", x.ProjectId.ToString() }
            });
        await loader.DeleteThenLoadNodes("PipelineEnvironment", pipelineEnvironmentsVals.ToArray());
    }

    private static async Task LoadVariableGroups(Loader loader, DataContext db)
    {
        var variableGroups = db.VariableGroup.Select(x => new { x.Name, x.Id }).ToList();
        var variableGroupsVals = variableGroups.Select(x => new Dictionary<string, string> {
                { "id", x.Id.ToString() },
                { "name", x.Name }
            });
        await loader.DeleteThenLoadNodes("VariableGroup", variableGroupsVals.ToArray());
    }

    private static async Task LoadCheckConfigurations(Loader loader, DataContext db)
    {
        var checkConfigs = db.CheckConfiguration.Select(x => new { x.Settings, x.Id }).ToList();
        var checkConfigsVals = checkConfigs.Select(x => new Dictionary<string, string> {
                { "id", x.Id.ToString() },
                { "settings", x.Settings }
            });
        await loader.DeleteThenLoadNodes("CheckConfiguration", checkConfigsVals.ToArray());
    }

    private static async Task AddPipelineToPipelineRelationships(Loader loader, DataContext db)
    {
        var pipelineRuns = db.PipelineRun.Include(x => x.ResourcesPipelines).ToList();
        List<Relationship> relationships = new();
        pipelineRuns.ForEach(pr =>
        {
            pr.ResourcesPipelines.ForEach(prp =>
            {
                relationships.Add(new Relationship
                {
                    RelationshipName = "PIPELINE_CONSUMES",
                    SourceNodeType = "Pipeline",
                    SourceMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = "id", MatchValue = pr.PipelineId.ToString() }
                    },
                    DestNodeType = "Pipeline",
                    DestMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = "name", MatchValue = prp.Name },
                        new RelationshipMatchPair { MatchName = "folder", MatchValue = prp.Folder }
                    }
                });
            });
        });
        await loader.LoadRelationships(relationships);
    }

    private static async Task AddPipelineToRepoRelationships(Loader loader, DataContext db)
    {
        var pipelineRuns = db.PipelineRun.Include(x => x.ResourcesRepositories).ToList();
        List<Relationship> relationships = new();
        pipelineRuns.ForEach(pr =>
        {
            pr.ResourcesRepositories.ForEach(prr =>
            {
                if (prr.RepositoryId == null)
                {
                    return;
                }
                relationships.Add(new Relationship
                {
                    RelationshipName = "PIPELINE_CONSUMES",
                    SourceNodeType = "Pipeline",
                    SourceMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = "id", MatchValue = pr.PipelineId.ToString() }
                    },
                    DestNodeType = "Repository",
                    DestMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = "id", MatchValue = prr.RepositoryId }
                    }
                });
            });
        });
        await loader.LoadRelationships(relationships);
    }

    private static async Task AddCheckConfigurationRelationships(Loader loader, DataContext db)
    {
        var checkConfigs = db.CheckConfiguration.ToList();
        List<Relationship> relationships = new();
        checkConfigs.ForEach(cc =>
        {
            relationships.Add(new Relationship
            {
                RelationshipName = "HAS_CHECK_CONFIGURATION",
                SourceNodeType = ResourceTypeToSourceNodeType(cc.ResourceType),
                SourceMatches = new List<RelationshipMatchPair> {
                    new RelationshipMatchPair { MatchName = "id", MatchValue = cc.ResourceId.ToString() }
                },
                DestNodeType = "CheckConfiguration",
                DestMatches = new List<RelationshipMatchPair> {
                    new RelationshipMatchPair { MatchName = "id", MatchValue = cc.Id.ToString() }
                }
            });
        });
        await loader.LoadRelationships(relationships);
    }

    private static string ResourceTypeToSourceNodeType(string? resourceType)
    {
        switch (resourceType.ToLower())
        {
            case "endpoint": return "ServiceEndpoint";
            case "environment": return "PipelineEnvironment";
            case "variablegroup": return "VariableGroup";
            default: throw new NotImplementedException($"Count not find resource type '{resourceType}'");
        }
    }

    private static async Task AddPipelineToServiceConnectionRelationships(Loader loader, DataContext db)
    {
        var result =
            db.LatestPipelineDefaultRun
            .Join(db.LatestPipeline, lpr => lpr.PipelineId, lp => lp.Id, (lpr, lp) => new { lpr, lp })
            .Join(db.BuildYamlAnalysis, combined => combined.lpr.Id, bya => bya.BuildRunId, (combined, bya) => new { combined, bya })
            .Join(db.BuildYamlAnalysisFile, combined => combined.bya.BuildYamlHash, byaf => byaf.Hash, (combined, byaf) => new { combined, byaf })
            .Join(db.BuildYamlAnalysisServiceConnectionUsage, combined => combined.byaf.Hash, byasc => byasc.FileHash, (combined, byasc) => new { combined, byasc })
            .Join(db.BuildYamlAnalysisServiceConnectionRef, combined => combined.byasc.ServiceConnectionRefId, byascr => byascr.Id, (combined, byascr) => new
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
                RelationshipName = "PIPELINE_USES",
                SourceNodeType = "Pipeline",
                SourceMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = "id", MatchValue = x.PipelineId.ToString() }
                    },
                DestNodeType = "ServiceEndpoint",
                DestMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = "name", MatchValue = x.ServiceConnectionName }
                    }
            });
        });
        await loader.LoadRelationships(relationships);
    }

    private static async Task AddPipelineToVariableGroupRelationships(Loader loader, DataContext db)
    {
        var result =
            db.LatestPipelineDefaultRun
            .Join(db.LatestPipeline, lpr => lpr.PipelineId, lp => lp.Id, (lpr, lp) => new { lpr, lp })
            .Join(db.BuildYamlAnalysis, combined => combined.lpr.Id, bya => bya.BuildRunId, (combined, bya) => new { combined, bya })
            .Join(db.BuildYamlAnalysisFile, combined => combined.bya.BuildYamlHash, byaf => byaf.Hash, (combined, byaf) => new { combined, byaf })
            .Join(db.BuildYamlAnalysisVariableGroupUsage, combined => combined.byaf.Hash, byasc => byasc.FileHash, (combined, byasc) => new { combined, byasc })
            .Join(db.BuildYamlAnalysisVariableGroupRef, combined => combined.byasc.VariableGroupRefId, byascr => byascr.Id, (combined, byascr) => new
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
                RelationshipName = "PIPELINE_USES",
                SourceNodeType = "Pipeline",
                SourceMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = "id", MatchValue = x.PipelineId.ToString() }
                    },
                DestNodeType = "VariableGroup",
                DestMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = "name", MatchValue = x.VariableGroupName }
                    }
            });
        });
        await loader.LoadRelationships(relationships);
    }

    private static async Task AddPipelineToPipelineEnvironmentRelationships(Loader loader, DataContext db)
    {
        var result =
            db.LatestPipelineDefaultRun
            .Join(db.LatestPipeline, lpr => lpr.PipelineId, lp => lp.Id, (lpr, lp) => new { lpr, lp })
            .Join(db.BuildYamlAnalysis, combined => combined.lpr.Id, bya => bya.BuildRunId, (combined, bya) => new { combined, bya })
            .Join(db.BuildYamlAnalysisFile, combined => combined.bya.BuildYamlHash, byaf => byaf.Hash, (combined, byaf) => new { combined, byaf })
            .Join(db.BuildYamlAnalysisPipelineEnvironmentUsage, combined => combined.byaf.Hash, byasc => byasc.FileHash, (combined, byasc) => new { combined, byasc })
            .Join(db.BuildYamlAnalysisPipelineEnvironmentRef, combined => combined.byasc.PipelineEnvironmentRefId, byascr => byascr.Id, (combined, byascr) => new
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
                RelationshipName = "PIPELINE_USES",
                SourceNodeType = "Pipeline",
                SourceMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = "id", MatchValue = x.PipelineId.ToString() }
                    },
                DestNodeType = "PipelineEnvironment",
                DestMatches = new List<RelationshipMatchPair> {
                        new RelationshipMatchPair { MatchName = "id", MatchValue = x.PipelineEnvironmentId.ToString() }
                    }
            });
        });
        await loader.LoadRelationships(relationships);
    }
}
