#include "window-utils.h";

using namespace System;
using namespace Cpp::Core;

namespace Cpp::Core {
  public ref class WindowUtils {
    public:
      static String^ GetProcessName(int hwnd) {
        auto processName = NativeImpls::GetProcessName(reinterpret_cast<HWND>(hwnd));
        return gcnew String(processName.data());
      }
  };
}
