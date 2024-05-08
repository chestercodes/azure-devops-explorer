using AzureDevopsExplorer.Database.Model.Data;

namespace AzureDevopsExplorer.Database.Extensions;

public static class BuildContextAddBuildExtensions
{
    private static Mappers.Mappers mapper = new Mappers.Mappers();

    public static DataContext AddBuild(this DataContext db, Microsoft.TeamFoundation.Build.WebApi.Build build)
    {
        db.AddIdentities(new[] {
            build.DeletedBy,
            build.LastChangedBy,
            build.RequestedBy,
            build.RequestedFor
        });

        var repositoryExists = db.BuildRepository.Any(x => x.Id == build.Repository.Id);
        if (!repositoryExists)
        {
            var newRepository = mapper.MapBuildRepository(build.Repository);
            db.BuildRepository.Add(newRepository);
        }

        var projectExists = db.Project.Any(x => x.Id == build.Project.Id);
        if (!projectExists)
        {
            var newProject = mapper.MapTeamProjectReference(build.Project);
            db.Project.Add(newProject);
        }

        var definitionExists = db.Definition.Any(x => x.Id == build.Definition.Id && build.Definition.Revision == x.Revision);
        if (!definitionExists)
        {
            var newDefinition = mapper.MapDefinitionReference(build.Definition);
            db.Definition.Add(newDefinition);
        }

        Guid? parseNullableGuid(string v)
        {
            if (string.IsNullOrWhiteSpace(v))
            {
                return null;
            }
            return Guid.Parse(v);
        }

        var newBuild = mapper.MapBuild(build);
        newBuild.RepositoryId = build.Repository.Id;
        newBuild.RequestedForId = parseNullableGuid(build.RequestedFor?.Id);
        newBuild.RequestedById = parseNullableGuid(build.RequestedBy?.Id);
        newBuild.DeletedById = parseNullableGuid(build.DeletedBy?.Id);
        newBuild.LastChangedById = parseNullableGuid(build.LastChangedBy?.Id);
        foreach (var plan in build.Plans)
        {
            var newPlan = new BuildTaskOrchestrationPlanReference
            {
                BuildId = build.Id,
                OrchestrationType = plan.OrchestrationType,
                PlanId = plan.PlanId,
            };
            db.BuildTaskOrchestrationPlanReference.Add(newPlan);
        }
        db.Add(newBuild);

        foreach (var templateParameter in build.TemplateParameters)
        {
            var newTemplateParameter = new BuildTemplateParameter
            {
                BuildId = build.Id,
                Name = templateParameter.Key,
                Value = templateParameter.Value,
            };
            db.BuildTemplateParameter.Add(newTemplateParameter);
        }

        foreach (var triggerInfo in build.TriggerInfo)
        {
            var newTriggerInfo = new BuildTriggerInfo
            {
                BuildId = build.Id,
                Name = triggerInfo.Key,
                Value = triggerInfo.Value,
            };
            db.BuildTriggerInfo.Add(newTriggerInfo);
        }

        return db;
    }
}