// DownscalerV3.Cpp.WinRT.cpp : Defines the functions for the static library.
//

#include "pch.h"

#include <intsafe.h>
#include "framework.h"
#include "Downscaler.Cpp.WinRT.h"
#include <winrt/Windows.Graphics.Capture.h>
#include <windows.graphics.capture.h>
#include <windows.graphics.capture.interop.h>
#include <winrt/Windows.Graphics.Capture.h>

using namespace winrt::Windows::Graphics::Capture;

namespace Downscaler::Cpp::WinRT {
  /**
   * @brief Creates a capture item for a window. A capture item is used to capture the contents of a window.
   * @param hwnd The window to capture.
   * @return The pointer to the IGraphicsCaptureItem interface for the window, or nullptr on failure.
   */
  void* CreateCaptureItemForWindow(HWND hwnd) {
    // Obtain the activation factory for the GraphicsCaptureItem class.
    auto activationFactory = winrt::get_activation_factory<GraphicsCaptureItem>();

    // Query for the IGraphicsCaptureItemInterop interface.
    auto interopFactory = activationFactory.as<IGraphicsCaptureItemInterop>();

    // The pointer to the IGraphicsCaptureItem interface.
    void* captureItemInterface = nullptr;

    // Try to create the capture item for the specified window.
    HRESULT itemCreationResult = interopFactory->CreateForWindow(
      hwnd,
      winrt::guid_of<ABI::Windows::Graphics::Capture::IGraphicsCaptureItem>(),
      &captureItemInterface // Pass the address of the interface pointer
    );

    // Check for failure in creating the capture item.
    if (FAILED(itemCreationResult)) {
      // Handle the error as needed, such as logging the HRESULT value.
      // For now, we'll just return nullptr to indicate failure. 
      return nullptr;
    }

    // Return the interface pointer. The caller is responsible for managing the lifetime of this COM object.
    return captureItemInterface;
  }
}
