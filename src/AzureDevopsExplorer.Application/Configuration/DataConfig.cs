
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
    public bool GitAddRepositories { get; set; } = false;
    public bool GitAddPullRequests { get; set; } = false;
    public bool IdentityImport { get; set; } = false;
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
        GitAddRepositories = GitAddRepositories || other.GitAddRepositories;
        GitAddPullRequests = GitAddPullRequests || other.GitAddPullRequests;
        IdentityImport = IdentityImport || other.IdentityImport;
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
                || GitAddRepositories
                || GitAddPullRequests
                || IdentityImport
                || PipelineRunImport
                || PipelineRunTemplateImport
                || ServiceEndpointAddLatest
                || ServiceEndpointAddHistory
                || VariableGroupAddLatest;
        }
    }
}
