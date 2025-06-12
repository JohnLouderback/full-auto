using Windows.Win32;
using GameLauncher.Script.Objects;
using static Core.Utils.Macros;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   Gets the taskbar object, which represents the Windows taskbar.
  /// </summary>
  /// <returns> A <see cref="Taskbar" /> object representing the taskbar. </returns>
  public static Taskbar GetTaskbar() {
    var hwnd = PInvoke.FindWindow("Shell_TrayWnd", lpWindowName: null);

    if (hwnd == NULL) {
      throw new InvalidOperationException("Taskbar window not found.");
    }

    return new Taskbar(hwnd);
  }
}
