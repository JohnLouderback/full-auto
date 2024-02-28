#pragma once
namespace DownscalerV3::Cpp::WinRT {
  /**
 * @brief Creates a capture item for a window. A capture item is used to capture the contents of a window.
 * @param hwnd The window to capture.
 * @return The pointer to the IGraphicsCaptureItem interface for the window, or nullptr on failure.
 */
  void* CreateCaptureItemForWindow(HWND hwnd);
}
