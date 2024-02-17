using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Cpp.Core;

#pragma warning disable CA1416

namespace DownscalerV3.Core.Utils;

public static class HwndExtensions {
  public static unsafe string GetClassName(this HWND hwnd) {
    var bufferSize = 1024;
    using (var classNameBuffer = new NativeBuffer<char>(bufferSize)) {
      PInvoke.GetClassName(hwnd, new PWSTR(classNameBuffer.GetPointer()), bufferSize);
      return classNameBuffer.ToManagedString(Encoding.Unicode);
    }
  }


  public static string GetProcessName(this HWND hwnd) {
    return WindowUtils.GetProcessName((int)hwnd.Value);
  }


  public static unsafe string GetWindowText(this HWND hwnd) {
    var bufferSize = 1024;
    using (var textBuffer = new NativeBuffer<char>(bufferSize)) {
      PInvoke.GetWindowText(hwnd, new PWSTR(textBuffer.GetPointer()), bufferSize);
      return textBuffer.ToManagedString(Encoding.Unicode);
    }
  }
}

#pragma warning restore CA1416
