using Core.Models;
using Core.Utils;
using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Abstractly represents an executable application that is or was running on the system.
/// </summary>
[TypeScriptExport]
public class Application : ObjectBase {
  private readonly ScriptEngine engine;

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


  internal Application() {
    engine = AppState.ScriptEngine;
  }


  /// <summary>
  ///   Lists the windows of the application. It does not list the windows of child processes or child
  ///   windows.
  /// </summary>
  /// <returns> A list of windows. </returns>
  [ScriptMember("listWindows", ScriptMemberFlags.ExposeRuntimeType)]
  public JSArray<Window> ListWindows() {
    var list = EnumerateWindows()
      .Where(
        // Where the window is a top-level window. i.e. has no owner and is not a child window.
        window => !window.Hwnd.HasParent() && !window.Hwnd.HasOwner()
      )
      .Select<Win32Window, Window>(
        win =>
          new Window(win)
      )
      .ToList();

    return JSArray<Window>.FromIEnumerable(list);
  }


  /// <summary>
  ///   Lists the windows of the application. It lists all windows belonging to the application,
  ///   including child windows. If <paramref name="includeChildProcesses" /> is <see langword="true" />,
  ///   then it will also list windows of child processes.
  /// </summary>
  /// <param name="includeChildProcesses">
  ///   Whether to additionally include windows of child processes.
  /// </param>
  /// <returns> A list of windows. </returns>
  [ScriptMember("listWindowsDeep", ScriptMemberFlags.ExposeRuntimeType)]
  public JSArray<Window> ListWindowsDeep(bool includeChildProcesses = false) {
    var list = EnumerateWindows(includeChildProcesses)
      .Select<Win32Window, Window>(
        win =>
          new Window(win)
      )
      .ToList();

    return JSArray<Window>.FromIEnumerable(list);
  }


  private IEnumerable<Win32Window> EnumerateWindows(bool includeChildProcesses = false) {
    // If we should include child processes, then get the process IDs of the child processes.
    var childProcessIDs = includeChildProcesses
                            ? Process.ListChildren().Select(child => child.Pid).ToArray()
                            : [];

    return NativeUtils.EnumerateWindows()
      .Where(
        window => {
          if (!includeChildProcesses) {
            return window.ProcessID == Process.Pid;
          }

          // If we should include child processes, then check if the window belongs to the process
          // or any of its children.
          return window.ProcessID == Process.Pid || childProcessIDs.Contains((int)window.ProcessID);
        }
      );
  }
}
