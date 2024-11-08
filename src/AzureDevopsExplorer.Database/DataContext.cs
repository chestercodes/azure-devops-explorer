﻿namespace AzureDevopsExplorer.Database;

using Microsoft.EntityFrameworkCore;
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
    public DbSet<Definition> Definition { get; set; }
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
    public DbSet<ImportError> ImportError { get; set; }
    public DbSet<ImportState> ImportState { get; set; }
    public DbSet<PipelineCurrent> PipelineCurrent { get; set; }
    public DbSet<PipelineCurrentChange> PipelineCurrentChange { get; set; }
    public DbSet<Project> Project { get; set; }



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




    // Graph
    public DbSet<EntraApplication> EntraApplication { get; set; }
    public DbSet<GraphAppRole> GraphAppRole { get; set; }
    public DbSet<EntraGroup> EntraGroup { get; set; }



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
    public DbSet<BuildYamlAnalysis> BuildYamlAnalysis { get; set; }
    public DbSet<BuildYamlAnalysisFile> BuildYamlAnalysisFile { get; set; }
    public DbSet<BuildYamlAnalysisPipelineEnvironmentRef> BuildYamlAnalysisPipelineEnvironmentRef { get; set; }
    public DbSet<BuildYamlAnalysisPipelineEnvironmentUsage> BuildYamlAnalysisPipelineEnvironmentUsage { get; set; }
    public DbSet<BuildYamlAnalysisServiceConnectionRef> BuildYamlAnalysisServiceConnectionRef { get; set; }
    public DbSet<BuildYamlAnalysisServiceConnectionUsage> BuildYamlAnalysisServiceConnectionUsage { get; set; }
    public DbSet<BuildYamlAnalysisSpecificVariableRef> BuildYamlAnalysisSpecificVariableRef { get; set; }
    public DbSet<BuildYamlAnalysisSpecificVariableUsage> BuildYamlAnalysisSpecificVariableUsage { get; set; }
    public DbSet<BuildYamlAnalysisVariableGroupRef> BuildYamlAnalysisVariableGroupRef { get; set; }
    public DbSet<BuildYamlAnalysisVariableGroupUsage> BuildYamlAnalysisVariableGroupUsage { get; set; }
    public DbSet<LatestPipeline> LatestPipeline { get; set; }
    public DbSet<LatestPipelineDefaultRun> LatestPipelineDefaultRun { get; set; }
    public DbSet<LatestPipelineTemplateImport> LatestPipelineTemplateImport { get; set; }
    public DbSet<Pipeline> Pipeline { get; set; }
    public DbSet<PipelineImport> PipelineImport { get; set; }
    public DbSet<PipelineRun> PipelineRun { get; set; }
    public DbSet<PipelineRunPipelineInfo> PipelineRunPipelineInfo { get; set; }
    public DbSet<PipelineRunRepositoryInfo> PipelineRunRepositoryInfo { get; set; }
    public DbSet<PipelineVariable> PipelineVariable { get; set; }



    // Security
    public DbSet<AccessControl> AccessControl { get; set; }
    public DbSet<AccessControlChange> AccessControlChange { get; set; }
    public DbSet<PipelineEnvironmentPipelinePermission> PipelineEnvironmentPipelinePermission { get; set; }
    public DbSet<SecurityNamespace> SecurityNamespace { get; set; }
    public DbSet<SecurityNamespaceAction> SecurityNamespaceAction { get; set; }
    public DbSet<SecurityNamespacePermission> SecurityNamespacePermission { get; set; }
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

        modelBuilder.Entity<Definition>()
            .Property(x => x.QueueStatus)
            .AddEnumMaxLengthAndConversion();
        modelBuilder.Entity<Definition>()
            .Property(x => x.Type)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<Project>()
            .Property(u => u.State)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<BuildYamlAnalysis>()
            .Property(u => u.State)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<BuildYamlAnalysisFile>()
            .Property(u => u.Status)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<BuildImport>()
            .Property(u => u.ArtifactImportState)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<BuildImport>()
            .Property(u => u.PipelineRunImportState)
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

        modelBuilder.Entity<ServiceEndpointExecutionHistory>()
            .Property(u => u.Result)
            .AddEnumMaxLengthAndConversion();

        //modelBuilder.Entity<TeamProjectReference>()
        //    .Property(u => u.Visibility)
        //    .HasConversion<string>()
        //    .HasMaxLength(50);
    }
}
