using AzureDevopsExplorer.Application.Entrypoints.Data;
using AzureDevopsExplorer.AzureDevopsApi.Auth;
using AzureDevopsExplorer.IntegrationTests.Fixtures;
using AzureDevopsExplorer.IntegrationTests.Stubs.ApiDtos;
using AzureDevopsExplorer.Database;
using AzureDevopsExplorer.Database.Model.Data;
using Microsoft.EntityFrameworkCore;
using AzureDevopsExplorer.Application.Configuration;
using AzureDevopsExplorer.Application;
using AzureDevopsExplorer.AzureDevopsApi;

namespace AzureDevopsExplorer.IntegrationTests;

public class BuildTests : IClassFixture<WiremockFixture>
{
    public BuildTests(WiremockFixture wiremock)
    {
        WireMockFixture = wiremock;
    }

    public WiremockFixture WireMockFixture { get; }

    [Fact]
    public async Task ImportsListOfBuildsWithAllEntities()
    {
        try
        {
            ConnectionDataMapping.SetupMapping(WireMockFixture);
            var buildId1 = 1;
            var buildId2 = 2;
            var buildJson1 = new BuildJsonBuilder(buildId1);
            var buildJson2 = new BuildJsonBuilder(buildId2);
            BuildJsonMapping.Setup(WireMockFixture, buildJson1);
            BuildJsonMapping.Setup(WireMockFixture, buildJson2);
            BuildJsonListMapping.Setup(WireMockFixture, [buildJson1, buildJson2]);

            var buildArtifactId1 = 1001;
            var buildArtifactId2 = 1002;
            var buildArtifactJson1 = new BuildArtifactJsonBuilder(buildArtifactId1, "zip", buildId1);
            var buildArtifactJson2 = new BuildArtifactJsonBuilder(buildArtifactId2, "zip", buildId2);
            BuildArtifactsJsonMapping.Setup(WireMockFixture, buildId1, [buildArtifactJson1]);
            BuildArtifactsJsonMapping.Setup(WireMockFixture, buildId2, [buildArtifactJson2]);

            var buildTimeline1Builder = new BuildTimelineJsonBuilder(buildId1);
            var buildTimeline2Builder = new BuildTimelineJsonBuilder(buildId2);
            BuildTimelineJsonMapping.Setup(WireMockFixture, buildId1, buildTimeline1Builder);
            BuildTimelineJsonMapping.Setup(WireMockFixture, buildId2, buildTimeline2Builder);

            List<int> buildIds = [buildId1, buildId2];
            CleanUpBuildArtifacts([buildArtifactId1, buildArtifactId2]);
            CleanUpBuildTimelines(buildIds);
            CleanUpBuilds(buildIds);

            var dataContext = new AzureDevopsProjectDataContext(
                new Application.Domain.AzureDevopsProject(Constants.ProjectName, Guid.Parse(Constants.ProjectId)),
                new Lazy<Microsoft.VisualStudio.Services.WebApi.VssConnection>(Connection.GetFakeConnection()),
                new Lazy<AzureDevopsApi.Client.AzureDevopsApiProjectClient>((AzureDevopsApi.Client.AzureDevopsApiProjectClient)null),
                new Lazy<AzureDevopsApiProjectQueries>((AzureDevopsApiProjectQueries)null),
                () => null
                );
            var importEntrypoint = new RunImport();
            await importEntrypoint.RunProjectEntityImport(new DataConfig
            {
                BuildsAddFromStart = true,
                BuildAddArtifacts = true,
                BuildAddTimeline = true,
            },
            dataContext);
            using var db = new DataContext();

            var b1 = buildJson1.Value;
            var build1 = db.Build
                .SingleOrDefault(x => x.Id == buildId1);
            Assert.NotNull(build1);
            Assert.Equal(b1.buildNumber, build1.BuildNumber);
            Assert.Equal(b1.buildNumberRevision, build1.BuildNumberRevision);
            Assert.Equal(b1.definition.id, build1.DefinitionId);
            //Assert.Equal(b1.deleted, build1.Deleted);
            Assert.Equal(b1.finishTime, build1.FinishTime);
            Assert.Equal(b1.id, build1.Id);
            //Assert.Equal(b1.keepForever, build1.KeepForever);
            Assert.Equal(b1.lastChangedBy.id, build1.LastChangedById);
            Assert.Equal(b1.lastChangedDate, build1.LastChangedDate);
            Assert.Equal(b1.logs.id, build1.LogsId);
            Assert.Equal(b1.logs.type, build1.LogsType);
            //Assert.Equal(b1.logs.url, build1.LogsUrl);
            Assert.Equal(b1.orchestrationPlan.planId, build1.OrchestrationPlanPlanId.ToString());
            Assert.Equal(b1.parameters, build1.Parameters);
            Assert.Equal("normal", b1.priority);
            Assert.Equal(QueuePriority.Normal, build1.Priority);
            Assert.Equal(b1.project.id, build1.ProjectId.ToString());
            //Assert.Equal(b1.quality, build1.Quality);
            //Assert.Equal(b1.queueOptions, build1.QueueOptions);
            //Assert.Equal(b1.queuePosition, build1.QueuePosition);
            Assert.Equal(b1.queueTime, build1.QueueTime);
            Assert.Equal("manual", b1.reason);
            Assert.Equal(BuildReason.Manual, build1.Reason);
            Assert.Equal(b1.repository.id, build1.RepositoryId);
            Assert.Equal(b1.requestedBy.id, build1.RequestedById);
            Assert.Equal(b1.requestedFor.id, build1.RequestedForId);
            Assert.Equal("failed", b1.result);
            Assert.Equal(BuildResult.Failed, build1.Result);
            Assert.Equal(b1.retainedByRelease, build1.RetainedByRelease);
            Assert.Equal(b1.sourceBranch, build1.SourceBranch);
            Assert.Equal(b1.sourceVersion, build1.SourceVersion);
            Assert.Equal(b1.startTime, build1.StartTime);
            Assert.Equal("completed", b1.status);
            Assert.Equal(BuildStatus.Completed, build1.Status);
            Assert.Equal(b1.uri, build1.Uri.ToString());
            //Assert.Equal(b1.url, build1.Url);

            Assert.True(b1.triggerInfo.ContainsKey("isPr"));
            Assert.Equal("true", b1.triggerInfo["isPr"]);
            var build1Trigger = db.BuildTriggerInfo
                .SingleOrDefault(x => x.BuildId == buildId1);
            Assert.NotNull(build1Trigger);
            Assert.Equal("isPr", build1Trigger.Name);
            Assert.Equal(b1.triggerInfo["isPr"], build1Trigger.Value);

            var buildArtifact1 = db.BuildArtifact
                .SingleOrDefault(x => x.BuildId == build1.Id);
            Assert.NotNull(buildArtifact1);
            var ba1 = buildArtifactJson1.Value;
            Assert.Equal(ba1.name, buildArtifact1.Name);
            Assert.Equal(ba1.source, buildArtifact1.Source);
            Assert.Equal(ba1.id, buildArtifact1.Id);
            Assert.Equal(buildId1, buildArtifact1.BuildId);
            Assert.Equal(ba1.resource.data, buildArtifact1.ResourceData);
            Assert.Equal(ba1.resource.downloadUrl, buildArtifact1.ResourceDownloadUrl);
            Assert.Equal(ba1.resource.type, buildArtifact1.ResourceType);
            //Assert.Equal(ba1.resource.url, buildArtifact1.ResourceUrl);
            Assert.Equal(ba1.resource.properties.artifactsize, db.BuildArtifactProperty.SingleOrDefault(x => x.Name == "artifactsize" && x.BuildArtifactId == buildArtifactId1)?.Value);
            Assert.Equal(ba1.resource.properties.HashType, db.BuildArtifactProperty.SingleOrDefault(x => x.Name == "HashType" && x.BuildArtifactId == buildArtifactId1)?.Value);
            Assert.Equal(ba1.resource.properties.RootId, db.BuildArtifactProperty.SingleOrDefault(x => x.Name == "RootId" && x.BuildArtifactId == buildArtifactId1)?.Value);

            var buildTimeline1 = db.BuildTimeline
                .Include(x => x.Records)
                .ThenInclude(x => x.Issues)
                .ThenInclude(x => x.Data)
                .Include(x => x.Records)
                .ThenInclude(x => x.PreviousAttempts)
                .SingleOrDefault(x => x.BuildId == build1.Id);

            var bt1 = buildTimeline1Builder.Value;
            Assert.NotNull(buildTimeline1);
            Assert.Equal(buildId1, buildTimeline1.BuildId);
            Assert.Equal(bt1.changeId, buildTimeline1.ChangeId);
            Assert.Equal(bt1.id, buildTimeline1.Id);
            Assert.Equal(bt1.lastChangedBy, buildTimeline1.LastChangedBy);
            Assert.Equal(bt1.lastChangedOn, buildTimeline1.LastChangedOn);
            //Assert.Equal(bt1.url, buildTimeline1.Url);
            var btr1 = bt1.records[0];
            var btRecord1 = buildTimeline1.Records[0];
            Assert.Equal(btr1.attempt, btRecord1.Attempt);
            Assert.Equal(bt1.id, btRecord1.BuildTimelineId);
            Assert.Equal(btr1.changeId, btRecord1.ChangeId);
            Assert.Equal(btr1.details.changeId, btRecord1.DetailsChangeId);
            Assert.Equal(btr1.details.id, btRecord1.DetailsId.ToString());
            //Assert.Equal(btr1.details.url, btRecord1.DetailsUrl);
            Assert.Equal(btr1.errorCount, btRecord1.ErrorCount);
            Assert.Equal(btr1.finishTime, btRecord1.FinishTime);
            Assert.Equal(btr1.id, btRecord1.Id);
            Assert.Equal(btr1.identifier, btRecord1.Identifier);
            Assert.Equal(btr1.id, btRecord1.Issues[0].BuildTimelineRecordId);
            Assert.Equal(btr1.issues[0].category, btRecord1.Issues[0].Category);
            Assert.Equal(btr1.issues[0].data["thing1"], btRecord1.Issues[0].Data[0].Value);
            Assert.Equal(btr1.issues[0].message, btRecord1.Issues[0].Message);
            Assert.Equal("warning", btr1.issues[0].type);
            Assert.Equal(BuildIssueType.Warning, btRecord1.Issues[0].Type);
            Assert.Equal(btr1.lastModified, btRecord1.LastModified);
            Assert.Equal(btr1.log.id, btRecord1.LogId);
            Assert.Equal(btr1.log.type, btRecord1.LogType);
            //Assert.Equal(btr1.log.url, btRecord1.LogUrl);
            Assert.Equal(btr1.name, btRecord1.Name);
            Assert.Equal(btr1.order, btRecord1.Order);
            Assert.Equal(btr1.parentId, btRecord1.ParentId);
            Assert.Equal(btr1.percentComplete, btRecord1.PercentComplete);
            Assert.Equal(btr1.previousAttempts[0].attempt, btRecord1.PreviousAttempts[0].Attempt);
            Assert.Equal(btr1.id, btRecord1.PreviousAttempts[0].BuildTimelineRecordId);
            Assert.Equal(btr1.previousAttempts[0].recordId, btRecord1.PreviousAttempts[0].RecordId);
            Assert.Equal(btr1.previousAttempts[0].timelineId, btRecord1.PreviousAttempts[0].TimelineId);
            Assert.Equal(btr1.queueId, btRecord1.QueueId);
            //Assert.Equal(btr1.re, btRecord1.RecordType);
            Assert.Equal("skipped", btr1.result);
            Assert.Equal(BuildTaskResult.Skipped, btRecord1.Result);
            Assert.Equal(btr1.resultCode, btRecord1.ResultCode);
            Assert.Equal(btr1.startTime, btRecord1.StartTime);
            Assert.Equal("completed", btr1.state);
            Assert.Equal(BuildTimelineRecordState.Completed, btRecord1.State);
            Assert.Equal(btr1.task.id, btRecord1.TaskId);
            Assert.Equal(btr1.task.name, btRecord1.TaskName);
            Assert.Equal(btr1.task.version, btRecord1.TaskVersion);
            //Assert.Equal(btr1.url, btRecord1.Url.ToString());
            Assert.Equal(btr1.warningCount, btRecord1.WarningCount);
            Assert.Equal(btr1.workerName, btRecord1.WorkerName);

            var btr2 = bt1.records[1];
            Assert.NotNull(btr2);

            var bt2 = buildTimeline2Builder.Value;
            Assert.NotNull(bt2);

            var buildProject1 = db.Project.SingleOrDefault(x => x.Id == build1.ProjectId);
            Assert.NotNull(buildProject1);
            Assert.Equal(b1.project.id, buildProject1.Id.ToString());
            Assert.Equal(b1.project.name, buildProject1.Name);
            Assert.Equal(b1.project.url, buildProject1.Url);
            Assert.Equal(b1.project.lastUpdateTime, buildProject1.LastUpdateTime);
            Assert.Equal("wellFormed", b1.project.state);
            Assert.Equal(ProjectState.WellFormed, buildProject1.State);
            Assert.Equal("private", b1.project.visibility);
            Assert.Equal(ProjectVisibility.Private, buildProject1.Visibility);

            var build2 = db.Build.SingleOrDefault(x => x.Id == buildId2);
            Assert.NotNull(build2);
            var thing = WireMockFixture.AdminApi.GetRequestsAsync().Result;
        }
        catch (Exception ex)
        {
            var thing = WireMockFixture.AdminApi.GetRequestsAsync().Result;
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    private void CleanUpBuildArtifacts(IEnumerable<int> buildArtifactIds)
    {
        using var db = new DataContext();
        foreach (var id in buildArtifactIds)
        {
            db.BuildArtifactProperty.RemoveRange(db.BuildArtifactProperty.Where(x => x.BuildArtifactId == id));
            db.BuildArtifact.RemoveRange(db.BuildArtifact.Where(x => x.Id == id));
        }
        db.SaveChanges();
    }
    private void CleanUpBuilds(IEnumerable<int> buildIds)
    {
        using var db = new DataContext();
        foreach (var id in buildIds)
        {
            db.BuildTaskOrchestrationPlanReference.RemoveRange(db.BuildTaskOrchestrationPlanReference.Where(x => x.BuildId == id));
            db.BuildTriggerInfo.RemoveRange(db.BuildTriggerInfo.Where(x => x.BuildId == id));
            db.BuildTemplateParameter.RemoveRange(db.BuildTemplateParameter.Where(x => x.BuildId == id));
            db.Build.RemoveRange(db.Build.Where(x => x.Id == id));
            db.ImportState.RemoveRange(db.ImportState);
        }
        db.SaveChanges();
    }
    private void CleanUpBuildTimelines(IEnumerable<int> buildIds)
    {
        using var db = new DataContext();
        foreach (var buildId in buildIds)
        {
            foreach (var timeline in db.BuildTimeline.Where(x => x.BuildId == buildId).ToList())
            {
                foreach (var record in db.BuildTimelineRecord.Where(x => x.BuildTimelineId == timeline.Id).ToList())
                {
                    foreach (var issue in db.BuildTimelineIssue.Where(x => x.BuildTimelineRecordId == timeline.Id).ToList())
                    {
                        foreach (var issueData in db.BuildTimelineIssueData.Where(x => x.BuildIssueId == issue.Id).ToList())
                        {
                            db.BuildTimelineIssueData.Remove(issueData);
                        }
                        db.BuildTimelineIssue.Remove(issue);
                    }
                    db.BuildTimelineRecord.Remove(record);
                }
                db.BuildTimeline.Remove(timeline);
            }
        }
        db.SaveChanges();
    }

    /*    
        SELECT * FROM Build ;
        SELECT * FROM BuildArtifact ;
        SELECT * FROM BuildArtifactProperty ;
        SELECT * FROM BuildRepository ;
        SELECT * FROM BuildTemplateParameter ;
        SELECT * FROM BuildTimeline ;
        SELECT * FROM BuildTimelineAttempt ;
        SELECT * FROM BuildTimelineIssue ;
        SELECT * FROM BuildTimelineIssueData ;
        SELECT * FROM BuildTimelineRecord ;
        SELECT * FROM BuildTriggerInfo ;
        SELECT * FROM DefinitionReference ;
        SELECT * FROM IdentityRef ;
        SELECT * FROM IdentityRefLink ;
        SELECT * FROM IdentitySubjectDescriptor ;
        SELECT * FROM ImportState ;
        SELECT * FROM ReferenceLink ;
        SELECT * FROM TaskOrchestrationPlanReference ;
        SELECT * FROM TeamProjectReference ;
    */
}