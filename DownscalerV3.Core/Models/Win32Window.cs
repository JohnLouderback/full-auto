using Windows.Win32.Foundation;

namespace DownscalerV3.Core.Models;

public struct Win32Window {
  public HWND Hwnd { get; set; }

  public string Title { get; set; }

  public string ClassName { get; set; }

  public string ProcessName { get; set; }
}
