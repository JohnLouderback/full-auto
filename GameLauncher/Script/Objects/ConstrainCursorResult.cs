using GameLauncher.Script.Utils.CodeGenAttributes;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents the result of constraining the cursor to a bounding box.
///   Calling the <see cref="ConstrainCursorResult.Undo" /> method will release the cursor, allowing
///   it to move freely again.
/// </summary>
[TypeScriptExport]
public class ConstrainCursorResult : UndoableResult {
  internal ConstrainCursorResult(
    bool shouldUndo
  ) {
    ShouldUndo = shouldUndo;
  }


  /// <inheritdoc />
  protected override async Task Reverse() {
    // To reverse, we simply call the ReleaseCursor task.
    Lib.Tasks.ReleaseCursor();
  }
}
