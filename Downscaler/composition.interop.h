/**
 * This file contains helper functions for working with the Windows.UI.Composition API. The Windows.UI.Composition API is
 * used to create and manipulate visual elements in a UWP application. This file contains helper functions for working with
 * this API.
 */

#pragma once
#include <d2d1_1.h>
#include <windows.ui.composition.interop.h>
#include <winrt/Windows.UI.Composition.h>

/**
 * @brief Creates a CompositionGraphicsDevice from a Compositor and a Direct3D 11 device. A compositor is used to create
 *        visual elements in a UWP application. A Direct3D 11 device is used to interact with the GPU.
 * @param compositor The Compositor to create the CompositionGraphicsDevice from.
 * @param device The Direct3D 11 device to create the CompositionGraphicsDevice from.
 * @returns The resulting CompositionGraphicsDevice.
 */
inline auto CreateCompositionGraphicsDevice(
  const winrt::Windows::UI::Composition::Compositor& compositor,
  IUnknown* device
) {
  winrt::Windows::UI::Composition::CompositionGraphicsDevice graphicsDevice{nullptr};
  auto compositorInterop = compositor.as<ABI::Windows::UI::Composition::ICompositorInterop>();
  winrt::com_ptr<ABI::Windows::UI::Composition::ICompositionGraphicsDevice> graphicsInterop;
  winrt::check_hresult(compositorInterop->CreateGraphicsDevice(device, graphicsInterop.put()));
  winrt::check_hresult(
    graphicsInterop->QueryInterface(
      winrt::guid_of<winrt::Windows::UI::Composition::CompositionGraphicsDevice>(),
      winrt::put_abi(graphicsDevice)
    )
  );
  return graphicsDevice;
}

/**
 * @brief Resizes a CompositionDrawingSurface to a new size. This is useful when the size of the visual element that the
 *        CompositionDrawingSurface is rendering changes.
 * @param surface The CompositionDrawingSurface to resize.
 * @param size The new size of the CompositionDrawingSurface.
 */
inline void ResizeSurface(
  const winrt::Windows::UI::Composition::CompositionDrawingSurface& surface,
  const winrt::Windows::Foundation::Size& size
) {
  auto surfaceInterop = surface.as<ABI::Windows::UI::Composition::ICompositionDrawingSurfaceInterop>();
  SIZE newSize = {};
  newSize.cx = static_cast<LONG>(std::round(size.Width));
  newSize.cy = static_cast<LONG>(std::round(size.Height));
  winrt::check_hresult(surfaceInterop->Resize(newSize));
}

/**
 * @brief Begins drawing to a CompositionDrawingSurface. This function returns a Direct2D device context that can be used
 *        to draw to the CompositionDrawingSurface. A device context is a drawing state that contains drawing attributes,
 *        such as the brush, the pen, and the drawing options.
 * @param surface The CompositionDrawingSurface to begin drawing to.
 * @returns The resulting Direct2D device context.
 */
inline auto SurfaceBeginDraw(
  const winrt::Windows::UI::Composition::CompositionDrawingSurface& surface
) {
  auto surfaceInterop = surface.as<ABI::Windows::UI::Composition::ICompositionDrawingSurfaceInterop>();
  winrt::com_ptr<ID2D1DeviceContext> context;
  POINT offset = {};
  winrt::check_hresult(surfaceInterop->BeginDraw(nullptr, __uuidof(ID2D1DeviceContext), context.put_void(), &offset));
  context->SetTransform(D2D1::Matrix3x2F::Translation(static_cast<FLOAT>(offset.x), static_cast<FLOAT>(offset.y)));
  return context;
}

/**
 * @brief Ends drawing to a CompositionDrawingSurface. This function should be called after drawing to a
 *        CompositionDrawingSurface is complete.
 * @param surface The CompositionDrawingSurface to end drawing to.
 */
inline void SurfaceEndDraw(
  const winrt::Windows::UI::Composition::CompositionDrawingSurface& surface
) {
  auto surfaceInterop = surface.as<ABI::Windows::UI::Composition::ICompositionDrawingSurfaceInterop>();
  winrt::check_hresult(surfaceInterop->EndDraw());
}

/**
 * @brief Creates a CompositionSurface from a DXGI swap chain. A CompositionSurface is used to create visual elements in a
 *        UWP application. A DXGI swap chain is used to present rendered frames to the screen. This is useful when you
 *        want to render a visual element to a swap chain, and then present the rendered frames to the screen.
 * @param compositor The Compositor to create the CompositionSurface from.
 * @param swapChain The DXGI swap chain to create the CompositionSurface from.
 * @returns The resulting CompositionSurface.
 */
inline auto CreateCompositionSurfaceForSwapChain(
  const winrt::Windows::UI::Composition::Compositor& compositor,
  IUnknown* swapChain
) {
  winrt::Windows::UI::Composition::ICompositionSurface surface{nullptr};
  auto compositorInterop = compositor.as<ABI::Windows::UI::Composition::ICompositorInterop>();
  winrt::com_ptr<ABI::Windows::UI::Composition::ICompositionSurface> surfaceInterop;
  winrt::check_hresult(compositorInterop->CreateCompositionSurfaceForSwapChain(swapChain, surfaceInterop.put()));
  winrt::check_hresult(
    surfaceInterop->QueryInterface(
      winrt::guid_of<winrt::Windows::UI::Composition::ICompositionSurface>(),
      winrt::put_abi(surface)
    )
  );
  return surface;
}
