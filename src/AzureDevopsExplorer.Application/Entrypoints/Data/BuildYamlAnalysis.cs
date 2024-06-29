using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Data;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using OneOf;
using System.Security.Cryptography;
using System.Text;

namespace AzureDevopsExplorer.Application.Entrypoints.Data;
public class BuildYamlAnalysis
{
    private readonly ILogger logger;
    private readonly VssConnection vssConnection;
    private readonly string projectName;

    public BuildYamlAnalysis(AzureDevopsProjectDataContext dataContext)
    {
        logger = dataContext.GetLogger();
        this.vssConnection = dataContext.VssConnection.Value;
        this.projectName = dataContext.Project.ProjectName;
    }

    public async Task Run(DataConfig config)
    {
        if (config.BuildRunYamlAnalysis)
        {
            await RunAddBuildRunIdsToTable();
            await RunAddBuildYaml();
            await RunYamlAnalysis();
        }
    }

    public async Task RunAddBuildRunIdsToTable()
    {
        using var db = new DataContext();

        var missingInTable =
            from b in db.BuildImport
            join a in db.BuildYamlAnalysis on b.BuildRunId equals a.BuildRunId into g
            from p in g.DefaultIfEmpty()
            where p == null
            select new { id = b.BuildRunId, pipelineId = b.PipelineId, pipelineRevision = b.PipelineRevision };

        foreach (var row in missingInTable)
        {
            db.BuildYamlAnalysis.Add(new Database.Model.Data.BuildYamlAnalysis
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
        var buildClient = vssConnection.GetClient<BuildHttpClient>();

        using var db = new DataContext();
        var unprocessedBuilds = db.BuildYamlAnalysis.Where(x => x.State == BuildYamlAnalysisState.Initial).ToList();

        foreach (var buildInfo in unprocessedBuilds)
        {
            try
            {
                var buildYamlLines = await buildClient.GetBuildLogLinesAsync(projectName, buildInfo.BuildRunId, 1);
                var buildYaml = string.Join(Environment.NewLine, buildYamlLines);
                var hash = GetMd5Hash(buildYaml);

                if (db.BuildYamlAnalysisFile.Any(x => x.Hash == hash) == false)
                {
                    db.BuildYamlAnalysisFile.Add(new BuildYamlAnalysisFile
                    {
                        Hash = hash,
                        Content = buildYaml,
                        Description = $"build yaml, pipeline {buildInfo.PipelineId}, first seen on build {buildInfo.BuildRunId}",
                        Status = BuildYamlAnalysisFileStatus.NotProcessed
                    });
                }

                buildInfo.State = BuildYamlAnalysisState.GotYaml;
                buildInfo.BuildYamlHash = hash;
            }
            catch (Exception ex)
            {
                buildInfo.State = BuildYamlAnalysisState.ErrorFromApi;
            }

            db.SaveChanges();
        }
    }

    public async Task RunYamlAnalysis()
    {
        List<(string, OneOf<ParsedYamlFile, Exception>)> parseResults = new();

        using (var db = new DataContext())
        {
            var unprocessedFiles = db.BuildYamlAnalysisFile.Where(x => x.Status == BuildYamlAnalysisFileStatus.NotProcessed).ToList();
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
        using (var db = new DataContext())
        {
            foreach (var pe in db.BuildYamlAnalysisPipelineEnvironmentRef.ToList())
            {
                pipelineEnvironmentRefs.Add(pe.Name, pe.Id);
            }
            foreach (var sc in db.BuildYamlAnalysisServiceConnectionRef.ToList())
            {
                serviceConnectionRefs.Add(sc.Name, sc.Id);
            }
            foreach (var vg in db.BuildYamlAnalysisVariableGroupRef.ToList())
            {
                variableGroupRefs.Add(vg.Name, vg.Id);
            }
            foreach (var v in db.BuildYamlAnalysisSpecificVariableRef.ToList())
            {
                variableRefs.Add(v.Name, v.Id);
            }
        }

        foreach (var result in parseResults)
        {
            AddEntities(pipelineEnvironmentRefs, serviceConnectionRefs, variableGroupRefs, variableRefs, result);
        }
    }

    private static void AddEntities(Dictionary<string, int> pipelineEnvironmentRefs, Dictionary<string, int> serviceConnectionRefs, Dictionary<string, int> variableGroupRefs, Dictionary<string, int> variableRefs, (string, OneOf<ParsedYamlFile, Exception>) result)
    {
        var db = new DataContext();
        var file = db.BuildYamlAnalysisFile.Single(x => x.Hash == result.Item1);
        result.Item2.Switch(parsedFile =>
        {
            foreach (var name in parsedFile.PipelineEnvironments.Select(x => x.Name).Distinct())
            {
                var peId = pipelineEnvironmentRefs.ContainsKey(name) ? pipelineEnvironmentRefs[name] : throw new Exception($"Could not get ref for pipeline environment {name}");
                db.BuildYamlAnalysisPipelineEnvironmentUsage.Add(new BuildYamlAnalysisPipelineEnvironmentUsage
                {
                    FileHash = file.Hash,
                    PipelineEnvironmentRefId = peId
                });
            }
            foreach (var name in parsedFile.ServiceConnections.Select(x => x.Name).Distinct())
            {
                var scId = serviceConnectionRefs.ContainsKey(name) ? serviceConnectionRefs[name] : throw new Exception($"Could not get ref for service connection {name}");
                db.BuildYamlAnalysisServiceConnectionUsage.Add(new BuildYamlAnalysisServiceConnectionUsage
                {
                    FileHash = file.Hash,
                    ServiceConnectionRefId = scId
                });
            }
            foreach (var name in parsedFile.VariableGroups.Select(x => x.Name).Distinct())
            {
                var vgId = variableGroupRefs.ContainsKey(name) ? variableGroupRefs[name] : throw new Exception($"Could not get ref for variable group {name}");
                db.BuildYamlAnalysisVariableGroupUsage.Add(new BuildYamlAnalysisVariableGroupUsage
                {
                    FileHash = file.Hash,
                    VariableGroupRefId = vgId
                });
            }
            foreach (var name in parsedFile.SpecificVariables.Select(x => x.Name).Distinct())
            {
                var vId = variableRefs.ContainsKey(name) ? variableRefs[name] : throw new Exception($"Could not get ref for variable {name}");
                db.BuildYamlAnalysisSpecificVariableUsage.Add(new BuildYamlAnalysisSpecificVariableUsage
                {
                    FileHash = file.Hash,
                    SpecificVariableRefId = vId
                });
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
            db.BuildYamlAnalysisPipelineEnvironmentRef
            .Where(x => distinctPipelineEnvironments.Contains(x.Name))
            .Select(x => x.Name)
            .ToList();
        var pipelineEnvironmentsToAdd = distinctPipelineEnvironments.Except(pipelineEnvironmentsAlreadyPresent);
        foreach (var pe in pipelineEnvironmentsToAdd)
        {
            db.BuildYamlAnalysisPipelineEnvironmentRef.Add(new BuildYamlAnalysisPipelineEnvironmentRef
            {
                Name = pe
            });
        }

        var distinctServiceConnections =
            successfulFiles.SelectMany(x => x.ServiceConnections.Select(x => x.Name))
            .Distinct();
        var serviceConnectionsAlreadyPresent =
            db.BuildYamlAnalysisServiceConnectionRef
            .Where(x => distinctServiceConnections.Contains(x.Name))
            .Select(x => x.Name)
            .ToList();
        var serviceConnectionsToAdd = distinctServiceConnections.Except(serviceConnectionsAlreadyPresent);
        foreach (var sc in serviceConnectionsToAdd)
        {
            db.BuildYamlAnalysisServiceConnectionRef.Add(new BuildYamlAnalysisServiceConnectionRef
            {
                Name = sc
            });
        }

        var distinctVariableGroups =
            successfulFiles.SelectMany(x => x.VariableGroups.Select(x => x.Name))
            .Distinct();
        var variableGroupsAlreadyPresent =
            db.BuildYamlAnalysisVariableGroupRef
            .Where(x => distinctVariableGroups.Contains(x.Name))
            .Select(x => x.Name)
            .ToList();
        var variableGroupsToAdd = distinctVariableGroups.Except(variableGroupsAlreadyPresent);
        foreach (var vg in variableGroupsToAdd)
        {
            db.BuildYamlAnalysisVariableGroupRef.Add(new BuildYamlAnalysisVariableGroupRef
            {
                Name = vg
            });
        }

        var distinctVariables =
            successfulFiles.SelectMany(x => x.SpecificVariables.Select(x => x.Name))
            .Distinct();
        var variablesAlreadyPresent =
            db.BuildYamlAnalysisSpecificVariableRef
            .Where(x => distinctVariables.Contains(x.Name))
            .Select(x => x.Name)
            .ToList();
        var variablesToAdd = distinctVariables.Except(variablesAlreadyPresent);
        foreach (var v in variablesToAdd)
        {
            db.BuildYamlAnalysisSpecificVariableRef.Add(new BuildYamlAnalysisSpecificVariableRef
            {
                Name = v
            });
        }

        await db.SaveChangesAsync();
    }

    private string GetMd5Hash(string buildYaml)
    {
        using (MD5 md5Hash = MD5.Create())
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(buildYaml));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
