using AzureDevopsExplorer.Application.Configuration;

public class ApplicationConfigParser
{
    public ApplicationConfig? Parse(string[] dataActions, int[] pipelineIds, string[] processActions)
    {
        var dataCommands = ApplicationActions.GetAllDataActions;
        var dataCommandNames = dataCommands.Select(x => x.Command).ToList();
        var notRecognisedDataActions = dataActions.Where(x => dataCommandNames.Contains(x) == false).ToList();
        if (notRecognisedDataActions.Count > 0)
        {
            Console.WriteLine("Data action not recognised. Available ones are:");
            Console.WriteLine(DataInfo);
            return null;
        }

        var processCommands = ApplicationActions.GetAllProcessActions;
        var processCommandNames = processCommands.Select(x => x.Command).ToList();
        var notRecognisedProcessActions = processActions.Where(x => processCommandNames.Contains(x) == false).ToList();
        if (notRecognisedProcessActions.Count > 0)
        {
            Console.WriteLine("Process action not recognised. Available ones are:");
            Console.WriteLine(ProcessInfo);
            return null;
        }

        var initialConfig = new ApplicationConfig();

        initialConfig.DataConfig.PipelineIds = pipelineIds.ToList();

        var withDataActions = dataActions.Select(x => dataCommands.Single(y => y.Command == x))
            .Aggregate(initialConfig, (agg, el) => { return agg.Combine(el.Config); });

        var withBothActions = processActions.Select(x => processCommands.Single(y => y.Command == x))
            .Aggregate(withDataActions, (agg, el) => { return agg.Combine(el.Config); });

        return withBothActions;
    }

    public static string DataInfo => string.Join(Environment.NewLine, ApplicationActions.GetAllDataActions.Select(x => $"\t{x.Command} - {x.Info}"));
    public static string ProcessInfo => string.Join(Environment.NewLine, ApplicationActions.GetAllProcessActions.Select(x => $"\t{x.Command} - {x.Info}"));
}