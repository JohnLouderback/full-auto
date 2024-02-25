using DownscalerV3.Core.Models;

namespace DownscalerV3.Core.Utils;

/// <summary>
///   General higher-level utilities for interacting with windows.
/// </summary>
public class WindowUtils {
  /// <summary>
  ///   Gets the window for the given process name and, optionally, class name. If a class name is
  ///   provided, then the window must have the process name and the class name. If no
  ///   <paramref name="className" /> is provided, then the window only needs to have the process
  ///   name. <see langword="null" /> is returned if no window has the given process name and,
  ///   optionally, class name.
  /// </summary>
  /// <param name="processName"> The process name to get the window for. </param>
  /// <param name="className"> Optionally, the class name to get the window for. </param>
  /// <returns> The window for the given process name and, optionally, class name. </returns>
  public static Win32Window? GetWindowForProcessName(string processName, string? className = null) {
    // Gets all top-level windows.
    var windows = NativeUtils.EnumerateWindows();

    // Because it's most likely that a top-level window will have the process name, we check the
    // top-level windows first.
    foreach (var window in windows) {
      if (WindowHasProcessName(window, processName, className)) {
        return window;
      }
    }

    // If no top-level window had the process name, and a class was provided, we may have not found
    // a match because a child window has the class name instead. We'll now deeply check the
    // children for the process name and class name.
    if (className != null) {
      foreach (var window in windows) {
        var result = RecurseChildrenForProcessName(window, processName, className);
        if (result != null) {
          return result;
        }
      }
    }

    // If no window or its children had the process name and, optionally, class name, return null.
    return null;
  }


  /// <summary>
  ///   Gets the window for the given title and, optionally, class name. If a class name is
  ///   provided, then the window must have the title and the class name or it must have a parent
  ///   window with the title and a child window with the class name.
  ///   If no <paramref name="className" /> is provided, then the window only needs to have the
  ///   title. <see langword="null" /> is returned if no window has the given title and, optionally,
  ///   class name.
  /// </summary>
  /// <param name="title"> </param>
  /// <param name="className"> </param>
  /// <returns> </returns>
  public static Win32Window? GetWindowForWindowTitle(string title, string? className = null) {
    // Gets all top-level windows.
    var windows = NativeUtils.EnumerateWindows();

    // Because it's most likely that a top-level window will have the title, we check the
    // top-level windows first.
    foreach (var window in windows) {
      if (WindowHasTitle(window, title, className)) {
        return window;
      }
    }

    // If no top-level window had the title and class nmae, and a class was provided, we may have
    // not found a match because a child window has the class name instead. We'll now deeply check
    // the children for the title and class name. If any child has the class name and either has the
    // title or has a parent window with the title, we'll return it.
    if (className != null) {
      foreach (var window in windows) {
        var result = RecurseChildrenForWindowTitle(window, title, className);
        if (result != null) {
          return result;
        }
      }
    }

    // If no window or its children had the title and, optionally, class name, return null.
    return null;
  }


  /// <summary>
  ///   Determines if the given string is a title or a process name. If the string ends with ".exe",
  ///   then it's considered a process name. Otherwise, it's considered a title. If the string can
  ///   be considered neither, then <see langword="null" /> is returned.
  /// </summary>
  /// <param name="input"> The string to check. </param>
  /// <returns> The type of search to perform when looking for a window. </returns>
  public static WindowSearchType? IsStringTitleOrProcessName(string input) {
    // If the input ends with ".exe", then we consider it a process name.
    if (input.EndsWith(".exe")) {
      return WindowSearchType.ProcessName;
    }

    // Otherwise, we consider it a title.
    return WindowSearchType.Title;
  }


  /// <summary>
  ///   Recursively checks the children of the given window for the given process name and,
  ///   optionally,
  /// </summary>
  /// <param name="window"> The window to check and whose children to check. </param>
  /// <param name="processName"> The process name to check for. </param>
  /// <param name="className"> Optionally, the class name to check for. </param>
  /// <returns>
  ///   The first child of the given window that has the given process name and, optionally,
  ///   class name.
  /// </returns>
  public static Win32Window? RecurseChildrenForProcessName(
    Win32Window window,
    string processName,
    string? className = null
  ) {
    // First check if this window matches the process name and, optionally, class name. If it does,
    // return it.
    if (WindowHasProcessName(window, processName, className)) {
      return window;
    }

    // If this window does not have the process name, then no child of this window will have the
    // process name, so we return null. We don't check for the class name because while _this_
    // window may not have the class name, we can check the children for the class name.
    if (!WindowHasProcessName(window, processName)) {
      return null;
    }

    // Deeply check the children for the process name and class name. If it has the process name,
    // but not the class name, continue deeply checking.
    var children = window.Children();
    foreach (var child in children) {
      var result = RecurseChildrenForProcessName(child, processName, className);
      // If the child has the process name and class name, return it without checking the other
      // children.
      if (result != null) {
        return result;
      }
    }

    // If no child has the process name and class name, return null.
    return null;
  }


