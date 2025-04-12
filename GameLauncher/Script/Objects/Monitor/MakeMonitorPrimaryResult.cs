using Core.Models;
using Core.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents the result of making a monitor the primary monitor.
///   Calling the <see cref="MakeMonitorPrimaryResult.Undo" /> method will revert the primary
///   monitor to the previous primary monitor.
/// </summary>
[TypeScriptExport]
public class MakeMonitorPrimaryResult : UndoableResult {
  private readonly bool         shouldUndo;
  private readonly Win32Monitor previousPrimary;


  internal MakeMonitorPrimaryResult(
    Win32Monitor previousPrimary,
    bool shouldUndo
  ) {
    this.shouldUndo      = shouldUndo;
    this.previousPrimary = previousPrimary;
  }


  /// <inheritdoc />
  protected override async Task Reverse() {
    // To reverse, we set the previous primary monitor back to primary.
    previousPrimary.SetAsPrimaryMonitor();
  }
}
