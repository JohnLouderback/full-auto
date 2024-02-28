#pragma once
namespace DownscalerV3::Cpp::WinRT {
  winrt::Windows::Graphics::Capture::GraphicsCaptureItem CreateCaptureItemForWindow(HWND hwnd);
}
