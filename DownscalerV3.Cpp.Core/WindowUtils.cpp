#include "window-utils.h";
#include "DownscalerV3.Cpp.WinRT.h"

using namespace System;
using namespace DownscalerV3::Cpp::Core;
using namespace DownscalerV3;

namespace DownscalerV3::Cpp::Core {
  public ref class WindowUtils {
    public:
      /**
       * @brief Gets the process name for a window.
       * @param hwnd The window handle.
       * @return The process name for the window as a managed string.
       */
      static String^ GetProcessName(IntPtr hwnd) {
        // Convert IntPtr to HWND (native handle)
        auto nativeHwnd = reinterpret_cast<HWND>(hwnd.ToPointer());

        auto processName = NativeImpls::GetProcessName(nativeHwnd);
        return gcnew String(processName.data());
      }

      /**
       * @brief Given a window handle, returns a Windows.Graphics.Capture.GraphicsCaptureItem for that window.
       * @param hwnd The window handle to create a capture item for.
       * @returns A pointer to the IGraphicsCaptureItem COM interface for the window.
       */
      static IntPtr CreateCaptureItemForWindow(IntPtr hwnd) {
        // Convert IntPtr to HWND (native handle)
        auto nativeHwnd = reinterpret_cast<HWND>(hwnd.ToPointer());

        // Call the function from the Cpp.WinRT static library.
        auto captureItemInterface = WinRT::CreateCaptureItemForWindow(nativeHwnd);

        // Convert the raw pointer to a System::IntPtr.
        return IntPtr(captureItemInterface);
      }
  };
}
