namespace AzureDevopsExplorer.Database;

using Microsoft.EntityFrameworkCore;
using AzureDevopsExplorer.Database.Model.Data;

public class DataContext : DbContext
{
    public DbSet<Build> Build { get; set; }
    public DbSet<BuildArtifact> BuildArtifact { get; set; }
    public DbSet<BuildArtifactProperty> BuildArtifactProperty { get; set; }
    public DbSet<BuildImport> BuildImport { get; set; }
    public DbSet<BuildTimeline> BuildTimeline { get; set; }
    public DbSet<BuildTimelineRecord> BuildTimelineRecord { get; set; }
    public DbSet<BuildIssue> BuildTimelineIssue { get; set; }
    public DbSet<BuildIssueData> BuildTimelineIssueData { get; set; }
    public DbSet<BuildTemplateParameter> BuildTemplateParameter { get; set; }
    public DbSet<BuildTriggerInfo> BuildTriggerInfo { get; set; }
    public DbSet<BuildRepository> BuildRepository { get; set; }
    public DbSet<BuildYamlAnalysis> BuildYamlAnalysis { get; set; }
    public DbSet<BuildYamlAnalysisFile> BuildYamlAnalysisFile { get; set; }
    public DbSet<BuildYamlAnalysisServiceConnectionRef> BuildYamlAnalysisServiceConnectionRef { get; set; }
    public DbSet<BuildYamlAnalysisServiceConnectionUsage> BuildYamlAnalysisServiceConnectionUsage { get; set; }
    public DbSet<BuildYamlAnalysisVariableGroupRef> BuildYamlAnalysisVariableGroupRef { get; set; }
    public DbSet<BuildYamlAnalysisVariableGroupUsage> BuildYamlAnalysisVariableGroupUsage { get; set; }
    public DbSet<BuildYamlAnalysisSpecificVariableRef> BuildYamlAnalysisSpecificVariableRef { get; set; }
    public DbSet<BuildYamlAnalysisSpecificVariableUsage> BuildYamlAnalysisSpecificVariableUsage { get; set; }
    public DbSet<DefinitionReference> DefinitionReference { get; set; }
    public DbSet<GitPullRequest> GitPullRequest { get; set; }
    public DbSet<GitPullRequestReview> GitPullRequestReview { get; set; }
    public DbSet<GitRepository> GitRepository { get; set; }
    public DbSet<Identity> Identity { get; set; }
    public DbSet<IdentityProperty> IdentityProperty { get; set; }
    public DbSet<IdentityImport> IdentityImport { get; set; }
    public DbSet<LatestPipelineTemplateImport> LatestPipelineTemplateImport { get; set; }
    public DbSet<PipelineImport> PipelineImport { get; set; }
    public DbSet<PipelineRun> PipelineRun { get; set; }
    public DbSet<PipelineRunPipelineInfo> PipelineRunPipelineInfo { get; set; }
    public DbSet<PipelineRunRepositoryInfo> PipelineRunRepositoryInfo { get; set; }
    public DbSet<Pipeline> Pipeline { get; set; }
    public DbSet<ReferenceLink> ReferenceLink { get; set; }
    public DbSet<ServiceEndpoint> ServiceEndpoint { get; set; }
    public DbSet<ServiceEndpointAuthorizationParameter> ServiceEndpointAuthorizationParameter { get; set; }
    public DbSet<ServiceEndpointData> ServiceEndpointData { get; set; }
    public DbSet<ServiceEndpointProjectReference> ServiceEndpointProjectReference { get; set; }
    public DbSet<ServiceEndpointExecutionHistory> ServiceEndpointExecutionHistory { get; set; }
    public DbSet<TeamProjectReference> TeamProjectReference { get; set; }
    public DbSet<TaskOrchestrationPlanReference> TaskOrchestrationPlanReference { get; set; }
    public DbSet<Variable> Variable { get; set; }
    public DbSet<VariableGroup> VariableGroup { get; set; }
    public DbSet<VariableGroupProjectReference> VariableGroupProjectReference { get; set; }

    public DbSet<ImportState> ImportState { get; set; }
    public DbSet<ImportError> ImportError { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(DatabaseConnection.SqlServerLocal);
        //options.UseSqlite(DatabaseConnection.Sqlite);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<DefinitionReference>()
            .Property(x => x.QueueStatus)
            .AddEnumMaxLengthAndConversion();
        modelBuilder.Entity<DefinitionReference>()
            .Property(x => x.Type)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<ReferenceLink>()
            .Property(x => x.SourceType)
            .AddEnumMaxLengthAndConversion();

        modelBuilder.Entity<TeamProjectReference>()
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

        modelBuilder.Entity<Identity>()
            .Property(u => u.MetaType)
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

        modelBuilder.Entity<ServiceEndpointExecutionHistory>()
            .Property(u => u.Result)
            .AddEnumMaxLengthAndConversion();

        //modelBuilder.Entity<TeamProjectReference>()
        //    .Property(u => u.Visibility)
        //    .HasConversion<string>()
        //    .HasMaxLength(50);
    }
}
