using GameLauncherTaskGenerator;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a window of an application or another window.
/// </summary>
[TypeScriptExport]
public class Window {
  private V8ScriptEngine engine;

  /// <summary>
  ///   The names of the process. For example: <c> "chrome" </c>.
  /// </summary>
  [ScriptMember("title")]
  public required string Title { get; init; }


  internal Window(V8ScriptEngine engine) {
    this.engine = engine;
  }
}
