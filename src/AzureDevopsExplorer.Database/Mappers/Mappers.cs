using AzureDevopsExplorer.AzureDevopsApi.Audit;
using AzureDevopsExplorer.AzureDevopsApi.Pipelines;
using AzureDevopsExplorer.AzureDevopsApi.ServiceEndpoints;
using AzureDevopsExplorer.Database.Model.Environment;
using AzureDevopsExplorer.Database.Model.Historical;
using AzureDevopsExplorer.Database.Model.Security;
using Riok.Mapperly.Abstractions;

namespace AzureDevopsExplorer.Database.Mappers
{
    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true, PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive)]
    public partial class Mappers
    {
        public partial Model.Pipelines.PipelineCurrent MapPipelineRef(AzureDevopsApi.Pipelines.PipelineRef x);
        public partial Model.Pipelines.Project MapTeamProjectReference(AzureDevopsApi.Core.Project x);
        [MapperIgnoreTarget(nameof(Model.Pipelines.Build.RepositoryId))]
        [MapperIgnoreTarget(nameof(Model.Pipelines.Build.RequestedById))]
        [MapperIgnoreTarget(nameof(Model.Pipelines.Build.RequestedForId))]
        [MapperIgnoreTarget(nameof(Model.Pipelines.Build.DeletedById))]
        [MapperIgnoreTarget(nameof(Model.Pipelines.Build.LastChangedById))]
        public partial Model.Pipelines.Build MapBuild(AzureDevopsApi.Build.BuildDto x);
        public partial Model.Pipelines.Definition MapDefinitionReference(AzureDevopsApi.Build.Definition x);
        //public partial Model.Pipelines.BuildRepository MapBuildRepository(Microsoft.TeamFoundation.Build.WebApi.BuildRepository x);
        public partial Model.Pipelines.BuildArtifact MapBuildArtifact(AzureDevopsApi.Build.BuildArtifact x);
        public partial Model.Pipelines.BuildTimeline MapBuildTimeline(AzureDevopsApi.Build.BuildTimeline x);
        //private BuildIssue MapToBuildIssue(global::Microsoft.TeamFoundation.Build.WebApi.Issue? source)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException(nameof(source));
        //    var target = new BuildIssue();
        //    if (source.Data != null)
        //    {
        //        var issueData = new List<BuildIssueData>();
        //        foreach (var item in source.Data)
        //        {
        //            issueData.Add(new BuildIssueData
        //            {
        //                Name = item.Key,
        //                Value = item.Value
        //            });
        //        }
        //        target.Data = issueData;
        //    }
        //    target.Type = (BuildIssueType)source.Type;
        //    target.Category = source.Category;
        //    target.Message = source.Message;
        //    return target;
        //}

        [MapperIgnoreTarget(nameof(Model.Pipelines.PipelineRun.ResourcesRepositories))]
        [MapperIgnoreTarget(nameof(Model.Pipelines.PipelineRun.ResourcesPipelines))]
        public partial Model.Pipelines.PipelineRun MapPipelineRun(AzureDevopsApi.Pipelines.PipelineRun x);
        public partial Model.Pipelines.Pipeline MapPipeline(PipelineYaml x);

        [MapperIgnoreTarget(nameof(VariableGroup.Variables))]
        public partial VariableGroup MapVariableGroup(AzureDevopsApi.DistributedTask.VariableGroup x);

        //[MapperIgnoreTarget(nameof(Model.Build.ServiceEndpoint.Data))]
        //[MapperIgnoreTarget(nameof(Model.Build.ServiceEndpoint.AuthorizationParameters))]
        //[MapperIgnoreTarget(nameof(Model.Build.ServiceEndpoint.ServiceEndpointProjectReferences))]
        public partial Model.Environment.ServiceEndpoint MapServiceEndpoint(AzureDevopsApi.ServiceEndpoints.ServiceEndpoint serviceEndpoint);
        public partial Model.Historical.ServiceEndpointExecutionHistory MapServiceEndpointExecutionHistory(Data data);

        public partial Model.Pipelines.GitRepository MapGitRepository(AzureDevopsApi.Git.GitRepository data);
        //public partial void MapGitRepository(AzureDevopsApi.Git.GitRepository data, AzureDevopsApi.Git.GitRepository existingTarget);
        public partial void MapGitRepository(Model.Pipelines.GitRepository data, Model.Pipelines.GitRepository existingTarget);

        // [MapProperty(nameof(PR.LastMergeCommit) + "." + nameof(GitCommitRef.CommitId), nameof(Model.Pipelines.GitPullRequest.LastMergeCommitId))]
        //[MapProperty(nameof(PR.LastMergeSourceCommit) + "." + nameof(GitCommitRef.CommitId), nameof(Model.Pipelines.GitPullRequest.LastMergeSourceCommitId))]
        //[MapProperty(nameof(PR.LastMergeTargetCommit) + "." + nameof(GitCommitRef.CommitId), nameof(Model.Pipelines.GitPullRequest.LastMergeTargetCommitId))]
        public partial Model.Pipelines.GitPullRequest MapGitPullRequest(AzureDevopsApi.Git.GitPullRequest data);

        public partial Model.Pipelines.Identity MapIdentity(AzureDevopsApi.Core.Identity data);

        [MapperIgnoreTarget(nameof(Model.Security.CheckConfiguration.Settings))]
        public partial Model.Security.CheckConfiguration MapCheckConfiguration(AzureDevopsApi.ApprovalsAndChecks.CheckConfiguration data);

        public partial PipelineEnvironment MapPipelineEnvironment(AzureDevopsApi.Environments.PipelineEnvironment data);

        public partial SecurityNamespace MapSecurityNamespace(AzureDevopsApi.Security.SecurityNamespace data);
        public partial SecureFile MapSecureFile(AzureDevopsApi.DistributedTask.SecureFile data);
        public partial AgentPool MapAgentPool(AzureDevopsApi.DistributedTask.AgentPool data);
        public partial AuditLog MapAuditLogEntry(DecoratedAuditLogEntry data);
    }
}
