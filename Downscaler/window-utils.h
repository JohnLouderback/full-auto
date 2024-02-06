#pragma once

#include <string>
#include <dwmapi.h>

#include "AppState.h"

/// Utility Functions ///

/**
 * @brief Determines in the given Window has the given process name and, optionally, class name.
 * @param window The window to check.
 * @param processName The process name to check for.
 * @param className Optionally, the class name to check for.
 * @returns `true` if the window has the given process name and, optionally, class name - otherwise `false`.
 */
inline bool WindowHasProcessName(
  const Window& window,
  const std::wstring& processName,
  const std::optional<std::wstring>& className
) {
  auto windowProcessName = window.ProcessName();

  // Chops off the path from the process name so "C:\Windows\explorer.exe" becomes "explorer.exe".
  auto windowProcessNameEnd = windowProcessName.substr(windowProcessName.find_last_of(L'\\') + 1);

  // If the process name is the same as the provided process name, return the window. The comparison will be case
  // insensitive.
  if (InsensitiveComparison(windowProcessNameEnd, processName)) {
    // and a class name was provided...
    if (className.has_value()) {
      // ...and the class name matches the window's class name, return the window that matched the process and class
      // name.
      if (window.ClassName() == className.value()) {
        return true;
      }
      // Otherwise, continue to the next window. (This code is redundant, but it's here for clarity.)
      return false;
    }
    // Otherwise, if the process matches and a class name was not provided, return the window that matched the process.
    return true;
  }

  return false;
}

/**
 * @brief Determines in the given Window has the given class name.
 * @param window The window to check.
 * @param className The class name to check for.
 * @returns `true` if the window has the given class name - otherwise `false`.
 */
inline bool WindowHasClassName(const Window& window, const std::wstring& className) {
  return window.ClassName() == className;
}

/**
 * @brief Determines in the given Window has the given title and, optionally, class name.
 * @param window The window to check.
 * @param title The title to check for.
 * @param className Optionally, the class name to check for.
 * @returns `true` if the window has the given title and, optionally, class name - otherwise `false`.
 */
inline bool WindowHasTitle(
  const Window& window,
  const std::wstring& title,
  const std::optional<std::wstring>& className
) {
  // If the title matches the window's title...
  if (window.Title() == title) {
    // and a class name was provided...
    if (className.has_value()) {
      // ...and the class name matches the window's class name, return the window that matched the title and class
      // name.
      if (window.ClassName() == className.value()) {
        return true;
      }
      // Otherwise, continue to the next window. (This code is redundant, but it's here for clarity.)
      return false;
    }
    // Otherwise, if the title matches and a class name was not provided, return the window that matched the title.
    return true;
  }

  return false;
}

/**
 * @brief Recursively searches the children of the given window for a window with the given process name and, optionally,
 *        class name.
 * @param window The window to search.
 * @param processName The process name to search for.
 * @param className Optionally, the class name to search for.
 * @returns The window with the given process name and, optionally, class name or `nullopt` if no window is found.
 */
inline std::optional<Window> RecurseChildrenForProcessName(
  const Window& window,
  const std::wstring& processName,
  const std::optional<std::wstring>& className
) {
  // If the window does not have the process name at all, we already know no child window will have it either.
  if (!WindowHasProcessName(window, processName, std::nullopt)) {
    return std::nullopt;
  }

  // Enumerate the children of the window.
  for (const auto& child : window.Children()) {
    // If the child has the process name and class name, return the child.
    if (WindowHasProcessName(child, processName, className)) {
      return child;
    }

    // Recurse into the children of the child.
    const auto foundChild = RecurseChildrenForProcessName(child, processName, className);
    if (foundChild.has_value()) {
      return foundChild;
    }
  }

  // If no child window has the process name and class name, return `nullopt`.
  return std::nullopt;
}

/**
 * @brief Retrieves the window with the given process name.
 * @param processName The process name of the window to retrieve.
 * @param className Optionally, the class name of the window to retrieve. This filters the search to only windows with
 *        the given class name.
 * @returns The window with the given process name or `nullopt` if no window is found.
 */
