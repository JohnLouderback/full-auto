//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED �AS IS�, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

#include "pch.h"
#include "App.h"

#include <d2d1_1.h>
#include <dwrite.h>
#include <winrt/Windows.Foundation.h>
#include <winrt/Windows.Graphics.Capture.h>
#include <winrt/Microsoft.Graphics.Canvas.Text.h>
#include <winrt/Microsoft.Graphics.Canvas.h> // Win2D namespace for WinRT
#include <winrt/Microsoft.Graphics.Canvas.UI.Composition.h>

#include "AppState.h"
#include "general-utils.h"
#include "SimpleCapture.h"

using namespace winrt;
using namespace winrt::Windows::UI::Composition;
// using namespace winrt::Windows::Graphics::Canvas;
// using namespace winrt::Windows::Graphics::Canvas::UI::Composition;
// using namespace winrt::Windows::Graphics::Canvas::Text;
using namespace winrt::Windows::Graphics::Capture;
using namespace Windows::Graphics::DirectX;

void App::Initialize(
  const ContainerVisual& root
) {
  const auto appState = AppState::GetInstance();
  auto queue          = Windows::System::DispatcherQueue::GetForCurrentThread();

  this->compositor = root.Compositor();
  this->root       = this->compositor.CreateContainerVisual();
  this->content    = this->compositor.CreateSpriteVisual();
  this->brush      = this->compositor.CreateSurfaceBrush();

  this->root.RelativeSizeAdjustment({1, 1});
  root.Children().InsertAtTop(this->root);

  this->content.AnchorPoint({0.5f, 0.5f});
  this->content.RelativeOffsetAdjustment({0.5f, 0.5f, 0});
  this->content.RelativeSizeAdjustment({1, 1});
  this->content.Size({0, 0});
  this->content.Brush(this->brush);
  this->brush.HorizontalAlignmentRatio(0.5f);
  this->brush.VerticalAlignmentRatio(0.5f);
  this->brush.BitmapInterpolationMode(CompositionBitmapInterpolationMode::NearestNeighbor);
  this->brush.SnapToPixels(true);

  switch (appState.GetAspectRatio()) {
    case AspectRatio::Maintain:
      this->brush.Stretch(CompositionStretch::Uniform);
      break;
    case AspectRatio::Stretch:
      this->brush.Stretch(CompositionStretch::Fill);
      break;
  }

  // auto shadow = this->compositor.CreateDropShadow();
  // shadow.Mask(this->brush);
  // this->content.Shadow(shadow);
  this->root.Children().InsertAtTop(this->content);

  InitializeFPSCounter();

  const auto d3dDevice  = CreateD3DDevice();
  const auto dxgiDevice = d3dDevice.as<IDXGIDevice>();
  this->device          = CreateDirect3DDevice(dxgiDevice.get());
}

void App::StartCapture(HWND hwnd) {
  if (this->capture) {
    this->capture->Close();
    this->capture = nullptr;
  }

  GraphicsCaptureItem item = nullptr;

  try {
    item = CreateCaptureItemForWindow(hwnd);
    // Proceed with using 'item'
  }
  catch (const hresult_error& e) {
    // Log the error message and HRESULT
    std::ostringstream oss;
    oss << "Failed to create capture item. HRESULT: " << std::hex << e.code()
      << ", Message: " << to_string(e.message());
    FatalError(
      oss
    );
  }

  this->capture = std::make_unique<SimpleCapture>(this->device, item);

  const auto surface = this->capture->CreateSurface(this->compositor);
  this->brush.Surface(surface);

  this->capture->StartCapture();
}

void App::StartCapture() {
  // Get the window to capture from the application state.
  const auto appState      = AppState::GetInstance();
  const auto windowToScale = appState.GetWindowToScale();
  const auto hwnd          = windowToScale.Hwnd();
  this->StartCapture(hwnd);
}

void App::InitializeFPSCounter() {
  // Initialize Direct2D resources for text rendering
  // auto d2dFactory = []() -> com_ptr<ID2D1Factory1> {
  //   com_ptr<ID2D1Factory1> factory;
  //   D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, factory.put());
  //   return factory;
  // }();
  //
  // com_ptr<IDWriteFactory> writeFactory;
  // DWriteCreateFactory(
  //   DWRITE_FACTORY_TYPE_SHARED,
  //   __uuidof(IDWriteFactory),
  //   reinterpret_cast<IUnknown**>(writeFactory.put())
  // );
  //
  // // ...
  //
  // // Create a CanvasDevice from a Direct3D device, not a Direct2D device context
  // auto direct3DDevice = this->device;
  // auto canvasDevice   = CanvasDevice::CreateFromDirect3D11Device(direct3DDevice);
  //
  // auto textFormat = CanvasTextFormat(); // Use WinRT's CanvasTextFormat
  // textFormat.FontFamily(L"Arial");
  // textFormat.FontSize(32.0f);
  // textFormat.WordWrapping(CanvasWordWrapping::NoWrap);
  // textFormat.HorizontalAlignment(CanvasHorizontalAlignment::Center);
  // textFormat.VerticalAlignment(CanvasVerticalAlignment::Center);
  //
  // // ...
  //
  // auto compositor = this->compositor; // You need to get the Compositor from somewhere, e.g., the current window
  // auto textVisual = compositor.CreateSpriteVisual();
  // textVisual.Size({100.0f, 30.0f});
  // textVisual.Offset({10.0f, 10.0f, 0.0f});
  //
  // auto graphicsDevice = CanvasComposition::CreateCompositionGraphicsDevice(compositor, canvasDevice);
  // auto drawingSurface = graphicsDevice.CreateDrawingSurface(
  //   {100.0f, 30.0f},
  //   DirectXPixelFormat::B8G8R8A8UIntNormalized,
  //   DirectXAlphaMode::Premultiplied
  // );
  //
  // // Use Win2D to draw text on the drawing surface here
  // // ...
  //
  // auto surfaceBrush = compositor.CreateSurfaceBrush(drawingSurface);
  //
  // textVisual.Brush(surfaceBrush);
  //
  // // Retrieve a drawing session to draw on the surface.
  // auto drawingSession = CanvasComposition::CreateDrawingSession(drawingSurface);
  //
  // // Set the text and draw it on the surface.
  // drawingSession.DrawText(L"Hello World", 0, 0, Windows::UI::Colors::White(), textFormat);
  //
  // // Always close the drawing session after use.
  // drawingSession.Close();
  //
  // // Add the visual to the visual tree
  // this->root.Children().InsertAtTop(textVisual);
}
