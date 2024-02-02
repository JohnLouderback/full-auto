/**
 * Contains helper functions for creating Direct3D and Direct2D resources.
 */

#pragma once
#include "composition.interop.h"

/**
 * @brief A struct representing a SurfaceContext. A SurfaceContext is a wrapper around a CompositionDrawingSurface and a
 *        ID2D1DeviceContext. This is used to simplify the process of drawing to a CompositionDrawingSurface.
 */
struct SurfaceContext {
public:
  SurfaceContext(std::nullptr_t) {}
  /**
   * @brief Initializes a new instance of the SurfaceContext class.
   * @param surface The CompositionDrawingSurface to wrap.
   */
  SurfaceContext(
    winrt::Windows::UI::Composition::CompositionDrawingSurface surface
  ) {
    m_surface = surface;
    m_d2dContext = SurfaceBeginDraw(m_surface);
  }

  ~SurfaceContext() {
    SurfaceEndDraw(m_surface);
    m_d2dContext = nullptr;
    m_surface = nullptr;
  }

  winrt::com_ptr<ID2D1DeviceContext> GetDeviceContext() { return m_d2dContext; }

private:
  winrt::com_ptr<ID2D1DeviceContext> m_d2dContext;
  winrt::Windows::UI::Composition::CompositionDrawingSurface m_surface{nullptr};
};

/**
 * @brief A struct representing a D3D11DeviceLock. This is used to lock the D3D11 device for multithreaded access.
 */
struct D3D11DeviceLock {
public:
  D3D11DeviceLock(std::nullopt_t) {}
  /**
   * @brief Initializes a new instance of the D3D11DeviceLock class.
   * @param pMultithread The `ID3D11Multithread` to lock.
   */
  D3D11DeviceLock(ID3D11Multithread* pMultithread) {
    m_multithread.copy_from(pMultithread);
    m_multithread->Enter();
  }

  ~D3D11DeviceLock() {
    m_multithread->Leave();
    m_multithread = nullptr;
  }

private:
  winrt::com_ptr<ID3D11Multithread> m_multithread;
};

/**
 * @brief Creates a new WICFactory. A WICFactory is used to create WIC resources, or Windows Imaging Component resources.
 * @return The newly created WICFactory.
 */
inline auto
CreateWICFactory() {
  winrt::com_ptr<IWICImagingFactory2> wicFactory;
  winrt::check_hresult(
    CoCreateInstance(
      CLSID_WICImagingFactory,
      nullptr,
      CLSCTX_INPROC_SERVER,
      winrt::guid_of<IWICImagingFactory>(),
      wicFactory.put_void()
    )
  );

  return wicFactory;
}

/**
 * @brief Creates a new D2DDevice. A Direct2D device represents a rendering device and provides a context for creating
 *        resources.
 * @param factory The D2D1Factory to use to create the device. This is typically the factory that was used to create the
 *                D3D11Device.
 * @param device The D3D11Device to use to create the device.
 * @return  The newly created D2DDevice.
 */
inline auto
CreateD2DDevice(
  const winrt::com_ptr<ID2D1Factory1>& factory,
  const winrt::com_ptr<ID3D11Device>& device
) {
  winrt::com_ptr<ID2D1Device> result;
  winrt::check_hresult(factory->CreateDevice(device.as<IDXGIDevice>().get(), result.put()));
  return result;
}

/**
 * @brief Creates a new D3DDevice. A Direct3D device represents a virtual adapter and provides a context for creating
 *        resources.
 * @param type The type of driver to use to create the device. 
 * @param device
 * @return  The newly created D3DDevice.
 */
inline auto
CreateD3DDevice(
  const D3D_DRIVER_TYPE type,
  winrt::com_ptr<ID3D11Device>& device
) {
  WINRT_ASSERT(!device);

  UINT flags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;

  //#ifdef _DEBUG
  //	flags |= D3D11_CREATE_DEVICE_DEBUG;
  //#endif

  return D3D11CreateDevice(
    nullptr,
    type,
    nullptr,
    flags,
    nullptr,
    0,
    D3D11_SDK_VERSION,
    device.put(),
    nullptr,
    nullptr
  );
}

