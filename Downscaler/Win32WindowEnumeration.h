#pragma once
#include <dwmapi.h>
#include <string>
#include <vector>
#include <array>

struct Window {
  public:
    Window(nullptr_t) {}

    Window(HWND hwnd, const std::wstring& title, std::wstring& className, std::wstring& processName) {
      this->hwnd = hwnd;
      this->title = title;
      this->className = className;
      this->processName = processName;
    }

    HWND Hwnd() const noexcept { return this->hwnd; }
    std::wstring Title() const noexcept { return this->title; }
    std::wstring ClassName() const noexcept { return this->className; }
    std::wstring ProcessName() const noexcept { return this->processName; }

    /**
     * @brief Retrieves the rectangle that defines the window's size and position.
     * @param rect The rectangle that defines the window's size and position.
     * @returns `TRUE` if the function succeeds, otherwise `FALSE`.
     */
    bool GetRect(RECT* rect) const noexcept {
      return GetWindowRect(this->hwnd, rect);
    }

    /**
     * @brief Retrieves the client area of the window.
     * @param rect The client area of the window.
     * @returns `TRUE` if the function succeeds, otherwise `FALSE`.
     */
    bool GetClientRect(RECT* rect) const noexcept {
      return ::GetClientRect(this->hwnd, rect);
    }

    /**
     * @brief Retrieves the client area of the window relative to the window. This is useful if we need to crop the
     *        captured content to the client area of the window.
     */
    bool GetClientRectRelativeToWindow(RECT* rect) const noexcept {
      RECT clientRect;
      if (!GetClientRect(&clientRect)) {
        return false;
      }

      POINT point = {0, 0};
      if (!ClientToScreen(this->hwnd, &point)) {
        return false;
      }

      rect->left = point.x;
      rect->top = point.y;
      rect->right = point.x + clientRect.right - clientRect.left;
      rect->bottom = point.y + clientRect.bottom - clientRect.top;

      return true;
    }

    /**
     * @brief Retrieves the width of the window.
     * @returns The width of the window.
     */
    int Width() const noexcept {
      RECT rect;
      GetWindowRect(this->hwnd, &rect);
      return rect.right - rect.left;
    }

    /**
     * @brief Retrieves the width of the client area of the window.
     * @returns The width of the client area of the window.
     */
    int ClientWidth() const noexcept {
      RECT rect;
      GetClientRect(&rect);
      return rect.right - rect.left;
    }

    /**
     * @brief Retrieves the height of the window.
     * @returns The height of the window.
     */
    int Height() const noexcept {
      RECT rect;
      GetWindowRect(this->hwnd, &rect);
      return rect.bottom - rect.top;
    }

    /**
     * @brief Retrieves the height of the client area of the window.
     * @returns The height of the client area of the window.
     */
    int ClientHeight() const noexcept {
      RECT rect;
      GetClientRect(&rect);
      return rect.bottom - rect.top;
    }

    /**
     * @brief Retrieves the x-coordinate of the window based on the top-left corner of the window.
     * @returns The x-coordinate of the window.
     */
    int X() const noexcept {
      RECT rect;
      GetWindowRect(this->hwnd, &rect);
      return rect.left;
    }

    /**
     * @brief Retrieves the x-coordinate of the client area of the window based on the top-left corner of the client
     *        area of the window.
     * @returns The x-coordinate of the client area of the window.
     */
    int ClientX() const noexcept {
      RECT rect;
      GetClientRect(&rect);
      return rect.left;
    }

    /**
     * @brief Retrieves the y-coordinate of the window based on the top-left corner of the window.
     * @returns The y-coordinate of the window.
     */
    int Y() const noexcept {
      RECT rect;
      GetWindowRect(this->hwnd, &rect);
      return rect.top;
    }

    /**
     * @brief Retrieves the y-coordinate of the client area of the window based on the top-left corner of the client
     *        area of the window.
     * @returns The y-coordinate of the client area of the window.
     */
    int ClientY() const noexcept {
      RECT rect;
      GetClientRect(&rect);
      return rect.top;
    }

  private:
    /** The handle to the window. A memory location that represents the window. */
    HWND hwnd;

    /** The title of the window. The title is the text that appears in the title bar of the window. */
    std::wstring title;

    /** The class name of the window. The class name is the name of the window's class. Like "Chrome_WidgetWin_1". */
    std::wstring className;

    /** The process name of the window. The process name is the name of the process that created the window. Like "explorer.exe". */
    std::wstring processName;
};

inline std::wstring GetClassName(HWND hwnd) {
  std::array<WCHAR, 1024> className;

  ::GetClassName(hwnd, className.data(), className.size());

  std::wstring title(className.data());
  return title;
}

inline std::wstring GetWindowText(HWND hwnd) {
  std::array<WCHAR, 1024> windowText;

  ::GetWindowText(hwnd, windowText.data(), windowText.size());

  std::wstring title(windowText.data());
  return title;
}

inline bool IsAltTabWindow(const Window& window) {
  auto hwnd = window.Hwnd();
  auto shellWindow = GetShellWindow();

  auto title = window.Title();
  auto className = window.ClassName();

  if (hwnd == shellWindow) {
    return false;
  }

  if (title.length() == 0) {
    return false;
  }

  if (!IsWindowVisible(hwnd)) {
    return false;
  }

  if (GetAncestor(hwnd, GA_ROOT) != hwnd) {
    return false;
  }

  LONG style = GetWindowLong(hwnd, GWL_STYLE);
  if (!((style & WS_DISABLED) != WS_DISABLED)) {
    return false;
  }

  DWORD cloaked = FALSE;
  HRESULT hrTemp = DwmGetWindowAttribute(hwnd, DWMWA_CLOAKED, &cloaked, sizeof(cloaked));
  if (SUCCEEDED(hrTemp) &&
    cloaked == DWM_CLOAKED_SHELL) {
    return false;
  }

  return true;
}

/**
 * @brief Retrieves the name of the process that created the window.
 * @param hwnd The handle to the window.
 * @returns The name of the process that created the window.
 */
inline std::wstring GetProcessName(HWND hwnd) {
  // The numeric identifier of the process that created the window.
  DWORD processId;

  // Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window.
  GetWindowThreadProcessId(hwnd, &processId);

  // Open the process given the process identifier to get the process handle.
  const auto process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, processId);

  // If the process handle is null, return an empty string since the process name cannot be retrieved.
  if (process == nullptr) {
    return L"";
  }

  // The full path of the executable file of the process.
  std::array<WCHAR, 1024> processName;
  DWORD size = processName.size();

  // Retrieves the full name of the executable image for the specified process.
  QueryFullProcessImageNameW(process, 0, processName.data(), &size);

  // Close the process handle since it is no longer needed.
  CloseHandle(process);

  // Convert the process name to a wide string and return it.
  std::wstring name(processName.data());
  return name;
}

inline Window WindowFromHWND(HWND hwnd) {
  auto className = GetClassName(hwnd);
  const auto title = GetWindowText(hwnd);

  auto procName = GetProcessName(hwnd);
  auto window = Window(hwnd, title, className, procName);

  return window;
}

inline BOOL CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam) {
  const auto window = WindowFromHWND(hwnd);

  if (!IsAltTabWindow(window)) {
    return TRUE;
  }

  auto& windows = *reinterpret_cast<std::vector<Window>*>(lParam);
  windows.push_back(window);

  return TRUE;
}

inline const std::vector<Window> EnumerateWindows() {
  std::vector<Window> windows;
  EnumWindows(EnumWindowsProc, reinterpret_cast<LPARAM>(&windows));

  return windows;
}
