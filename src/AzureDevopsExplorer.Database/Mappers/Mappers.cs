using AzureDevopsExplorer.Database.Model.Data;
using Riok.Mapperly.Abstractions;
using PR = Microsoft.TeamFoundation.SourceControl.WebApi.GitPullRequest;

namespace AzureDevopsExplorer.Database.Mappers
{
    [Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, EnumMappingIgnoreCase = true, PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseInsensitive)]
    public partial class Mappers
    {
        public partial Model.Data.Project MapTeamProjectReference(Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference x);
        [MapperIgnoreTarget(nameof(Model.Data.Build.RepositoryId))]
        [MapperIgnoreTarget(nameof(Model.Data.Build.RequestedById))]
        [MapperIgnoreTarget(nameof(Model.Data.Build.RequestedForId))]
        [MapperIgnoreTarget(nameof(Model.Data.Build.DeletedById))]
        [MapperIgnoreTarget(nameof(Model.Data.Build.LastChangedById))]
        public partial Model.Data.Build MapBuild(Microsoft.TeamFoundation.Build.WebApi.Build x);
        public partial Model.Data.Definition MapDefinitionReference(Microsoft.TeamFoundation.Build.WebApi.DefinitionReference x);
        public partial Model.Data.BuildRepository MapBuildRepository(Microsoft.TeamFoundation.Build.WebApi.BuildRepository x);
        public partial Model.Data.BuildArtifact MapBuildArtifact(Microsoft.TeamFoundation.Build.WebApi.BuildArtifact x);
        public partial Model.Data.BuildTimeline MapBuildTimeline(Microsoft.TeamFoundation.Build.WebApi.Timeline x);
        private BuildIssue MapToBuildIssue(global::Microsoft.TeamFoundation.Build.WebApi.Issue? source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var target = new BuildIssue();
            if (source.Data != null)
            {
                var issueData = new List<BuildIssueData>();
                foreach (var item in source.Data)
                {
                    issueData.Add(new BuildIssueData
                    {
                        Name = item.Key,
                        Value = item.Value
                    });
                }
                target.Data = issueData;
            }
            target.Type = (BuildIssueType)source.Type;
            target.Category = source.Category;
            target.Message = source.Message;
            return target;
        }

        [MapperIgnoreTarget(nameof(Model.Data.PipelineRun.ResourcesRepositories))]
        [MapperIgnoreTarget(nameof(Model.Data.PipelineRun.ResourcesPipelines))]
        public partial Model.Data.PipelineRun MapPipelineRun(AzureDevopsApi.Dtos.PipelineRun x);
        public partial Model.Data.Pipeline MapPipeline(AzureDevopsApi.Dtos.PipelineYaml x);

        [MapperIgnoreTarget(nameof(Model.Data.VariableGroup.Variables))]
        public partial Model.Data.VariableGroup MapVariableGroup(AzureDevopsApi.Dtos.VariableGroup x);

        //[MapperIgnoreTarget(nameof(Model.Build.ServiceEndpoint.Data))]
        //[MapperIgnoreTarget(nameof(Model.Build.ServiceEndpoint.AuthorizationParameters))]
        //[MapperIgnoreTarget(nameof(Model.Build.ServiceEndpoint.ServiceEndpointProjectReferences))]
        public partial Model.Data.ServiceEndpoint MapServiceEndpoint(AzureDevopsApi.Dtos.ServiceEndpoint serviceEndpoint);
        public partial Model.Data.ServiceEndpointExecutionHistory MapServiceEndpointExecutionHistory(AzureDevopsApi.Dtos.Data data);

        public partial Model.Data.GitRepository MapGitRepository(Microsoft.TeamFoundation.SourceControl.WebApi.GitRepository data);
        public partial void MapGitRepository(Microsoft.TeamFoundation.SourceControl.WebApi.GitRepository data, Model.Data.GitRepository existingTarget);
        public partial void MapGitRepository(Model.Data.GitRepository data, Model.Data.GitRepository existingTarget);

        [MapProperty(nameof(PR.LastMergeCommit) + "." + nameof(GitCommitRef.CommitId), nameof(Model.Data.GitPullRequest.LastMergeCommitId))]
        [MapProperty(nameof(PR.LastMergeSourceCommit) + "." + nameof(GitCommitRef.CommitId), nameof(Model.Data.GitPullRequest.LastMergeSourceCommitId))]
        [MapProperty(nameof(PR.LastMergeTargetCommit) + "." + nameof(GitCommitRef.CommitId), nameof(Model.Data.GitPullRequest.LastMergeTargetCommitId))]
        public partial Model.Data.GitPullRequest MapGitPullRequest(Microsoft.TeamFoundation.SourceControl.WebApi.GitPullRequest data);

        public partial Model.Data.Identity MapIdentity(AzureDevopsApi.Dtos.Identity data);

        [MapperIgnoreTarget(nameof(Model.Data.CheckConfiguration.Settings))]
        public partial Model.Data.CheckConfiguration MapCheckConfiguration(AzureDevopsApi.Dtos.ConfigurationCheck data);

        public partial Model.Data.PipelineEnvironment MapPipelineEnvironment(AzureDevopsApi.Dtos.PipelineEnvironment data);

        public partial Model.Data.SecurityNamespace MapSecurityNamespace(AzureDevopsApi.Dtos.SecurityNamespace data);
        public partial Model.Data.SecureFile MapSecureFile(AzureDevopsApi.Dtos.SecureFile data);
        public partial Model.Data.AgentPool MapAgentPool(AzureDevopsApi.Dtos.AgentPool data);
        public partial Model.Data.AuditLog MapAuditLogEntry(AzureDevopsApi.Dtos.DecoratedAuditLogEntry data);
    }
}
