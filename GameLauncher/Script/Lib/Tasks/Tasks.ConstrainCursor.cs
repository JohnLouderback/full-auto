using Windows.Win32.Foundation;
using GameLauncher.Script.Objects;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;
using static Windows.Win32.PInvoke;

namespace GameLauncher.Script.Lib;

public static class Tasks {
  /// <summary>
  ///   Constrains the cursor to a specified bounding box on the screen. The cursor will be
  ///   unable to move outside the specified bounding box until the cursor is released.
  /// </summary>
  /// <param name="boundingBox">
  ///   The bounding box to constrain the cursor to. The bounding box must have a positive
  ///   width and height greater than zero.
  /// </param>
  /// <param name="shouldPersist">
  ///   Whether the cursor constraint should persist after the script has finished executing.
  /// </param>
  /// <returns>
  ///   A <see cref="ConstrainCursorResult" /> that can be used to manually reverse the cursor
  ///   constraint. Alternatively, you may call the <see cref="ReleaseCursor" /> task.
  /// </returns>
  /// <exception cref="ArgumentException">
  ///   Thrown when the bounding box has a width or height less than or equal to zero.
  /// </exception>
  public static ConstrainCursorResult ConstrainCursor(
    BoundingBox boundingBox,
    bool shouldPersist = false
  ) {
    ArgumentNullException.ThrowIfNull(boundingBox);

    if (boundingBox.Width <= 0 ||
        boundingBox.Height <= 0) {
      throw new ArgumentException(
        "Bounding box must have a positive width and height.",
        nameof(boundingBox)
      );
    }

    ClipCursor(
      new RECT {
        left   = boundingBox.X,
        top    = boundingBox.Y,
        right  = boundingBox.X + boundingBox.Width,
        bottom = boundingBox.Y + boundingBox.Height
      }
    );

    return new ConstrainCursorResult(!shouldPersist);
  }


  [HideFromTypeScript]
  public static ConstrainCursorResult ConstrainCursor(
    ScriptObject boundingBox,
    bool shouldPersist
  ) {
    return ConstrainCursor((BoundingBox)boundingBox, shouldPersist);
  }


  [HideFromTypeScript]
  public static ConstrainCursorResult ConstrainCursor(
    ScriptObject boundingBox
  ) {
    return ConstrainCursor((BoundingBox)boundingBox);
  }


  /// <summary>
  ///   Releases the cursor from its current clipping bounds, allowing it to move freely
  ///   outside the previously constrained area. This works both when the cursor was
  ///   constrained by the <see cref="ConstrainCursor" /> task or by any other means (i.e.,
  ///   any other application or system setting that may have constrained the cursor).
  /// </summary>
  /// <exception cref="InvalidOperationException">
  ///   Thrown when the cursor clipping fails to release.
  /// </exception>
  public static unsafe void ReleaseCursor() {
    if (!ClipCursor()) {
      throw new InvalidOperationException("Failed to release cursor clipping.");
    }
  }
}
