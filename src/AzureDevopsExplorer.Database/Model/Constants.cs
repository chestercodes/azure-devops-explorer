namespace AzureDevopsExplorer.Database.Model;

public class Constants
{
    // guid length
    public const int GuidLength = 36;
    // https://learn.microsoft.com/en-us/azure/devops/organizations/settings/naming-restrictions?view=azure-devops
    // org name length
    public const int OrgNameLength = 50;
    // project name length
    public const int ProjectNameLength = 64;
    // found from devops runs
    public const int BuildNameLength = 255;
    // guess
    public const int BranchRefNameLength = 255;
    public const int GitHashLength = 40;
    // devops silently cuts off pipeline name at 261 chars
    public const int PipelineNameLength = 261;
    // bit of a guess
    public const int PlanTypeLength = 50;
}