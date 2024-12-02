using Windows.Win32.Foundation;
using Downscaler.Cpp.Core;

namespace Downscaler.Core.Utils;

public static class HwndExtensions {
  public static nint CreateCaptureItem(this HWND hwnd) {
    return WindowUtils.CreateCaptureItemForWindow(hwnd.Value);
  }
}
