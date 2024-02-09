/**
 * A series of helper functions for working with Direct3D 11. These functions are used to create Direct3D 11 resources
 * from DXGI resources. Direct 3D 11 is the API used to interact with the GPU. DXGI is the API used to interact with the
 * operating system's graphics stack. DXGI stands for DirectX Graphics Infrastructure.
 */

#pragma once
#include <winrt/windows.graphics.directx.direct3d11.h>

extern "C" {
/**
 * @brief Creates a Direct3D 11 device from a DXGI device. The difference between a DXGI device and a Direct3D 11
 *        device is that the DXGI device is used to interact with the operating system's graphics stack, while the
 *        Direct3D 11 device is used to interact with the GPU.
 * @param dxgiDevice The DXGI device to create the Direct3D 11 device from.
 * @param graphicsDevice The resulting Direct3D 11 device.
 * @return An HRESULT indicating success or failure.
 */
HRESULT __stdcall CreateDirect3D11DeviceFromDXGIDevice(
  IDXGIDevice* dxgiDevice,
  IInspectable** graphicsDevice
);

/**
 * @brief Creates a Direct3D 11 surface from a DXGI surface. The difference between a DXGI surface and a Direct3D 11
 *        surface is that the DXGI surface is used to interact with the operating system's graphics stack, while the
 *        Direct3D 11 surface is used to interact with the GPU.
 * @param dgxiSurface The DXGI surface to create the Direct3D 11 surface from.
 * @param graphicsSurface The resulting Direct3D 11 surface.
 * @return An HRESULT indicating success or failure.
 */
HRESULT __stdcall CreateDirect3D11SurfaceFromDXGISurface(
  IDXGISurface* dgxiSurface,
  IInspectable** graphicsSurface
);
}

/**
 * @brief An interface for accessing DXGI interfaces from Windows Runtime objects.
 */
struct __declspec(uuid("A9B3D012-3DF2-4EE3-B8D1-8695F457D3C1"))
  IDirect3DDxgiInterfaceAccess : IUnknown {
  virtual HRESULT __stdcall GetInterface(const GUID& id, void** object) = 0;
};

/**
 * @brief Creates a Direct3D 11 device from a DXGI device.
 * @param dxgi_device The DXGI device to create the Direct3D 11 device from.
 * @return The resulting Direct3D 11 device.
 */
inline auto CreateDirect3DDevice(IDXGIDevice* dxgi_device) {
  winrt::com_ptr<IInspectable> d3d_device;
  winrt::check_hresult(CreateDirect3D11DeviceFromDXGIDevice(dxgi_device, d3d_device.put()));
  return d3d_device.as<winrt::Windows::Graphics::DirectX::Direct3D11::IDirect3DDevice>();
}

/**
 * @brief Creates a Direct3D 11 surface from a DXGI surface.
 * @param dxgi_surface The DXGI surface to create the Direct3D 11 surface from.
 * @return The resulting Direct3D 11 surface.
 */
inline auto CreateDirect3DSurface(IDXGISurface* dxgi_surface) {
  winrt::com_ptr<IInspectable> d3d_surface;
  winrt::check_hresult(CreateDirect3D11SurfaceFromDXGISurface(dxgi_surface, d3d_surface.put()));
  return d3d_surface.as<winrt::Windows::Graphics::DirectX::Direct3D11::IDirect3DSurface>();
}

/**
 * @brief Gets a DXGI interface from a Windows Runtime object.
 * @param object The Windows Runtime object to get the DXGI interface from.
 * @return The DXGI interface.
 */
template <typename T>
auto GetDXGIInterfaceFromObject(const winrt::Windows::Foundation::IInspectable& object) {
  auto access = object.as<IDirect3DDxgiInterfaceAccess>();
  winrt::com_ptr<T> result;
  winrt::check_hresult(access->GetInterface(winrt::guid_of<T>(), result.put_void()));
  return result;
}
