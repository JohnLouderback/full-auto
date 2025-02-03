using GameLauncherTaskGenerator;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a process that is or was running on the system.
/// </summary>
[TypeScriptExport]
public class Process {
  /// <summary>
  ///   The names of the process. For example: <c> "chrome" </c>.
  /// </summary>
  [ScriptMember("name")]
  public required string Name { get; init; }

  /// <summary>
  ///   The full path to the process. For example:
  ///   <c> "C:\Program Files\Google\Chrome\Application\chrome.exe" </c>.
  /// </summary>
  [ScriptMember("fullPath")]
  public required string FullPath { get; init; }

  /// <summary>
  ///   The process ID, which is unique for each process. For example: <c> 1234 </c>.
  /// </summary>
  [ScriptMember("pid")]
  public required int Pid { get; init; }


  /// <summary>
  ///   Forcefully terminates the process.
  /// </summary>
  [ScriptMember("kill")]
  public void Kill() {
    var process = System.Diagnostics.Process.GetProcessById(Pid);
    process.Kill();
  }
}
