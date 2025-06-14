using GameLauncher.Core.CodeGenAttributes;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents the result of creating a matte window around a specified window.
///   Calling the <see cref="MatteWindowResult.Undo" /> method will reverse the matte effect
///   by closing the matte window, allowing the matted window to function normally again.
/// </summary>
[TypeScriptExport]
public class MatteWindowResult : UndoableResult {
  /// <summary>
  ///   The "matte" window that was created to surround the matted window. This window is
  ///   used to create a backdrop effect around the matted window, effectively
  ///   "blacking out" the rest of the screen around it.
  /// </summary>
  public Window MatteWindow { get; }

  /// <summary>
  ///   The window that was matted. This is the original window that was modified to
  ///   create the matte effect.
  /// </summary>
  public Window MattedWindow { get; }


  internal MatteWindowResult(
    Window matteWindow,
    Window mattedWindow,
    bool shouldUndo
  ) {
    ShouldUndo   = shouldUndo;
    MatteWindow  = matteWindow;
    MattedWindow = mattedWindow;
  }


  /// <inheritdoc />
  protected override async Task Reverse() {
    // To reverse, we set the previous primary monitor back to primary.
    if (!MatteWindow.IsClosed) {
      // If the matte window is still open, we close it. If the matted window is still open,
      // the matte window will automatically "unparent" it, allowing it to function normally again.
      MatteWindow.Close();
    }
  }
}
