using GameLauncher.Script.Objects;
using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;
using static GameLauncher.Script.Utils.JSTypeConverter;

namespace GameLauncher.Script;

public partial class Tasks {
  /// <summary>
  ///   Searches for any windows that match the specified criteria. If no windows are found, it
  ///   waits for a new window to be created that matches the criteria. If a window is found, it is
  ///   returned immediately. If no window is found within the specified timeout, an empty array is
  ///   returned.
  /// </summary>
  /// <param name="searchCriteria"> The criteria to use to search for the window. </param>
  /// <param name="timeout">
  ///   The maximum time to wait for the window to be created, if none were found initially. If
  ///   <c> 0 </c>, the method waits indefinitely.
  /// </param>
  /// <returns>
  ///   An array of windows that match the criteria if any exist at the time of calling. If no
  ///   windows are found, it will return an array containing the first newly created window that
  ///   matches the criteria. If no window is found within the specified timeout, an empty array is
  ///   returned.
  /// </returns>
  public static async Task<JSArray<Window>> FindOrAwaitWindow(
    WindowSearchCriteria searchCriteria,
    int timeout = 0
  ) {
    // Try to find any matching windows that already exist.
    var existingWindows = FindWindows(searchCriteria);

    // If any matching windows were found, return them.
    if (existingWindows.Any()) {
      return JSArray<Window>.FromIEnumerable(existingWindows);
    }

    var                 awaitedWindow = await AwaitWindow(searchCriteria, timeout);
    IEnumerable<Window> windowList    = awaitedWindow is not null ? [awaitedWindow] : [];

    return JSArray<Window>.FromIEnumerable(windowList);
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
  ///   The maximum time to wait for the window to be created, if none were found initially. If
  ///   <c> 0 </c>, the method waits indefinitely.
  /// </param>
  /// <returns>
  ///   An array of windows that match the criteria if any exist at the time of calling. If no
  ///   windows are found, it will return an array containing the first newly created window that
  ///   matches the criteria. If no window is found within the specified timeout, an empty array is
  ///   returned.
  /// </returns>
  public static async Task<JSArray<Window>> FindOrAwaitWindow(
    WindowCriteriaCallback searchCriteria,
    int timeout = 0
  ) {
    ArgumentNullException.ThrowIfNull(searchCriteria);

    // Try to find any matching windows that already exist.
    var existingWindows = FindWindows(searchCriteria);

    // If any matching windows were found, return them.
    if (existingWindows.Any()) {
      return JSArray<Window>.FromIEnumerable(existingWindows);
    }

    var                 awaitedWindow = await AwaitWindow(searchCriteria, timeout);
    IEnumerable<Window> windowList    = awaitedWindow is not null ? [awaitedWindow] : [];

    return JSArray<Window>.FromIEnumerable(windowList);
  }


  [HideFromTypeScript]
  public static async Task<JSArray<Window>> FindOrAwaitWindow(
    ScriptObject searchCriteria,
    int timeout = 0
  ) {
    ArgumentNullException.ThrowIfNull(searchCriteria);

    if (IsFunction(searchCriteria)) {
      return await FindOrAwaitWindow(
               window => searchCriteria.Invoke(false, window).IsValueTruthy(),
               timeout
             );
    }

    if (IsPlainObject(searchCriteria)) {
      return await FindOrAwaitWindow((WindowSearchCriteria)searchCriteria, timeout);
    }

    throw new ArgumentException(
      $"Invalid search criteria. Expected a function or plain object, but got \"{
        GetJSType(searchCriteria)
      }\"."
    );
  }
}
