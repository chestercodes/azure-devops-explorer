using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Pipelines;
using Microsoft.Extensions.Logging;
using OneOf;

namespace AzureDevopsExplorer.Application.Entrypoints.Import.Pipelines;
public class BuildPipelineRunExpandedYamlAnalysis
{
    private readonly ILogger logger;
    private readonly AzureDevopsProjectDataContext dataContext;
    private readonly int YamlProcessBatchSize = 100;

    public BuildPipelineRunExpandedYamlAnalysis(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.LoggerFactory.Create(this);
        this.dataContext = dataContext;
    }

    public async Task Run(ImportConfig config)
    {
        if (config.BuildRunRuntimeExpandedYamlAnalysis)
        {
            await RunAddBuildRunIdsToTable();
            await RunAddBuildYaml();
            await RunYamlAnalysis(config);
        }
    }

    public async Task RunAddBuildRunIdsToTable()
    {
        using var db = dataContext.DataContextFactory.Create();

        var pipelinesIdsWhichUseYaml = db.Pipeline.Where(x => x.ConfigurationType == "yaml").Select(x => x.Id).Distinct();
        var missingInTable =
            from b in db.Build
            join a in db.BuildRunExpandedYamlAnalysis on b.Id equals a.BuildRunId into g
            from p in g.DefaultIfEmpty()
            where p == null && pipelinesIdsWhichUseYaml.Contains(b.DefinitionId)
            select new { id = b.Id, pipelineId = b.DefinitionId, pipelineRevision = b.DefinitionRevision };

        foreach (var row in missingInTable)
        {
            db.BuildRunExpandedYamlAnalysis.Add(new Database.Model.Pipelines.BuildRunExpandedYamlAnalysis
            {
                BuildRunId = row.id,
                PipelineId = row.pipelineId,
                PipelineRevision = row.pipelineRevision,
                State = BuildYamlAnalysisState.Initial
            });
        }

        await db.SaveChangesAsync();
    }

    public async Task RunAddBuildYaml()
    {
        logger.LogInformation($"Running build yaml analysis import");

        using var db = dataContext.DataContextFactory.Create();
        var pipelineIdsInProject = db.PipelineCurrent
            .Where(x => x.ProjectId == dataContext.Project.ProjectId)
            .Select(x => x.Id);
        var unprocessedBuilds = db.BuildRunExpandedYamlAnalysis.Where(x => x.State == BuildYamlAnalysisState.Initial && pipelineIdsInProject.Contains(x.PipelineId)).ToList();

        foreach (var buildInfo in unprocessedBuilds)
        {
            logger.LogDebug($"Running build yaml analysis import " + buildInfo.BuildRunId);

            var buildYamlLinesResult = await dataContext.Queries.Build.GetExpandedYaml(buildInfo.BuildRunId);
            if (buildYamlLinesResult.IsT1)
            {
                if (buildYamlLinesResult.AsT1.ProbablyNotTransientError)
                {
                    logger.LogError(buildYamlLinesResult.AsT1.AsError);
                    buildInfo.State = BuildYamlAnalysisState.ErrorFromApi;
                    db.SaveChanges();
                }

                continue;
            }

            var buildYaml = buildYamlLinesResult.AsT0;
            var hash = Md5Hasher.Hash(buildYaml);

            if (db.BuildRunExpandedYamlFile.Any(x => x.Hash == hash) == false)
            {
                db.BuildRunExpandedYamlFile.Add(new BuildRunExpandedYamlFile
                {
                    Hash = hash,
                    Content = buildYaml,
                    Description = $"build yaml, pipeline {buildInfo.PipelineId}, first seen on build {buildInfo.BuildRunId}",
                    Status = BuildYamlAnalysisFileStatus.NotProcessed
                });
            }

            buildInfo.State = BuildYamlAnalysisState.GotYaml;
            buildInfo.BuildYamlHash = hash;
            db.SaveChanges();
        }
    }

    public async Task RunYamlAnalysis(ImportConfig config)
    {
        bool carryOn = UnprocessedFilesExist();
        while (carryOn)
        {
            await RunYamlAnalysisBatch(config);
            carryOn = UnprocessedFilesExist();
        }
    }

