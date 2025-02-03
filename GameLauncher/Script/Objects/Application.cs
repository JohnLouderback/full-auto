using GameLauncherTaskGenerator;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Abstractly represents an executable application that is or was running on the system.
/// </summary>
[TypeScriptExport]
public class Application {
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
}
