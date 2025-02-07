using Core.Models;
using Core.Utils;
using GameLauncher.Script.Objects;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   Waits for a window to be spawned with the specified title. This only awaits new windows and
  ///   will not return a window that already exists at the time of calling.
  /// </summary>
  /// <param name="title"> The title of the window to wait for. </param>
  /// <param name="processID">
  ///   The process ID of the window to wait for. If <c> 0 </c>, the window is allowed to be from
  ///   any process.
  /// </param>
  /// <param name="timeout">
  ///   The maximum time to wait for the window to be created. If <c> 0 </c>, the method waits
  ///   indefinitely.
  /// </param>
  /// <returns> The window that was created, or <see langword="null" /> if the timeout elapsed. </returns>
  public static async Task<Window?> AwaitWindow(string title, int processID = 0, int timeout = 0) {
    return await AwaitWindow(hwnd => hwnd.GetWindowText() == title, processID, timeout);
  }


  private static async Task<Window?> AwaitWindow(
    WindowCriteria criteria,
    int processID = 0,
    int timeout = 0
  ) {
    var hwnd = await WinEventAwaiter.AwaitEvent(
                 [WinEvent.EVENT_OBJECT_CREATE],
                 criteria,
                 timeout == 0 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeout)
               );
    if (hwnd is null) return null;
    return new Window(
      new Win32Window {
        Hwnd        = hwnd.Value,
        ClassName   = hwnd.Value.GetClassName(),
        Title       = hwnd.Value.GetWindowText(),
        ProcessID   = processID == 0 ? hwnd.Value.GetProcessID() : (uint)processID,
        ProcessName = hwnd.Value.GetProcessName()
      }
    );
  }
}
