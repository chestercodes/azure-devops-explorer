using System.CommandLine;

var cliCommand = new CliCommand();
var rootCommand = cliCommand.GetRootCommand();
return await rootCommand.InvokeAsync(args);