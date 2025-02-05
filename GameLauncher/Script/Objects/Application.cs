using Core.Models;
using GameLauncher.Script.Utils;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using static Core.Utils.NativeUtils;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Abstractly represents an executable application that is or was running on the system.
/// </summary>
[TypeScriptExport]
public class Application {
  private readonly V8ScriptEngine engine;

  /// <summary>
  ///   An awaitable signal that will resolve when the application's process exits.
  /// </summary>
  [ScriptMember("exitSignal")]
  public required Task ExitSignal { get; init; }

  /// <summary>
  ///   Represents the process that is running the application.
  /// </summary>
  [ScriptMember("process")]
  public required Process Process { get; init; }


  internal Application(V8ScriptEngine engine) {
    this.engine = engine;
  }


  /// <summary>
  ///   Lists the windows of the application. It does not list the windows of child processes or child
  ///   windows.
  /// </summary>
  /// <returns> A list of windows. </returns>
  [ScriptMember("listWindows", ScriptMemberFlags.ExposeRuntimeType)]
  public IList<Window> ListWindows() {
    var list = EnumerateWindows()
      .Select(
        window => {
          //Console.WriteLine(window.ProcessName);
          return window;
        }
      )
      .Where(
        window =>
          window.ProcessName == Process.FullPath
      )
      .Select<Win32Window, Window>(
        win =>
          new Window(engine) {
            Title = win.Title
          }
      )
      .ToList();
    return list.ListToJSArray(engine);
  }
}
