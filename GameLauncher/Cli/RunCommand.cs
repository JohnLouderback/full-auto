using System.ComponentModel;
using GameLauncher.Script;
using Microsoft.ClearScript.V8;
using Spectre.Console;
using Spectre.Console.Cli;
using static System.IO.Path;

namespace GameLauncher.Cli;

public class RunCommand : AsyncCommand<RunCommand.Settings> {
  public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
    using var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);

    if (settings.ScriptPath is null) {
      AnsiConsole.MarkupLine("[red]Error: No script path provided.[/]");
      return 1;
    }

    var path = IsPathRooted(settings.ScriptPath)
                 ? settings.ScriptPath
                 : GetFullPath(settings.ScriptPath);


    var scriptRunner = new ScriptRunner(path);
    await scriptRunner.RunScript();
    return 0;
  }


  public sealed class Settings : CommandSettings {
    [Description("The path to the script file to execute.")]
    [CommandArgument(0, "<path-to-script>")]
    public string ScriptPath { get; init; }

    [Description("Enables debug mode.")]
    [CommandOption("-d|--debug")]
    [DefaultValue(false)]
    public bool DebugMode { get; init; }
  }
}
