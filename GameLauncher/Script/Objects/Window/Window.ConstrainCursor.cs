using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

public partial class Window {
  /// <summary>
  ///   <para>
  ///     Constrains the cursor to the bounding box of the window. This means that the cursor
  ///     will not be able to move outside the window's bounding box until the cursor is released.
  ///   </para>
  ///   <para>
  ///     Note: The bounding box is not updated automatically if the window is resized, moved, or
  ///     closed. In those case, you should call this method again to update the cursor constraint
  ///     or release the cursor.
  ///   </para>
  /// </summary>
  /// <param name="shouldPersist">
  ///   If true, the cursor will remain constrained even after the script has finished executing.
  /// </param>
  /// <returns>
  ///   A <see cref="ConstrainCursorResult" /> that can be used to manually reverse the cursor
  ///   constraint. Alternatively, you may call the <see cref="Lib.Tasks.ReleaseCursor" /> task.
  /// </returns>
  [ScriptMember("constrainCursor")]
  public ConstrainCursorResult ConstrainCursor(bool shouldPersist) {
    var boundingBox = GetBoundingBox();

    return Lib.Tasks.ConstrainCursor(boundingBox, shouldPersist);
  }


  /// <summary>
  ///   <para>
  ///     Constrains the cursor to the bounding box of the window. This means that the cursor
  ///     will not be able to move outside the window's bounding box until the cursor is released.
  ///   </para>
  ///   <para>
  ///     Note: The bounding box is not updated automatically if the window is resized, moved, or
  ///     closed. In those case, you should call this method again to update the cursor constraint
  ///     or release the cursor.
  ///   </para>
  /// </summary>
  /// <returns>
  ///   A <see cref="ConstrainCursorResult" /> that can be used to manually reverse the cursor
  ///   constraint. Alternatively, you may call the <see cref="Lib.Tasks.ReleaseCursor" /> task.
  /// </returns>
  [ScriptMember("constrainCursor")]
  public ConstrainCursorResult ConstrainCursor() {
    const bool shouldPersist = false;
    return ConstrainCursor(shouldPersist);
  }
}
