using GameLauncherTaskGenerator;
using Microsoft.ClearScript.V8;

namespace GameLauncher.Script.Lib;

/// <summary>
///   A collection of utility functions.
/// </summary>
[EntryPointExport]
public class Utils {
  private static V8ScriptEngine?       engine;
  private static TaskCompletionSource? foreverTask;


  /// <summary>
  ///   Returns a promise that never resolves. This is useful to await at the end of a script to
  ///   keep the script running indefinitely. The script will only exit when forced.
  /// </summary>
  /// <returns> A promise that never resolves. </returns>
  public static Task Forever() {
    foreverTask ??= new TaskCompletionSource();
    return foreverTask.Task;
  }


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
