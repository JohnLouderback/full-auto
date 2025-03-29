using Windows.Win32.Foundation;
using Core.Models;
using Core.Utils;
using GameLauncher.Script.Objects;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript;
using static GameLauncher.Script.Utils.JSTypeConverter;
using static GameLauncher.Script.Utils.JSInteropUtils;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   A function whose purpose is to determine whether a given window matches the criteria. If the
  ///   window matches the criteria, the function should return <see langword="true" />; otherwise,
  ///   <see langword="false" />.
  /// </summary>
  /// <param name="window"> The window to check. </param>
  public delegate bool WindowCriteriaCallback(Window window);


  /// <summary>
  ///   Waits for a window to be spawned with the specified criteria. This only awaits new windows and
  ///   will not return a window that already exists at the time of calling.
  /// </summary>
  /// <param name="searchCriteria"> The criteria to use to search for the window. </param>
  /// <param name="timeout">
  ///   The maximum time to wait for the window to be created. If <c> 0 </c>, the method waits
  ///   indefinitely.
  /// </param>
  /// <returns> The window that was created, or <see langword="null" /> if the timeout elapsed. </returns>
  public static async Task<Window?> AwaitWindow(
    WindowSearchCriteria searchCriteria,
    int timeout = 0
  ) {
    return await AwaitWindow(
             (HWND hwnd) => {
               var windowMatches = true;

               // If a title was provided to match against, check if the window's title matches.
               if (searchCriteria.Title is not null) {
                 windowMatches &= hwnd.GetWindowText() == searchCriteria.Title;
               }

               // If a class name was provided to match against, check if the window's class name matches.
               if (searchCriteria.ClassName is not null) {
                 windowMatches &= hwnd.GetClassName() == searchCriteria.ClassName;
               }

               return windowMatches;
             },
             timeout
           );
  }


  /// <summary>
  ///   Waits for a window to be spawned with the specified criteria. This only awaits new windows and
  ///   will not return a window that already exists at the time of calling.
  /// </summary>
  /// <param name="searchCriteria">
  ///   A function that takes a <see cref="Window" /> and returns <see langword="true" /> if the
  ///   window matches the criteria.
  /// </param>
  /// <param name="timeout">
  ///   The maximum time to wait for the window to be created. If <c> 0 </c>, the method waits
  ///   indefinitely.
  /// </param>
  /// <returns> The window that was created, or <see langword="null" /> if the timeout elapsed. </returns>
  public static async Task<Window?> AwaitWindow(
    WindowCriteriaCallback searchCriteria,
    int timeout = 0
  ) {
    return await AwaitWindow(
             hwnd => {
               var window = new Window(
                 new Win32Window {
                   Hwnd        = hwnd,
                   ClassName   = hwnd.GetClassName(),
                   Title       = hwnd.GetWindowText(),
                   ProcessID   = hwnd.GetProcessID(),
                   ProcessName = hwnd.GetProcessName()
                 }
               );

               return searchCriteria(window);
             },
             timeout
           );
  }


  [HideFromTypeScript]
  public static async Task<Window?> AwaitWindow(
    ScriptObject searchCriteria,
    int timeout = 0
  ) {
    ArgumentNullException.ThrowIfNull(searchCriteria);

    if (IsFunction(searchCriteria)) {
      return await AwaitWindow(
               (Window window) => searchCriteria.Invoke(false, window).IsValueTruthy(),
               timeout
             );
    }

    if (IsPlainObject(searchCriteria)) {
      return await AwaitWindow((WindowSearchCriteria)searchCriteria, timeout);
    }

    throw new ArgumentException(
      $"Invalid search criteria. Expected a function or plain object, but got \"{
        GetJSType(searchCriteria)
      }\"."
    );
  }


  private static async Task<Window?> AwaitWindow(
    WindowCriteria criteria,
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
        ProcessID   = hwnd.Value.GetProcessID(),
        ProcessName = hwnd.Value.GetProcessName()
      }
    );
  }
}
