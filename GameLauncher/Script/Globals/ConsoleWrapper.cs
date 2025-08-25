using Microsoft.ClearScript.V8;
using Console = System.Console;
using static GameLauncher.Script.Utils.ObjectUtils;

namespace GameLauncher.Script.Globals;

internal class ConsoleWrapper {
  public static string HandleToString(object? obj) {
    if (obj is string s) return s;
    return ToJsonLikeString(obj);
  }


  public static void InjectIntoEngine(V8ScriptEngine engine) {
    engine.AddHostType(typeof(Console));
    engine.Script.__handleToString = new Func<object, string>(HandleToString);
    engine.Execute(
      """
      // noinspection JSUnresolvedReference,JSUnusedLocalSymbols    
      console = {
          log: value => Console.WriteLine('{0}', __handleToString(value)),
          warn: value => Console.WriteLine('WARNING: {0}', __handleToString(value)),
          error: value => Console.WriteLine('ERROR: {0}', __handleToString(value))
      };
      """
    );
  }
}
