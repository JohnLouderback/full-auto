using Microsoft.ClearScript.V8;
using Console = System.Console;

namespace GameLauncher.Script.Globals;

internal class ConsoleWrapper {
  public static void InjectIntoEngine(V8ScriptEngine engine) {
    engine.AddHostType(typeof(Console));
    engine.Execute(
      @"
            console = {
                log: value => Console.WriteLine('{0}', value),
                warn: value => Console.WriteLine('WARNING: {0}', value),
                error: value => Console.WriteLine('ERROR: {0}', value)
            };
          "
    );
  }
}
