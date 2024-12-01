#pragma once

#include <dwmapi.h>
#include <string>
#include <vector>
#include <array>

namespace Downscaler::Cpp::Core::NativeImpls {
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
}
