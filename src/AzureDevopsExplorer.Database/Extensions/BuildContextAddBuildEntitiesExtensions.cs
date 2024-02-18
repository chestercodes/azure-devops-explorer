using AzureDevopsExplorer.Database.Model.Data;

namespace AzureDevopsExplorer.Database.Extensions;

public static class BuildContextAddBuildEntitiesExtensions
{
    private static Mappers.Mappers mapper = new Mappers.Mappers();

    public static DataContext AddBuildArtifacts(this DataContext db, IEnumerable<Microsoft.TeamFoundation.Build.WebApi.BuildArtifact> buildArtifacts, int buildId)
    {
        foreach (var buildArtifact in buildArtifacts)
        {
            var newArtifact = mapper.MapBuildArtifact(buildArtifact);
            newArtifact.BuildId = buildId;
            db.BuildArtifact.Add(newArtifact);
            foreach (var prop in buildArtifact.Resource.Properties)
            {
                db.BuildArtifactProperty.Add(new BuildArtifactProperty
                {
                    BuildArtifactId = buildArtifact.Id,
                    Name = prop.Key,
                    Value = prop.Value,
                });
            }
        }
        return db;
    }

    public static DataContext AddBuildTimeline(this DataContext db, Microsoft.TeamFoundation.Build.WebApi.Timeline tl, int buildId)
    {
        var timeline = mapper.MapBuildTimeline(tl);
        timeline.BuildId = buildId;
        db.BuildTimeline.Add(timeline);
        return db;
    }


}