inline std::optional<Window> GetWindowForProcessName(
  const std::wstring& processName,
  const std::optional<std::wstring>& className
) {
  auto allWindows = AppState::GetInstance().GetAllWindows();

  // Search for the window with the given process name. The provided process name will be only the executable name. So
  // we'll need to check the end of the process name, since the process name will be the full path to the executable in
  // the window object's process name property.
  for (const auto& window : allWindows) {
    if (WindowHasProcessName(window, processName, className)) {
      return window;
    }
  }

  // If no window was found so far, and a class name was provided, search for the window with the given class name in
  // each main window's children.
  if (className.has_value()) {
    for (const auto& window : allWindows) {
      // Check if the window's process name matches the provided process name. If it doesn't, don't bother checking the
      // children for the class name.
      if (!WindowHasProcessName(window, processName, std::nullopt)) {
        continue;
      }
      // Enumerate the children of the window.
      for (const auto& child : window.Children()) {
        // If the child has the process name and class name, return the child.
        if (WindowHasProcessName(child, processName, className)) {
          return child;
        }

        // Recurse into the children of the child.
        const auto foundChild = RecurseChildrenForProcessName(child, processName, className);
        if (foundChild.has_value()) {
          return foundChild;
        }
      }
    }
  }

  // If no window is found, return nullopt.
  return std::nullopt;
}

inline std::optional<Window> RecurseChildrenForWindowTitle(
  const Window& window,
  const std::wstring& title,
  const std::optional<std::wstring>& className,
  bool parentHasTitleButNotClassName = false
) {
  // Enumerate the children of the window.
  for (const auto& child : window.Children()) {
    // If the child has the title and class name, return the child.
    if (WindowHasTitle(child, title, className)) {
      return child;
    }

    // If a parent had the title but not the class name, and the child has the class name, return the child.
    if (parentHasTitleButNotClassName && WindowHasClassName(child, className.value())) {
      return child;
    }

    // If this window has the title but not the class name, set the parentHasTitleButNotClassName flag to true.
    // We'll accept any child with the class name, as long as any ancestor window had the title.
    if (WindowHasTitle(child, title, std::nullopt)) {
      parentHasTitleButNotClassName = true;
    }

    // Recurse into the children of the child.
    const auto foundChild = RecurseChildrenForWindowTitle(child, title, className, parentHasTitleButNotClassName);
    if (foundChild.has_value()) {
      return foundChild;
    }
  }

  return std::nullopt;
}

/**
 * @brief Retrieves the window with the given title.
 * @param title The title of the window to retrieve.
 * @param className Optionally, the class name of the window to retrieve. This filters the search to only windows with
 *        the given class name.
 * @returns The window with the given title or `nullopt` if no window is found.
 */
inline std::optional<Window> GetWindowForWindowTitle(
  const std::wstring& title,
  const std::optional<std::wstring>& className
) {
  const auto allWindows = AppState::GetInstance().GetAllWindows();

  // Search for the window with the given title.
  for (const auto& window : allWindows) {
    if (WindowHasTitle(window, title, className)) {
      return window;
    }
  }

  // If no window was found so far, and a class name was provided, search for the window with the given class name in
  // each main window's children.
  if (className.has_value()) {
    for (const auto& window : allWindows) {
      // Recursively loop over the window's children to find the window with the given title and class name.
      // We'll accept any window with the class name that either also has the title or has any ancestor window with the
      // title.
      const auto foundChild = RecurseChildrenForWindowTitle(window, title, className);
      if (foundChild.has_value()) {
        return foundChild;
      }
    }
  }

  // If no window is found, return `nullopt`.
  return std::nullopt;
}

enum class WindowSearchType {
  Title,
  ProcessName
};

/**
 * @brief Determines if the given string is a window title or a process name.
 * @param str The string to check.
 * @returns The type of the string.
 */
inline WindowSearchType IsStringTitleOrProcessName(const std::wstring& str) {
  // If the string is a process name, it will end with ".exe". Otherwise, it is presumed to be a window title.
  if (str.find(L".exe") != std::wstring::npos) {
    return WindowSearchType::ProcessName;
  }

  // If the string is not a process name, then it is presumed to be a window title.
  return WindowSearchType::Title;
}
