#pragma once

#include <string>
#include <dwmapi.h>

#include "AppState.h"

/// Utility Functions ///

/**
 * @brief Retrieves the window with the given title.
 * @param title The title of the window to retrieve.
 * @returns The window with the given title or `nullopt` if no window is found.
 */
inline std::optional<Window> GetWindowForWindowTitle(const std::wstring& title) {
  // Search for the window with the given title.
  for (const auto allWindows = AppState::GetInstance().GetAllWindows(); const auto& window : allWindows) {
    if (window.Title() == title) {
      return window;
    }
  }

  // If no window is found, return nullopt.
  return std::nullopt;
}

/**
 * @brief Retrieves the window with the given process name.
 * @param processName The process name of the window to retrieve.
 * @returns The window with the given process name or `nullopt` if no window is found.
 */
inline std::optional<Window> GetWindowForProcessName(const std::wstring& processName) {
  auto allWindows = AppState::GetInstance().GetAllWindows();

  // Search for the window with the given process name. The provided process name will be only the executable name. So
  // we'll need to check the end of the process name, since the process name will be the full path to the executable in
  // the window object's process name property.
  for (const auto& window : allWindows) {
    auto windowProcessName = window.ProcessName();

    // Chops off the path from the process name so "C:\Windows\explorer.exe" becomes "explorer.exe".
    auto windowProcessNameEnd = windowProcessName.substr(windowProcessName.find_last_of(L'\\') + 1);

    // If the process name is the same as the provided process name, return the window. The comparison will be case
    // insensitive.
    if (InsensitiveComparison(windowProcessNameEnd, processName)) {
      return window;
    }
  }

  // If no window is found, return nullopt.
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
  // If the string is a process name, it will not contain any spaces and will end with ".exe". If either of these
  // conditions are not met, then the string is presumed to be a window title.
  if (str.find(L' ') == std::wstring::npos && str.find(L".exe") != std::wstring::npos) {
    return WindowSearchType::ProcessName;
  }

  // If the string is not a process name, then it is presumed to be a window title.
  return WindowSearchType::Title;
}