/**
 * @brief Creates a new D3DDevice. A Direct3D device represents a virtual adapter and provides a context for creating
 *        resources.
 * @return  The newly created D3DDevice.
 */
inline auto
CreateD3DDevice() {
  winrt::com_ptr<ID3D11Device> device;
  HRESULT hr = CreateD3DDevice(D3D_DRIVER_TYPE_HARDWARE, device);

  if (DXGI_ERROR_UNSUPPORTED == hr)
  {
    hr = CreateD3DDevice(D3D_DRIVER_TYPE_WARP, device);
  }

  winrt::check_hresult(hr);
  return device;
}

/**
 * @brief Creates a new D2DFactory. A D2D1Factory is used to create Direct2D resources.
 * @return The newly created D2DFactory.
 */
inline auto
CreateD2DFactory() {
  D2D1_FACTORY_OPTIONS options{};

  //#ifdef _DEBUG
  //	options.debugLevel = D2D1_DEBUG_LEVEL_INFORMATION;
  //#endif

  winrt::com_ptr<ID2D1Factory1> factory;

  winrt::check_hresult(
    D2D1CreateFactory(
      D2D1_FACTORY_TYPE_SINGLE_THREADED,
      options,
      factory.put()
    )
  );

  return factory;
}

/**
 * @brief Creates a new DXGI swap chain. DXGI is a set of components for enumerating, configuring, and managing
 * devices and monitors. The swap chain is used to present rendered frames to the screen.
 * @param device The D3D11Device to use to create the swap chain.
 * @param desc The descriptor for the swap chain. This is used to configure the swap chain.
 * @return 
 */
inline auto
CreateDXGISwapChain(
  const winrt::com_ptr<ID3D11Device>& device,
  const DXGI_SWAP_CHAIN_DESC1* desc
) {
  auto dxgiDevice = device.as<IDXGIDevice2>();
  winrt::com_ptr<IDXGIAdapter> adapter;
  winrt::check_hresult(dxgiDevice->GetParent(winrt::guid_of<IDXGIAdapter>(), adapter.put_void()));
  winrt::com_ptr<IDXGIFactory2> factory;
  winrt::check_hresult(adapter->GetParent(winrt::guid_of<IDXGIFactory2>(), factory.put_void()));

  winrt::com_ptr<IDXGISwapChain1> swapchain;
  winrt::check_hresult(
    factory->CreateSwapChainForComposition(
      device.get(),
      desc,
      nullptr,
      swapchain.put()
    )
  );

  return swapchain;
}

/**
 * @brief Creates a new DXGI swap chain. DXGI is a set of components for enumerating, configuring, and managing
 * devices and monitors. The swap chain is used to present rendered frames to the screen.
 * @param device The D3D11Device to use to create the swap chain.
 * @param width The width of the textures in the swap chain.
 * @param height The height of the textures in the swap chain.
 * @param format The format of the textures in the swap chain.
 * @param bufferCount The number of buffers in the swap chain.
 * @return 
 */
inline auto
CreateDXGISwapChain(
  const winrt::com_ptr<ID3D11Device>& device,
  uint32_t width,
  uint32_t height,
  DXGI_FORMAT format,
  uint32_t bufferCount
) {
  DXGI_SWAP_CHAIN_DESC1 desc = {};
  desc.Width = width;
  desc.Height = height;
  desc.Format = format;
  desc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
  desc.SampleDesc.Count = 1;
  desc.SampleDesc.Quality = 0;
  desc.BufferCount = bufferCount;
  desc.Scaling = DXGI_SCALING_STRETCH;
  desc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
  desc.AlphaMode = DXGI_ALPHA_MODE_PREMULTIPLIED;

  return CreateDXGISwapChain(device, &desc);
}
