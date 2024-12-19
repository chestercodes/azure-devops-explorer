namespace AzureDevopsExplorer.Database;

using Microsoft.EntityFrameworkCore;
using AzureDevopsExplorer.Database.Model.Core;
using AzureDevopsExplorer.Database.Model.Pipelines;
using AzureDevopsExplorer.Database.Model.Graph;
using AzureDevopsExplorer.Database.Model.Security;
using AzureDevopsExplorer.Database.Model.Environment;
using AzureDevopsExplorer.Database.Model.Historical;

public interface ICreateDataContexts
{
    DataContext Create();
}

public class DataContextFactory : ICreateDataContexts
{
    private readonly string sqlConnectionString;
    private readonly DatabaseType databaseType;

    public DataContextFactory(string sqlConnectionString, DatabaseType databaseType)
    {
        this.sqlConnectionString = sqlConnectionString;
        this.databaseType = databaseType;
    }
    public DataContext Create()
    {
        return new DataContext(sqlConnectionString, databaseType);
    }
}

public class DataContext : DbContext
{
    private readonly string connectionString;
    private readonly DatabaseType databaseType;

    public DataContext(string connectionString, DatabaseType databaseType)
    {
        this.connectionString = connectionString;
        this.databaseType = databaseType;
    }

    // Core
    public DbSet<GitCommit> GitCommit { get; set; }
    public DbSet<GitPullRequest> GitPullRequest { get; set; }
    public DbSet<GitPullRequestReview> GitPullRequestReview { get; set; }
    public DbSet<GitRepository> GitRepository { get; set; }
    public DbSet<GitRepositoryChange> GitRepositoryChange { get; set; }
    public DbSet<Identity> Identity { get; set; }
    public DbSet<IdentityChange> IdentityChange { get; set; }
    //public DbSet<IdentityProperty> IdentityProperty { get; set; }
    public DbSet<IdentityImport> IdentityImport { get; set; }
    public DbSet<IdentityMemberOf> IdentityMemberOf { get; set; }
    public DbSet<IdentityMember> IdentityMember { get; set; }
    public DbSet<IdentityExpandedMemberOf> IdentityMemberOfExpanded { get; set; }
    public DbSet<IdentityExpandedMember> IdentityMemberExpanded { get; set; }
    public DbSet<ImportError> ImportError { get; set; }
    public DbSet<ImportState> ImportState { get; set; }
    public DbSet<Project> Project { get; set; }
    public DbSet<Triage> Triage { get; set; }



    // Environment
    public DbSet<AgentPool> AgentPool { get; set; }
    public DbSet<AgentPoolChange> AgentPoolChange { get; set; }
    public DbSet<CheckConfiguration> CheckConfiguration { get; set; }
    public DbSet<CheckConfigurationChange> CheckConfigurationChange { get; set; }
    public DbSet<CodeSearchKeyword> CodeSearchKeyword { get; set; }
    public DbSet<CodeSearchKeywordUsage> CodeSearchKeywordUsage { get; set; }
    public DbSet<PipelineEnvironment> PipelineEnvironment { get; set; }
    public DbSet<PipelineEnvironmentChange> PipelineEnvironmentChange { get; set; }
    public DbSet<SecureFile> SecureFile { get; set; }
    public DbSet<SecureFileChange> SecureFileChange { get; set; }
    public DbSet<ServiceEndpoint> ServiceEndpoint { get; set; }
    public DbSet<ServiceEndpointAuthorizationParameter> ServiceEndpointAuthorizationParameter { get; set; }
    public DbSet<ServiceEndpointChange> ServiceEndpointChange { get; set; }
    public DbSet<ServiceEndpointData> ServiceEndpointData { get; set; }
    public DbSet<ServiceEndpointProjectReference> ServiceEndpointProjectReference { get; set; }
    public DbSet<VariableGroup> VariableGroup { get; set; }
    public DbSet<VariableGroupChange> VariableGroupChange { get; set; }
    public DbSet<VariableGroupProjectReference> VariableGroupProjectReference { get; set; }
    public DbSet<VariableGroupVariable> VariableGroupVariable { get; set; }
    public DbSet<VariableGroupVariableContainingEntraApplicationClientId> VariableGroupVariableContainingEntraApplicationClientId { get; set; }




