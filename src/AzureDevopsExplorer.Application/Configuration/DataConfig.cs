
namespace AzureDevopsExplorer.Application.Configuration;
public class DataConfig
{
    public List<int> PipelineIds { get; set; } = new();
    public bool BuildsAddFromStart { get; set; } = false;
    public bool BuildsAddLatestDefaultFromPipeline { get; set; } = false;
    public bool BuildFillImportTable { get; set; } = false;
    public bool BuildAddPipelineRun { get; set; } = false;
    public bool BuildAddArtifacts { get; set; } = false;
    public bool BuildAddTimeline { get; set; } = false;
    public bool BuildRunYamlAnalysis { get; set; } = false;
    public bool CheckConfigurationImport { get; set; } = false;
    public bool GitAddRepositories { get; set; } = false;
    public bool GitAddPullRequests { get; set; } = false;
    public bool IdentityImport { get; set; } = false;
    public bool PipelineEnvironmentImport { get; set; } = false;
    public bool PipelineRunImport { get; set; } = false;
    public bool PipelineRunTemplateImport { get; set; } = false;
    public bool ServiceEndpointAddLatest { get; set; } = false;
    public bool ServiceEndpointAddHistory { get; set; } = false;
    public bool VariableGroupAddLatest { get; set; } = false;

    public ApplicationConfig ToApplicationConfig()
    {
        return new ApplicationConfig
        {
            DataConfig = this,
        };
    }

    public DataConfig Combine(DataConfig other)
    {
        BuildsAddFromStart = BuildsAddFromStart || other.BuildsAddFromStart;
        BuildsAddLatestDefaultFromPipeline = BuildsAddLatestDefaultFromPipeline || other.BuildsAddLatestDefaultFromPipeline;
        BuildFillImportTable = BuildFillImportTable || other.BuildFillImportTable;
        BuildAddPipelineRun = BuildAddPipelineRun || other.BuildAddPipelineRun;
        BuildAddArtifacts = BuildAddArtifacts || other.BuildAddArtifacts;
        BuildAddTimeline = BuildAddTimeline || other.BuildAddTimeline;
        BuildRunYamlAnalysis = BuildRunYamlAnalysis || other.BuildRunYamlAnalysis;
        CheckConfigurationImport = CheckConfigurationImport || other.CheckConfigurationImport;
        GitAddRepositories = GitAddRepositories || other.GitAddRepositories;
        GitAddPullRequests = GitAddPullRequests || other.GitAddPullRequests;
        IdentityImport = IdentityImport || other.IdentityImport;
        PipelineEnvironmentImport = PipelineEnvironmentImport || other.PipelineEnvironmentImport;
        PipelineRunImport = PipelineRunImport || other.PipelineRunImport;
        PipelineRunTemplateImport = PipelineRunTemplateImport || other.PipelineRunTemplateImport;
        ServiceEndpointAddLatest = ServiceEndpointAddLatest || other.ServiceEndpointAddLatest;
        ServiceEndpointAddHistory = ServiceEndpointAddHistory || other.ServiceEndpointAddHistory;
        VariableGroupAddLatest = VariableGroupAddLatest || other.VariableGroupAddLatest;
        return this;
    }

    public bool AnyAzureDevopsDownloadingNeeded
    {
        get
        {
            return BuildsAddFromStart
                || BuildsAddLatestDefaultFromPipeline
                || BuildFillImportTable
                || BuildAddPipelineRun
                || BuildAddArtifacts
                || BuildAddTimeline
                || BuildRunYamlAnalysis
                || CheckConfigurationImport
                || GitAddRepositories
                || GitAddPullRequests
                || IdentityImport
                || PipelineEnvironmentImport
                || PipelineRunImport
                || PipelineRunTemplateImport
                || ServiceEndpointAddLatest
                || ServiceEndpointAddHistory
                || VariableGroupAddLatest;
        }
    }
}
