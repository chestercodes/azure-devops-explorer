using AzureDevopsExplorer.Application.Configuration;

public class ApplicationConfigParser
{
    public ApplicationConfig? Parse(string[] dataActions, int[] pipelineIds)
    {
        var dataCommands = ApplicationActions.GetAllDataActions;
        var dataCommandNames = dataCommands.Select(x => x.Command).ToList();
        var notRecognisedDataActions = dataActions.Where(x => dataCommandNames.Contains(x) == false).ToList();
        if (notRecognisedDataActions.Count > 0)
        {
            Console.WriteLine("Data action not recognised. Available ones are:");
            Console.WriteLine(Info);
            return null;
        }

        var initialConfig = new ApplicationConfig();

        initialConfig.DataConfig.PipelineIds = pipelineIds.ToList();

        var withDataActions = dataActions.Select(x => dataCommands.Single(y => y.Command == x))
            .Aggregate(initialConfig, (agg, el) => { return agg.Combine(el.Config); });

        return withDataActions;
    }

    public static string Info => string.Join(Environment.NewLine, ApplicationActions.GetAllDataActions.Select(x => $"\t{x.Command} - {x.Info}"));
}