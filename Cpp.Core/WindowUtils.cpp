#include "window-utils.h";

using namespace System;
using namespace Cpp::Core;

namespace Cpp::Core {
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
  };
}
