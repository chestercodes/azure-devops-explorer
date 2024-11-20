using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application.Domain;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Environment;
using AzureDevopsExplorer.Database.Model.Graph;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AzureDevopsExplorer.Application.Entrypoints.Evaluate;

public class ScanVariables
{
    private readonly ICreateDataContexts dataContextFactory;

    public ScanVariables(ICreateDataContexts dataContextFactory)
    {
        this.dataContextFactory = dataContextFactory;
    }

    public async Task Run(ProcessConfig config)
    {
        if (config.ScanVariables)
        {
            await ScanVariableGroupsForSecrets();
            await ScanPipelineVariablesForSecrets();
        }

        if (config.ScanVariablesForEntraApplications)
        {
            await ScanVariableGroupsForEntraApplications();
        }
    }

    private async Task ScanVariableGroupsForEntraApplications()
    {
        using var db = dataContextFactory.Create();
        db.VariableGroupVariableContainingEntraApplicationClientId.RemoveRange(db.VariableGroupVariableContainingEntraApplicationClientId);

        var valuesToAdd =
            db.VariableGroupVariable.Where(x => x.Value != null)
            .Join(
                db.EntraApplication,
                v => v.Value.ToLower(),
                ea => ea.AppId.ToString().ToLower(),
                (v, ea) =>
                new VariableGroupVariableContainingEntraApplicationClientId
                {
                    VariableGroupId = v.VariableGroupId,
                    VariableName = v.Name,
                    ApplicationClientId = Guid.Parse(v.Value),
                    ApplicationId = ea.Id,
                    ApplicationDisplayName = ea.DisplayName
                }
                );
        db.VariableGroupVariableContainingEntraApplicationClientId.AddRange(valuesToAdd);

        await db.SaveChangesAsync();
    }

    public async Task ScanVariableGroupsForSecrets()
    {
        using var db = dataContextFactory.Create();

        var variableGroups = await db.VariableGroup
            .Include(x => x.Variables)
            .Include(x => x.VariableGroupProjectReferences)
            .ToListAsync();

        foreach (var variableGroup in variableGroups)
        {
            foreach (var variable in variableGroup.Variables)
            {
                var (isInteresting, reason) = ScanVar(variable.Name, variable.Value);
                if (isInteresting)
                {
                    var valueHash = Md5Hasher.Hash(variable.Value);
                    var key = $"VARIABLEGROUP_{variable.VariableGroupId}_{variable.Name}_{valueHash}";
                    var message = $"Variable group {variable.VariableGroupId} has variable {variable.Name} which {reason}";
                    if (db.Triage.Any(x => x.TriageKey == key) == false)
                    {
                        db.Triage.Add(new Database.Model.Core.Triage
                        {
                            TriageKey = key,
                            Message = message,
                            Category = "Variable Scan",
                            State = Database.Model.Core.TriageState.New
                        });
                    }
                }
            }
        }

        await db.SaveChangesAsync();
    }

    public async Task ScanPipelineVariablesForSecrets()
    {
        using var db = dataContextFactory.Create();

        var pipelineVariables = await db.PipelineVariable.ToListAsync();
        foreach (var variable in pipelineVariables)
        {
            var (isInteresting, reason) = ScanVar(variable.Name, variable.Value);
            if (isInteresting)
            {
                var valueHash = Md5Hasher.Hash(variable.Value);
                var key = $"VARIABLEPIPELINE_{variable.PipelineId}_{variable.Name}_{valueHash}";
                var message = $"Pipeline variable {variable.PipelineId} has variable {variable.Name} which {reason}";
                if (db.Triage.Any(x => x.TriageKey == key) == false)
                {
                    db.Triage.Add(new Database.Model.Core.Triage
                    {
                        TriageKey = key,
                        Message = message,
                        Category = "Variable Scan",
                        State = Database.Model.Core.TriageState.New
                    });
                }
            }

            await db.SaveChangesAsync();
        }
    }

    private (bool IsInteresting, string Reason) ScanVar(string variableName, string? variableValue)
    {
        var name = variableName.ToLower();
        var val = variableValue?.ToLower() ?? "";

        var nameContainsKey = name.Contains("key");
        var isSameLengthAsApimKey = val.Length == 32;

        if (nameContainsKey && isSameLengthAsApimKey)
        {
            return (true, "Looks like it is a subs key");
        }

        var valContainsSecret =
            val.Contains("password")
                || val.Contains("pwd")
                || val.Contains("sharedaccesskey")
                || val.Contains("sharedaccesskeyname")
                || val.Contains("accountkey");

        if (valContainsSecret)
        {
            return (true, "Looks like it has a secret in it");
        }

        return (false, "");
    }
}