    // Graph
    public DbSet<EntraApplication> EntraApplication { get; set; }
    public DbSet<GraphAppRole> GraphAppRole { get; set; }
    public DbSet<EntraGroup> EntraGroup { get; set; }
    public DbSet<GraphGroup> GraphGroup { get; set; }
    public DbSet<GraphServicePrincipal> GraphServicePrincipal { get; set; }
    public DbSet<GraphUser> GraphUser { get; set; }



    // Historical
    public DbSet<AuditLog> AuditLog { get; set; }
    public DbSet<AuditLogImport> AuditLogImport { get; set; }
    public DbSet<ServiceEndpointExecutionHistory> ServiceEndpointExecutionHistory { get; set; }



    // Pipelines
    public DbSet<Build> Build { get; set; }
    public DbSet<BuildArtifact> BuildArtifact { get; set; }
    public DbSet<BuildArtifactProperty> BuildArtifactProperty { get; set; }
    public DbSet<BuildImport> BuildImport { get; set; }
    public DbSet<BuildRepository> BuildRepository { get; set; }
    public DbSet<BuildTaskOrchestrationPlanReference> BuildTaskOrchestrationPlanReference { get; set; }
    public DbSet<BuildTemplateParameter> BuildTemplateParameter { get; set; }
    public DbSet<BuildTimeline> BuildTimeline { get; set; }
    public DbSet<BuildTimelineRecord> BuildTimelineRecord { get; set; }
    public DbSet<BuildIssue> BuildTimelineIssue { get; set; }
    public DbSet<BuildIssueData> BuildTimelineIssueData { get; set; }
    public DbSet<BuildTriggerInfo> BuildTriggerInfo { get; set; }
    public DbSet<BuildRunExpandedYamlAnalysis> BuildRunExpandedYamlAnalysis { get; set; }
    public DbSet<BuildRunExpandedYamlFile> BuildRunExpandedYamlFile { get; set; }
    public DbSet<BuildRunExpandedYamlPipelineEnvironmentRef> BuildRunExpandedYamlEnvironmentRef { get; set; }
    public DbSet<BuildRunExpandedYamlPipelineEnvironmentUsage> BuildRunExpandedYamlEnvironmentUsage { get; set; }
    public DbSet<BuildRunExpandedYamlServiceConnectionRef> BuildRunExpandedYamlServiceConnectionRef { get; set; }
    public DbSet<BuildRunExpandedYamlServiceConnectionUsage> BuildRunExpandedYamlServiceConnectionUsage { get; set; }
    public DbSet<BuildRunExpandedYamlSpecificVariableRef> BuildRunExpandedYamlSpecificVariableRef { get; set; }
    public DbSet<BuildRunExpandedYamlSpecificVariableUsage> BuildRunExpandedYamlSpecificVariableUsage { get; set; }
    public DbSet<BuildRunExpandedYamlVariableGroupRef> BuildRunExpandedYamlVariableGroupRef { get; set; }
    public DbSet<BuildRunExpandedYamlVariableGroupUsage> BuildRunExpandedYamlVariableGroupUsage { get; set; }
    public DbSet<PipelineConfigurationYamlTemplate> PipelineConfigurationYamlTemplate { get; set; }
    public DbSet<PipelineConfigurationYamlTemplateImport> PipelineConfigurationYamlTemplateImport { get; set; }
    public DbSet<Pipeline> Pipeline { get; set; }
    public DbSet<PipelineCurrent> PipelineCurrent { get; set; }
    public DbSet<PipelineImport> PipelineImport { get; set; }
    public DbSet<PipelineLatest> PipelineLatest { get; set; }
    public DbSet<PipelineLatestDefaultBranchRun> PipelineLatestDefaultBranchRun { get; set; }
    public DbSet<PipelineRun> PipelineRun { get; set; }
    public DbSet<PipelineRunImport> PipelineRunImport { get; set; }
    public DbSet<PipelineRunPipelineInfo> PipelineRunPipelineInfo { get; set; }
    public DbSet<PipelineRunRepositoryInfo> PipelineRunRepositoryInfo { get; set; }
    public DbSet<PipelineVariable> PipelineVariable { get; set; }



