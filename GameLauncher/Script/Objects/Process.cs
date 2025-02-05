using Core.Utils;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a process that is or was running on the system.
/// </summary>
[TypeScriptExport]
public class Process {
  private readonly System.Diagnostics.Process process;
  private readonly V8ScriptEngine             engine;

  /// <summary>
  ///   The names of the process. For example: <c> "chrome" </c>.
  /// </summary>
  [ScriptMember("name")]
  public string Name { get; }

  /// <summary>
  ///   The full path to the process. For example:
  ///   <c> "C:\Program Files\Google\Chrome\Application\chrome.exe" </c>.
  /// </summary>
  [ScriptMember("fullPath")]
  public string FullPath { get; }

  /// <summary>
  ///   The process ID, which is unique for each process. For example: <c> 1234 </c>.
  /// </summary>
  [ScriptMember("pid")]
  public int Pid { get; }


  internal Process(V8ScriptEngine engine, System.Diagnostics.Process process) {
    this.engine  = engine;
    this.process = process;
    Name         = process.ProcessName;
    FullPath     = process.MainModule?.FileName ?? string.Empty;
    Pid          = process.Id;
  }


  /// <summary>
  ///   Forcefully terminates the process.
  /// </summary>
  [ScriptMember("kill")]
  public void Kill() {
    process.Kill();
  }


  /// <summary>
  ///   Lists the current child processes of the process.
  /// </summary>
  /// <returns> A list of child processes. </returns>
  [ScriptMember("listChildren")]
  public IList<Process> ListChildren() {
    return process.GetChildProcesses()
      .Select<System.Diagnostics.Process, Process>(
        proc =>
          new Process(engine, proc)
      )
      .ToList();
  }
}