  /// <summary>
  ///   Recursively checks the children of the given window for the given title and, optionally,
  ///   class name. If a parent window has the title, and a class name was passed, and a child
  ///   had the class name, then we return the child.
  /// </summary>
  /// <param name="window"> The window to check and whose children to check. </param>
  /// <param name="title"> The title to check for. </param>
  /// <param name="className"> Optionally, the class name to check for. </param>
  /// <param name="parentHasTitleButNotClassName">
  ///   Whether or not a parent of this window had the title but not the class name.
  /// </param>
  /// <returns>
  ///   The first child of the given window that has the given title and, optionally, class name. Or
  ///   the first child that had the class, if provided, that has an ancestor with the title. Or
  ///   <see langword="null" /> if no child has the given title and, optionally, class name.
  /// </returns>
  public static Win32Window? RecurseChildrenForWindowTitle(
    Win32Window window,
    string title,
    string? className = null,
    bool parentHasTitleButNotClassName = false
  ) {
    // First check if this window matches the title and, optionally, class name. If it does,
    // return it.
    if (WindowHasTitle(window, title, className)) {
      return window;
    }

    // If a parent of this window had the title, and _this_ child window has the class, then we
    // return it. We don't check for the title because while _this_ window may not have the title,
    // we allow matching a child whose parent had the title as long as this child has the class name.
    if (parentHasTitleButNotClassName && WindowHasClassName(window, className)) {
      return window;
    }

    // Check if this window has the title but not the class name. If it does, then we can't return
    // this window, but we can check the children for the class name. We allow matching children
    // who have the class name when any parent has the title. We don't pass the class name here
    // because we know the window already doesn't have it from the previous check.
    if (WindowHasTitle(window, title)) {
      parentHasTitleButNotClassName = true;
    }

    // Deeply check the children for the title and class name. If it has the title,
    // but not the class name, continue deeply checking.
    var children = window.Children();
    foreach (var child in children) {
      var result = RecurseChildrenForWindowTitle(
        child,
        title,
        className,
        parentHasTitleButNotClassName
      );

      // If the child has the title and class name, return it without checking the other children.
      if (result != null) {
        return result;
      }
    }

    // If no child has the title and class name, or no parent had the title with a child having the
    // class name, return null.
    return null;
  }


  /// <summary>
  ///   Determines if the given window has the given class name. The check is case-insensitive.
  /// </summary>
  /// <param name="window"> The window to check. </param>
  /// <param name="className"> The class name to check for. </param>
  /// <returns>
  ///   <see langword="true" /> if the window has the given class name; <see langword="false" />
  ///   otherwise.
  /// </returns>
  public static bool WindowHasClassName(Win32Window window, string className) {
    return string.Equals(window.ClassName, className, StringComparison.OrdinalIgnoreCase);
  }


  /// <summary>
  ///   Determines if the given window has the given process name and, optionally, class name. The
  ///   check for the process name is case-insensitive, and the check for the class name is
  ///   case-insensitive.
  /// </summary>
  /// <param name="window"> The window to check. </param>
  /// <param name="processName"> The process name to check for. </param>
  /// <param name="className"> Optionally, the class name to check for. </param>
  /// <returns>
  ///   <see langword="true" /> if the window has the given process name and, optionally, class name;
  ///   <see langword="false" /> otherwise.
  /// </returns>
  public static bool WindowHasProcessName(
    Win32Window window,
    string processName,
    string? className = null
  ) {
    var windowProcessName = window.ProcessName;

    // Get the process name without the path. For example, "C:\Windows\explorer.exe" becomes
    // "explorer.exe".
    var windowProcessNameEnd = windowProcessName.Substring(windowProcessName.LastIndexOf('\\') + 1);

    // If the process name ends with the given process name, then the window has the given process
    // name.
    if (string.Equals(windowProcessNameEnd, processName, StringComparison.OrdinalIgnoreCase)) {
      // If the class name is null, then we don't care about the class name, so we return true.
      if (className == null) {
        return true;
      }

      // If the class name is not null, then we check if the window's class name is equal to the
      // given class name. If it is, then the window has the given process name and class name.
      // Otherwise, it doesn't and we return false.
      return string.Equals(window.ClassName, className, StringComparison.OrdinalIgnoreCase);
    }

    // Finally, if the process name doesn't end with the given process name, then the window doesn't
    // have the given process name, so we return false.
    return false;
  }


  /// <summary>
  ///   Determines if the given window has the given title and, optionally, class name. The check for
  ///   the title is case-sensitive, and the check for the class name is case-insensitive.
  /// </summary>
  /// <param name="window"> The window to check. </param>
  /// <param name="title"> The title to check for. </param>
  /// <param name="className"> Optionally, the class name to check for. </param>
  /// <returns>
  ///   <see langword="true" /> if the window has the given title and, optionally, class name;
  ///   <see langword="false" /> otherwise.
  /// </returns>
  public static bool WindowHasTitle(Win32Window window, string title, string? className = null) {
    // If the window's title is equal to the given title, then the window has the given title.
    if (string.Equals(window.Title, title, StringComparison.Ordinal)) {
      // If the class name is null, then we don't care about the class name, so we return true.
      if (className == null) {
        return true;
      }

      // Otherwise, we check if the window's class name is equal to the given class name. If it is,
      // then the window has the given title and class name. Otherwise, it doesn't and we return
      // false.
      return string.Equals(window.ClassName, className, StringComparison.OrdinalIgnoreCase);
    }

    // Finally, if the window's title is not equal to the given title, then the window doesn't have
    // the given title at all, so we return false.
    return false;
  }
}
