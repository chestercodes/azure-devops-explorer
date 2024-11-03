using AzureDevopsExplorer.Database.Model.Pipelines;

namespace AzureDevopsExplorer.Database.Extensions;

public static class BuildContextAddBuildEntitiesExtensions
{
    private static Mappers.Mappers mapper = new Mappers.Mappers();

    public static DataContext AddBuildArtifacts(this DataContext db, IEnumerable<AzureDevopsApi.Build.BuildArtifact> buildArtifacts, int buildId)
    {
        foreach (var buildArtifact in buildArtifacts)
        {
            var newArtifact = mapper.MapBuildArtifact(buildArtifact);
            newArtifact.BuildId = buildId;
            db.BuildArtifact.Add(newArtifact);
            foreach (var prop in buildArtifact.resource.properties)
            {
                db.BuildArtifactProperty.Add(new BuildArtifactProperty
                {
                    BuildArtifactId = buildArtifact.id,
                    Name = prop.Key,
                    Value = prop.Value,
                });
            }
        }
        return db;
    }

    public static DataContext AddBuildTimeline(this DataContext db, AzureDevopsApi.Build.BuildTimeline tl, int buildId)
    {
        var timeline = mapper.MapBuildTimeline(tl);
        timeline.BuildId = buildId;
        db.BuildTimeline.Add(timeline);
        return db;
    }
}
