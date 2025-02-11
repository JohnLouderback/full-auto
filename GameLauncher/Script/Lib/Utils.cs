using GameLauncherTaskGenerator;
using Microsoft.ClearScript.V8;

namespace GameLauncher.Script.Lib;

/// <summary>
///   A collection of utility functions.
/// </summary>
[EntryPointExport]
public class Utils {
  private static V8ScriptEngine? engine;


  public static void InjectIntoEngine(V8ScriptEngine engine) {
    Utils.engine = engine;
    engine.AddHostType("__Utils", typeof(Utils));
  }


  /// <summary>
  ///   Returns a promise that resolves after the specified number of milliseconds.
  /// </summary>
  /// <param name="milliseconds"> The number of milliseconds to wait. </param>
  /// <returns> A promise that resolves after the specified number of milliseconds. </returns>
  public static Task Wait(int milliseconds) {
    return Task.Delay(milliseconds);
  }
}