    private bool UnprocessedFilesExist()
    {
        using (var db = dataContext.DataContextFactory.Create())
        {
            return db.BuildRunExpandedYamlFile.Any(x => x.Status == BuildYamlAnalysisFileStatus.NotProcessed);
        }
    }

    private async Task RunYamlAnalysisBatch(ImportConfig config)
    {
        logger.LogInformation($"Running build yaml analysis batch");
        List<(string, OneOf<ParsedYamlFile, Exception>)> parseResults = new();

        using (var db = dataContext.DataContextFactory.Create())
        {
            var unprocessedFiles =
                db.BuildRunExpandedYamlFile.Where(x => x.Status == BuildYamlAnalysisFileStatus.NotProcessed)
                .Take(YamlProcessBatchSize)
                .ToList();
            var parser = new ParseYamlFile();

            foreach (var file in unprocessedFiles)
            {
                var result = parser.ParseExpandedBuildYaml(file.Content);
                parseResults.Add((file.Hash, result));
            }

            await UpdateRefTables(db, parseResults);
        }

        Dictionary<string, int> pipelineEnvironmentRefs = new();
        Dictionary<string, int> serviceConnectionRefs = new();
        Dictionary<string, int> variableGroupRefs = new();
        Dictionary<string, int> variableRefs = new();
        using (var db = dataContext.DataContextFactory.Create())
        {
            foreach (var pe in db.BuildRunExpandedYamlEnvironmentRef.ToList())
            {
                pipelineEnvironmentRefs.Add(pe.Name, pe.Id);
            }
            foreach (var sc in db.BuildRunExpandedYamlServiceConnectionRef.ToList())
            {
                serviceConnectionRefs.Add(sc.Name, sc.Id);
            }
            foreach (var vg in db.BuildRunExpandedYamlVariableGroupRef.ToList())
            {
                variableGroupRefs.Add(vg.Name, vg.Id);
            }
            foreach (var v in db.BuildRunExpandedYamlSpecificVariableRef.ToList())
            {
                variableRefs.Add(v.Name, v.Id);
            }
        }

        foreach (var result in parseResults)
        {
            AddEntities(config, pipelineEnvironmentRefs, serviceConnectionRefs, variableGroupRefs, variableRefs, result);
        }
    }

    private void AddEntities(ImportConfig config, Dictionary<string, int> pipelineEnvironmentRefs, Dictionary<string, int> serviceConnectionRefs, Dictionary<string, int> variableGroupRefs, Dictionary<string, int> variableRefs, (string, OneOf<ParsedYamlFile, Exception>) result)
    {
        var db = dataContext.DataContextFactory.Create();
        var file = db.BuildRunExpandedYamlFile.Single(x => x.Hash == result.Item1);
        result.Item2.Switch(parsedFile =>
        {
            foreach (var name in parsedFile.PipelineEnvironments.Select(x => x.Name).Distinct())
            {
                var peId = pipelineEnvironmentRefs.ContainsKey(name) ? pipelineEnvironmentRefs[name] : throw new Exception($"Could not get ref for pipeline environment {name}");
                db.BuildRunExpandedYamlEnvironmentUsage.Add(new BuildRunExpandedYamlPipelineEnvironmentUsage
                {
                    FileHash = file.Hash,
                    PipelineEnvironmentRefId = peId
                });
            }
            foreach (var name in parsedFile.ServiceConnections.Select(x => x.Name).Distinct())
            {
                var scId = serviceConnectionRefs.ContainsKey(name) ? serviceConnectionRefs[name] : throw new Exception($"Could not get ref for service connection {name}");
                db.BuildRunExpandedYamlServiceConnectionUsage.Add(new BuildRunExpandedYamlServiceConnectionUsage
                {
                    FileHash = file.Hash,
                    ServiceConnectionRefId = scId
                });
            }
            foreach (var name in parsedFile.VariableGroups.Select(x => x.Name).Distinct())
            {
                var vgId = variableGroupRefs.ContainsKey(name) ? variableGroupRefs[name] : throw new Exception($"Could not get ref for variable group {name}");
                db.BuildRunExpandedYamlVariableGroupUsage.Add(new BuildRunExpandedYamlVariableGroupUsage
                {
                    FileHash = file.Hash,
                    VariableGroupRefId = vgId
                });
            }
            foreach (var name in parsedFile.SpecificVariables.Select(x => x.Name).Distinct())
            {
                var vId = variableRefs.ContainsKey(name) ? variableRefs[name] : throw new Exception($"Could not get ref for variable {name}");
                db.BuildRunExpandedYamlSpecificVariableUsage.Add(new BuildRunExpandedYamlSpecificVariableUsage
                {
                    FileHash = file.Hash,
                    SpecificVariableRefId = vId
                });
            }

            if (config.BuildRunYamlAnalysisKeepFileContents == false)
            {
                // the file contents take up more space than are worth
                file.Content = "";
            }

            file.Status = BuildYamlAnalysisFileStatus.Ok;
            db.SaveChanges();
        }, err =>
        {
            file.Status = BuildYamlAnalysisFileStatus.Failed;
            file.Description = err.Message;
            db.SaveChanges();
        });
    }

