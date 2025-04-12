using GameLauncher.Script.Objects;
using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;
using static GameLauncher.Script.Utils.JSTypeConverter;

namespace GameLauncher.Script;

public partial class Tasks {
  [HideFromTypeScript]
  public static JSArray<Window> FindWindow(
    ScriptObject searchCriteria,
    int timeout = 0
  ) {
    ArgumentNullException.ThrowIfNull(searchCriteria);

    if (IsFunction(searchCriteria)) {
      return FindWindows(
        window => searchCriteria.Invoke(false, window).IsValueTruthy()
      );
    }

    if (IsPlainObject(searchCriteria)) {
      return FindWindows(searchCriteria);
    }

    throw new ArgumentException(
      $"Invalid search criteria. Expected a function or plain object, but got \"{
        GetJSType(searchCriteria)
      }\"."
    );
  }


  /// <summary>
  ///   Searches for any windows that match the specified criteria. If no windows are found, it
  ///   returns an empty array.
  /// </summary>
  /// <param name="searchCriteria"> The criteria to use to search for the windows. </param>
  /// <returns> The window that was created, or <see langword="null" /> if the timeout elapsed. </returns>
  public static JSArray<Window> FindWindows(
    WindowSearchCriteria searchCriteria
  ) {
    return JSArray<Window>.FromIEnumerable(
      GetAllWindows()
        .Where(
          window => {
            var windowMatches = true;

            // If a title was provided to match against, check if the window's title matches.
            if (searchCriteria.Title is not null) {
              windowMatches &= window.Title == searchCriteria.Title;
            }

            // If a class name was provided to match against, check if the window's class name matches.
            if (searchCriteria.ClassName is not null) {
              windowMatches &= window.ClassName == searchCriteria.ClassName;
            }

            return windowMatches;
          }
        )
    );
  }


  /// <summary>
  ///   Searches for any windows that match the specified criteria. If no windows are found, it
  ///   returns an empty array.
  /// </summary>
  /// <param name="searchCriteria">
  ///   A function that takes a <see cref="Window" /> and returns <see langword="true" /> if the
  ///   window matches the criteria.
  /// </param>
  /// <returns> The window that was created, or <see langword="null" /> if the timeout elapsed. </returns>
  public static JSArray<Window> FindWindows(
    WindowCriteriaCallback searchCriteria
  ) {
    ArgumentNullException.ThrowIfNull(searchCriteria);
    var windows = GetAllWindows();
    var filteredWindows =
      windows
        .Where(searchCriteria.Invoke);

    return JSArray<Window>.FromIEnumerable(filteredWindows);
  }
}