    // Security
    public DbSet<AccessControl> AccessControl { get; set; }
    public DbSet<AccessControlChange> AccessControlChange { get; set; }
    public DbSet<PolicyConfiguration> PolicyConfiguration { get; set; }
    public DbSet<PolicyConfigurationChange> PolicyConfigurationChange { get; set; }
    public DbSet<PolicyConfigurationSetting> PolicyConfigurationSetting { get; set; }
    public DbSet<PipelineEnvironmentPipelinePermission> PipelineEnvironmentPipelinePermission { get; set; }
    public DbSet<SecurityNamespace> SecurityNamespace { get; set; }
    public DbSet<SecurityNamespaceAction> SecurityNamespaceAction { get; set; }
    public DbSet<SecurityNamespacePermission> SecurityNamespacePermission { get; set; }
    public DbSet<SecurityNamespacePermissionReport> SecurityNamespacePermissionReport { get; set; }
    public DbSet<SecurityNamespaceResourcePermission> SecurityNamespaceResourcePermission { get; set; }
    public DbSet<ServiceEndpointPipelinePermission> ServiceEndpointPipelinePermission { get; set; }
    public DbSet<VariableGroupPipelinePermission> VariableGroupPipelinePermission { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (databaseType == DatabaseType.SqlServer)
        {
            options.UseSqlServer(connectionString);
            return;
        }

        if (databaseType == DatabaseType.Sqlite)
        {
            options.UseSqlite(connectionString);
            return;
        }

        throw new NotImplementedException($"Database Type not implemented {databaseType}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogImport>()
            .Property(x => x.Status)
            .AddEnumMaxLengthAndConversion();
        modelBuilder.Entity<Build>()
            .Property(x => x.Priority)
            .AddEnumMaxLengthAndConversion();
        modelBuilder.Entity<Build>()
            .Property(x => x.QueueOptions)
            .AddEnumMaxLengthAndConversion();
        modelBuilder.Entity<Build>()
            .Property(x => x.Reason)
            .AddEnumMaxLengthAndConversion();
        modelBuilder.Entity<Build>()
            .Property(x => x.Result)
            .AddEnumMaxLengthAndConversion();
        modelBuilder.Entity<Build>()
            .Property(x => x.Status)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<BuildTimelineRecord>()
            .Property(x => x.State)
            .AddEnumMaxLengthAndConversion();
        modelBuilder.Entity<BuildTimelineRecord>()
            .Property(x => x.Result)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<BuildIssue>()
            .Property(x => x.Type)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<Project>()
            .Property(u => u.State)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<BuildRunExpandedYamlAnalysis>()
            .Property(u => u.State)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<BuildRunExpandedYamlFile>()
            .Property(u => u.Status)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<BuildImport>()
            .Property(u => u.ArtifactImportState)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<BuildImport>()
            .Property(u => u.TimelineImportState)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<GitPullRequest>()
            .Property(u => u.Status)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<GitPullRequest>()
            .Property(u => u.MergeStatus)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<GitPullRequest>()
            .Property(u => u.MergeFailureType)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<GitPullRequest>()
            .Property(u => u.CompletionOptionsMergeStrategy)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<PipelineRun>()
            .Property(u => u.State)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<PipelineRunImport>()
            .Property(u => u.PipelineRunImportState)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<PipelineImport>()
            .Property(u => u.PipelineImportState)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<PipelineRun>()
            .Property(u => u.Result)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<SecurityNamespaceResourcePermission>()
            .Property(u => u.ResourceType)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<SecurityNamespaceResourcePermission>()
            .Property(u => u.AllowOrDeny)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<SecurityNamespacePermissionReport>()
            .Property(u => u.AllowOrDeny)
            .AddEnumMaxLengthAndConversion();
        modelBuilder.Entity<SecurityNamespacePermissionReport>()
            .Property(u => u.PermissionScope)
            .AddEnumMaxLengthAndConversion();
        modelBuilder.Entity<SecurityNamespacePermissionReport>()
            .Property(u => u.ResourceType)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<ServiceEndpointExecutionHistory>()
            .Property(u => u.Result)
            .AddEnumMaxLengthAndConversion();

        //modelBuilder.Entity<TeamProjectReference>()
        //    .Property(u => u.Visibility)
        //    .HasConversion<string>()
        //    .HasMaxLength(50);
    }
}
