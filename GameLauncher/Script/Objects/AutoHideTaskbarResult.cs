using GameLauncher.Script.Utils.CodeGenAttributes;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents the result of enabling or disabling auto-hide for the taskbar.
///   Calling the <see cref="AutoHideTaskbarResult.Undo" /> method will reverse the change,
///   restoring the taskbar's previous auto-hide state.
/// </summary>
[TypeScriptExport]
public class AutoHideTaskbarResult : UndoableResult {
  private readonly bool wasPreviouslyAutoHidden;


  internal AutoHideTaskbarResult(
    bool wasPreviouslyAutoHidden,
    bool shouldUndo
  ) {
    this.wasPreviouslyAutoHidden = wasPreviouslyAutoHidden;
    ShouldUndo                   = shouldUndo;
  }


  /// <inheritdoc />
  protected override async Task Reverse() {
    var (settings, state) = Taskbar.GetTaskbarSettings();
    if (wasPreviouslyAutoHidden) {
      settings.lParam = Taskbar.ABS_AUTOHIDE; // Turn on auto-hide
    }
    else {
      settings.lParam = 0; // Turn off auto-hide
    }

    Taskbar.SaveTaskbarSettings(settings);
  }
}
