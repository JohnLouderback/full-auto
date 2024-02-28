// DownscalerV3.Cpp.WinRT.cpp : Defines the functions for the static library.
//

#include "pch.h"

#include <intsafe.h>
#include "framework.h"
#include "DownscalerV3.Cpp.WinRT.h"
#include <winrt/Windows.Graphics.Capture.h>
#include <windows.graphics.capture.h>
#include <windows.graphics.capture.interop.h>
#include <winrt/Windows.Graphics.Capture.h>

using namespace winrt::Windows::Graphics::Capture;

/**
 * @brief Creates a capture item for a window. A capture item is used to capture the contents of a window.
 * @param hwnd The window to capture.
 * @return The capture item for the window. 
 */
auto CreateCaptureItemForWindow(HWND hwnd) {
  const auto activationFactory = winrt::get_activation_factory<
    GraphicsCaptureItem
  >();

  const auto interopFactory = activationFactory.as<IGraphicsCaptureItemInterop>();

  GraphicsCaptureItem item = {nullptr};

  const auto itemCreationResult = interopFactory->CreateForWindow(
    hwnd,
    winrt::guid_of<ABI::Windows::Graphics::Capture::IGraphicsCaptureItem>(),
    winrt::put_abi(item)
  );

  if (FAILED(itemCreationResult)) {
    throw winrt::hresult_error(itemCreationResult);
  }

  return item;
}
