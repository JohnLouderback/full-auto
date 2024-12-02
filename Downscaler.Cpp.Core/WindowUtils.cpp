#include <dwmapi.h>
#include "Downscaler.Cpp.WinRT.h"

using namespace System;
using namespace Downscaler;

namespace Downscaler::Cpp::Core {
  public ref class WindowUtils {
    public:
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
