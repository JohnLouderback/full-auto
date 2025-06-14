using GameLauncher.Script.Objects;
using static Windows.Win32.PInvoke;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   Gets the current position of the cursor on the screen.
  /// </summary>
  /// <returns>
  ///   A <see cref="Coordinate" /> representing the current position of the cursor on the screen.
  /// </returns>
  public static Coordinate GetCursorPosition() {
    // Get the current position of the cursor.
    GetCursorPos(out var point);
    return new Coordinate {
      X = point.X,
      Y = point.Y
    };
  }


  /// <summary>
  ///   Sets the position of the cursor on the screen to the specified coordinates.
  /// </summary>
  /// <param name="x">
  ///   The X coordinate to set the cursor position to.
  /// </param>
  /// <param name="y">
  ///   The Y coordinate to set the cursor position to.
  /// </param>
  public static void SetCursorPosition(
    int x,
    int y
  ) {
    // Set the cursor position to the specified coordinates.
    SetCursorPos(x, y);
  }
}
