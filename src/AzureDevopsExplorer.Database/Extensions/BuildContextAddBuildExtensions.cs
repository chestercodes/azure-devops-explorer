using AzureDevopsExplorer.Database.Model.Pipelines;

namespace AzureDevopsExplorer.Database.Extensions;

public static class BuildContextAddBuildExtensions
{
    private static Mappers.Mappers mapper = new Mappers.Mappers();

    public static DataContext AddBuild(this DataContext db, AzureDevopsApi.Build.BuildDto build)
    {
        //db.AddIdentities(new[] {
        //    build.DeletedBy,
        //    build.LastChangedBy,
        //    build.RequestedBy,
        //    build.RequestedFor
        //});

        //var repositoryExists = db.BuildRepository.Any(x => x.Id == build.repository.id);
        //if (!repositoryExists)
        //{
        //    var newRepository = mapper.MapBuildRepository(build.repository);
        //    db.BuildRepository.Add(newRepository);
        //}

        //var projectExists = db.Project.Any(x => x.Id == build.Project.Id);
        //if (!projectExists)
        //{
        //    var newProject = mapper.MapTeamProjectReference(build.Project);
        //    db.Project.Add(newProject);
        //}

        //var definitionExists = db.Definition.Any(x => x.Id == build.definition.id && build.definition.revision == x.Revision);
        //if (!definitionExists)
        //{
        //    var newDefinition = mapper.MapDefinitionReference(build.definition);
        //    db.Definition.Add(newDefinition);
        //}

        Guid? parseNullableGuid(string v)
        {
            if (string.IsNullOrWhiteSpace(v))
            {
                return null;
            }
            return Guid.Parse(v);
        }

        var newBuild = mapper.MapBuild(build);
        newBuild.RepositoryId = build.repository.id;
        newBuild.RequestedForId = parseNullableGuid(build.requestedFor?.id);
        newBuild.RequestedById = parseNullableGuid(build.requestedBy?.id);
        //newBuild.DeletedById = parseNullableGuid(build.deletedBy?.id);
        newBuild.LastChangedById = parseNullableGuid(build.lastChangedBy?.id);
        foreach (var plan in build.plans)
        {
            var newPlan = new BuildTaskOrchestrationPlanReference
            {
                BuildId = build.id,
                OrchestrationType = plan.orchestrationType,
                PlanId = plan.planId,
            };
            db.BuildTaskOrchestrationPlanReference.Add(newPlan);
        }
        db.Add(newBuild);

        foreach (var templateParameter in build?.templateParameters ?? new Dictionary<string, string>())
        {
            var newTemplateParameter = new BuildTemplateParameter
            {
                BuildId = build.id,
                Name = templateParameter.Key,
                Value = templateParameter.Value,
            };
            db.BuildTemplateParameter.Add(newTemplateParameter);
        }

        foreach (var triggerInfo in build?.triggerInfo ?? new Dictionary<string, string>())
        {
            var newTriggerInfo = new BuildTriggerInfo
            {
                BuildId = build.id,
                Name = triggerInfo.Key,
                Value = triggerInfo.Value,
            };
            db.BuildTriggerInfo.Add(newTriggerInfo);
        }

        return db;
    }
}