    private static async Task UpdateRefTables(DataContext db, List<(string, OneOf<ParsedYamlFile, Exception>)> parseResults)
    {
        var successfulFiles = parseResults.Where(x => x.Item2.IsT0).Select(x => x.Item2.AsT0);

        var distinctPipelineEnvironments =
            successfulFiles.SelectMany(x => x.PipelineEnvironments.Select(x => x.Name))
            .Distinct();
        var pipelineEnvironmentsAlreadyPresent =
            db.BuildRunExpandedYamlEnvironmentRef
            .Where(x => distinctPipelineEnvironments.Contains(x.Name))
            .Select(x => x.Name)
            .ToList();
        var pipelineEnvironmentsToAdd = distinctPipelineEnvironments.Except(pipelineEnvironmentsAlreadyPresent);
        foreach (var pe in pipelineEnvironmentsToAdd)
        {
            db.BuildRunExpandedYamlEnvironmentRef.Add(new BuildRunExpandedYamlPipelineEnvironmentRef
            {
                Name = pe
            });
        }

        var distinctServiceConnections =
            successfulFiles.SelectMany(x => x.ServiceConnections.Select(x => x.Name))
            .Distinct();
        var serviceConnectionsAlreadyPresent =
            db.BuildRunExpandedYamlServiceConnectionRef
            .Where(x => distinctServiceConnections.Contains(x.Name))
            .Select(x => x.Name)
            .ToList();
        var serviceConnectionsToAdd = distinctServiceConnections.Except(serviceConnectionsAlreadyPresent);
        foreach (var sc in serviceConnectionsToAdd)
        {
            db.BuildRunExpandedYamlServiceConnectionRef.Add(new BuildRunExpandedYamlServiceConnectionRef
            {
                Name = sc
            });
        }

        var distinctVariableGroups =
            successfulFiles.SelectMany(x => x.VariableGroups.Select(x => x.Name))
            .Distinct();
        var variableGroupsAlreadyPresent =
            db.BuildRunExpandedYamlVariableGroupRef
            .Where(x => distinctVariableGroups.Contains(x.Name))
            .Select(x => x.Name)
            .ToList();
        var variableGroupsToAdd = distinctVariableGroups.Except(variableGroupsAlreadyPresent);
        foreach (var vg in variableGroupsToAdd)
        {
            db.BuildRunExpandedYamlVariableGroupRef.Add(new BuildRunExpandedYamlVariableGroupRef
            {
                Name = vg
            });
        }

        var distinctVariables =
            successfulFiles.SelectMany(x => x.SpecificVariables.Select(x => x.Name))
            .Distinct();
        var variablesAlreadyPresent =
            db.BuildRunExpandedYamlSpecificVariableRef
            .Where(x => distinctVariables.Contains(x.Name))
            .Select(x => x.Name)
            .ToList();
        var variablesToAdd = distinctVariables.Except(variablesAlreadyPresent);
        foreach (var v in variablesToAdd)
        {
            db.BuildRunExpandedYamlSpecificVariableRef.Add(new BuildRunExpandedYamlSpecificVariableRef
            {
                Name = v
            });
        }

        await db.SaveChangesAsync();
    }
}
