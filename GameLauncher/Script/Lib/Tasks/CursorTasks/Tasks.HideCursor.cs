using Windows.Win32;
using GameLauncher.Script.Objects;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   <para>
  ///     Makes the cursor invisible, effectively hiding it from the user.
  ///   </para>
  ///   <para>
  ///     This method will decrement the display count of the cursor, which means that the
  ///     cursor will not be visible until the display count reaches zero. If the display count
  ///     is already zero, the cursor will remain hidden.
  ///   </para>
  ///   <para>
  ///     If the <paramref name="force" /> parameter is set to true, the display count will be
  ///     decremented forcibly, meaning that the cursor will be hidden regardless of its current
  ///     display count. This is useful if you want to ensure that the cursor is hidden, even if
  ///     other applications or scripts have made it visible. However, this should be used with
  ///     caution, as it may interfere with the user's expectations or other applications' cursor
  ///     visibility.
  ///   </para>
  /// </summary>
  /// <param name="force">
  ///   If true, forcibly decrements the display count until the cursor is hidden.
  /// </param>
  /// <param name="shouldPersist">
  ///   If true, the cursor will remain hidden even after the script has finished executing.
  /// </param>
  public static HideCursorResult HideCursor(bool force = false, bool shouldPersist = false) {
    // Hide the cursor by setting its visibility to false.
    if (force) {
      while (PInvoke.ShowCursor(false) >= 0) {
        // Keep decrementing the display count until it reaches zero.
      }
    }
    else {
      PInvoke.ShowCursor(false);
    }

    return new HideCursorResult(!shouldPersist);
  }


  /// <summary>
  ///   <para>
  ///     Makes the cursor visible, allowing it to be seen by the user.
  ///   </para>
  ///   <para>
  ///     This method will increment the display count of the cursor, which means that the
  ///     cursor will be visible until the display count reaches zero. If the display count
  ///     is already zero, the cursor will remain visible.
  ///   </para>
  ///   <para>
  ///     If the <paramref name="force" /> parameter is set to true, the display count will be
  ///     incremented forcibly, meaning that the cursor will be made visible regardless of its
  ///     current display count. This is useful if you want to ensure that the cursor is visible,
  ///     even if other applications or scripts have made it invisible. However, this should be
  ///     used with caution, as it may interfere with the user's expectations or other applications'
  ///     cursor visibility.
  ///   </para>
  /// </summary>
  /// <param name="force">
  ///   If true, forcibly increments the display count until the cursor is shown.
  /// </param>
  public static void ShowCursor(bool force = false) {
    // Hide the cursor by setting its visibility to true.
    if (force) {
      while (PInvoke.ShowCursor(true) < 0) {
        // Keep incrementing the display count until it is visible.
      }
    }
    else {
      PInvoke.ShowCursor(true);
    }
  }
}
