/**
 * Includes interop code for capturing from a HWND.
 */

#pragma once
#include <windows.graphics.capture.h>
#include <windows.graphics.capture.interop.h>
#include <winrt/Windows.Graphics.Capture.h>

/**
 * @brief Creates a capture item for a window. A capture item is used to capture the contents of a window.
 * @param hwnd The window to capture.
 * @return The capture item for the window. 
 */
inline auto CreateCaptureItemForWindow(HWND hwnd) {
  const auto activationFactory = winrt::get_activation_factory<
    winrt::Windows::Graphics::Capture::GraphicsCaptureItem
  >();

  const auto interopFactory = activationFactory.as<IGraphicsCaptureItemInterop>();

  winrt::Windows::Graphics::Capture::GraphicsCaptureItem item = {nullptr};

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
