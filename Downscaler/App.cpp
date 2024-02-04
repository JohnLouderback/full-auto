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

#include "AppState.h"
#include "SimpleCapture.h"

using namespace winrt;
using namespace Windows::System;
using namespace Windows::Foundation;
using namespace Windows::UI;
using namespace Windows::UI::Composition;
using namespace Windows::Graphics::Capture;

void App::Initialize(
  const ContainerVisual& root
) {
  const auto appState = AppState::GetInstance();
  auto queue = DispatcherQueue::GetForCurrentThread();

  this->compositor = root.Compositor();
  this->root = this->compositor.CreateContainerVisual();
  this->content = this->compositor.CreateSpriteVisual();
  this->brush = this->compositor.CreateSurfaceBrush();

  this->root.RelativeSizeAdjustment({1, 1});
  root.Children().InsertAtTop(this->root);

  this->content.AnchorPoint({0.5f, 0.5f});
  this->content.RelativeOffsetAdjustment({0.5f, 0.5f, 0});
  this->content.RelativeSizeAdjustment({1, 1});
  this->content.Size({0, 0});
  this->content.Brush(this->brush);
  this->brush.HorizontalAlignmentRatio(0.5f);
  this->brush.VerticalAlignmentRatio(0.5f);

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

  const auto d3dDevice = CreateD3DDevice();
  const auto dxgiDevice = d3dDevice.as<IDXGIDevice>();
  this->device = CreateDirect3DDevice(dxgiDevice.get());
}

void App::StartCapture(HWND hwnd) {
  if (this->capture) {
    this->capture->Close();
    this->capture = nullptr;
  }

  const auto item = CreateCaptureItemForWindow(hwnd);

  this->capture = std::make_unique<SimpleCapture>(this->device, item);

  const auto surface = this->capture->CreateSurface(this->compositor);
  this->brush.Surface(surface);

  this->capture->StartCapture();
}

void App::StartCapture() {
  // Get the window to capture from the application state.
  const auto appState = AppState::GetInstance();
  const auto windowToScale = appState.GetWindowToScale();
  const auto hwnd = windowToScale.Hwnd();
  this->StartCapture(hwnd);
}
