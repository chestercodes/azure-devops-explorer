namespace AzureDevopsExplorer.Application.Configuration.Actions;

public class ProcessAction
{
    public string Command { get; set; }
    public string Info { get; set; }
    public ApplicationConfig Config { get; set; }
}

public class ProcessActions
{
    public static List<ProcessAction> All
    {
        get
        {
            return new List<ProcessAction>
            {
                new ProcessAction
                {
                    Command = "update-pipeline-and-run",
                    Info = "Update latest pipeline and run tables.",
                    Config = new ProcessConfig { UpdateLatestPipelineAndRun = true }.ToApplicationConfig()
                },

                new ProcessAction
                {
                    Command = "derive-resource-permissions",
                    Info = "Derive resource permissions from ACL tables.",
                    Config = new ProcessConfig { DerivePermissions = true }.ToApplicationConfig()
                },

            };
        }
    }
}