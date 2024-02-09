#pragma once
#include <dwmapi.h>
#include <string>
#include <vector>
#include <array>

// Declare the Window struct here so that it can be used in the EnumerateChildWindows function.
struct Win32Window;

// Declare the `EnumerateChildWindows` function here so that it can be used in the `Window` struct.
inline const std::vector<Win32Window> EnumerateChildWindows(HWND hwnd);

// Declare the `GetSystemBaseDpi` function here so that it can be used in the `Window` struct.
inline const int GetSystemBaseDpi();

struct Win32Window {
  public:
    Win32Window(nullptr_t) {}

    Win32Window(HWND hwnd, const std::wstring& title, std::wstring& className, std::wstring& processName) {
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
     * @brief Brings the window to the foreground and activates it.
     */
    void Focus() const noexcept {
      SetForegroundWindow(this->hwnd);
    }

    bool HasFocus() const noexcept {
      return GetFocus() == this->hwnd;
    }

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
    RECT GetClientRectRelativeToWindow() const {
      const auto hWnd = this->hwnd;
      RECT clientRect;
      ::GetClientRect(hWnd, &clientRect);

      // Convert the clientRect to screen coordinates.
      POINT topLeft = {clientRect.left, clientRect.top}; // Top-left
      ClientToScreen(hWnd, &topLeft);

      RECT extendedFrameBounds;
      HRESULT hr = DwmGetWindowAttribute(
        hWnd,
        DWMWA_EXTENDED_FRAME_BOUNDS,
        &extendedFrameBounds,
        sizeof(extendedFrameBounds)
      );

      const static int baseDPI = GetSystemBaseDpi();
      const int dpi = GetDpiForWindow(hWnd);
      const float dpiScale = static_cast<float>(dpi) / static_cast<float>(baseDPI);


      if (SUCCEEDED(hr)) {
        // Calculate the offset of the client area within the extended frame bounds.
        // Scale down the extended frame bounds by the DPI scale to get the correct client area. The extended frame
        // bounds are in physical pixels, so we need to scale them down to logical pixels.
        clientRect.left = topLeft.x - (extendedFrameBounds.left / dpiScale);
        clientRect.top = topLeft.y - (extendedFrameBounds.top / dpiScale);

        // Scale the client area to account for DPI scaling. The client side is not initially scaled by DPI, so we need
        // to do this manually.
        clientRect.right = (clientRect.left + (clientRect.right - clientRect.left)) * dpiScale;
        clientRect.bottom = (clientRect.top + (clientRect.bottom - clientRect.top)) * dpiScale;
      }
      else {
        // Handle the error case where DwmGetWindowAttribute might fail on non-DWM platforms.
        // Fallback to using GetWindowRect for non-composited desktops.
        RECT windowRect;
        GetWindowRect(hWnd, &windowRect);

        clientRect.left = topLeft.x - windowRect.left;
        clientRect.top = topLeft.y - windowRect.top;
        clientRect.right = clientRect.left + (clientRect.right - clientRect.left);
        clientRect.bottom = clientRect.top + (clientRect.bottom - clientRect.top);
      }

      return clientRect;
    }

    /**
     * @brief Retrieves the absolute position of the client area of the window. This is useful if we need get the delta
     *        between a parent window and a child window.
     */
    RECT GetAbsoluteClientRect() const {
      // Get the position of the window.
      RECT windowRect;
      GetWindowRect(this->hwnd, &windowRect);

      // Then get the position of the client area of the window relative to the window.
      auto clientRect = this->GetClientRectRelativeToWindow();

      // Add the position of the window to the position of the client area of the window to get the absolute position of
      // the client area of the window.
      clientRect.left += windowRect.left;
      clientRect.right += windowRect.left;
      clientRect.top += windowRect.top;
      clientRect.bottom += windowRect.top;

      return clientRect;
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

    std::vector<Win32Window> Children() const noexcept {
      return EnumerateChildWindows(this->hwnd);
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

inline bool IsAltTabWindow(const Win32Window& window) {
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

inline Win32Window WindowFromHWND(HWND hwnd) {
  auto className = GetClassName(hwnd);
  const auto title = GetWindowText(hwnd);

  auto procName = GetProcessName(hwnd);
  auto window = Win32Window(hwnd, title, className, procName);

  return window;
}

inline BOOL CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam) {
  const auto window = WindowFromHWND(hwnd);

  // if (!IsAltTabWindow(window)) {
  //   return TRUE;
  // }

  auto& windows = *reinterpret_cast<std::vector<Win32Window>*>(lParam);
  windows.push_back(window);

  return TRUE;
}

inline const std::vector<Win32Window> EnumerateWindows() {
  std::vector<Win32Window> windows;
  EnumWindows(EnumWindowsProc, reinterpret_cast<LPARAM>(&windows));

  return windows;
}

/**
 * @brief Enumerates the child windows of the given window.
 * @param hwnd The handle to the window.
 * @returns A vector of windows that are children of the given window.
 */
inline const std::vector<Win32Window> EnumerateChildWindows(HWND hwnd) {
  std::vector<Win32Window> windows;
  EnumChildWindows(hwnd, EnumWindowsProc, reinterpret_cast<LPARAM>(&windows));

  return windows;
}

/**
 * @brief Retrieves the base-DPI of the system.
 * @returns The DPI of the system.
 */

inline const int GetSystemBaseDpi() {
  static int baseDPI = -1;

  // If the base DPI has already been retrieved, return it.
  if (baseDPI != -1) {
    return baseDPI;
  }

  // Get the system's base DPI (usually 96 DPI on standard setups)
  // First, get the screen's device context.
  auto screen = GetDC(nullptr);
  // Then, get the DPI of the screen.
  baseDPI = GetDeviceCaps(screen, LOGPIXELSX);
  // Release the device context.
  ReleaseDC(nullptr, screen);

  return baseDPI;
}
