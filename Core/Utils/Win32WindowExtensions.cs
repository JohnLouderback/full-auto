using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Core.Models;
using static Windows.Win32.PInvoke;
using static Core.Utils.Macros;
using static Core.Utils.NativeUtils;

namespace Core.Utils;

public static class Win32WindowExtensions {
  private const int baseDPI = 96;


  /// <summary>
  ///   Retrieves the immediate child windows of this window.
  /// </summary>
  /// <param name="window"> The window to retrieve the children of. </param>
  /// <returns>
  ///   An <see cref="IEnumerable{T}" /> of <see cref="Win32Window" /> that contains the immediate
  ///   child windows of this window.
  /// </returns>
  public static IEnumerable<Win32Window> Children(this Win32Window window) {
    return EnumerateChildWindows(window.Hwnd);
  }


  /// <summary>
  ///   Focuses the window, making it the active window.
  /// </summary>
  /// <param name="window"> The window to focus. </param>
  /// <exception cref="InvalidOperationException"> The window handle is invalid. </exception>
  public static void Focus(this Win32Window window) {
    if (window.Hwnd == nint.Zero) {
      throw new InvalidOperationException("The window handle is invalid.");
    }

    SetForegroundWindow(window.Hwnd);
  }


  /// <summary>
  ///   Get the absolute screen coordinates of the window's client area. This can be useful for
  ///   getting the delta between a parent window and a child window, for example.
  /// </summary>
  /// <param name="window"> </param>
  /// <returns> </returns>
  public static RECT GetAbsoluteClientRect(this Win32Window window) {
    var clientRect = window.GetClientRect();
    var topLeft = new Point {
      X = clientRect.left,
      Y = clientRect.top
    };
    var bottomRight = new Point {
      X = clientRect.right,
      Y = clientRect.bottom
    };

    ClientToScreen(window.Hwnd, ref topLeft);
    ClientToScreen(window.Hwnd, ref bottomRight);

    clientRect.left   += topLeft.X;
    clientRect.top    += topLeft.Y;
    clientRect.right  += bottomRight.X;
    clientRect.bottom += bottomRight.Y;
    return clientRect;
  }


  /// <summary>
  ///   Gets the height of this window's client area.
  /// </summary>
  /// <param name="window"> The window to get the height of. </param>
  /// <returns> The height of the window's client area. </returns>
  public static int GetClientHeight(this Win32Window window) {
    var rect = window.GetClientRect();
    return rect.bottom - rect.top;
  }


  /// <summary>
  ///   Gets the rectangle that represents the window's client area. The client area is the window's
  ///   content area, excluding the window frame.
  /// </summary>
  /// <param name="window"> The window to get the client rectangle of. </param>
  /// <returns> The rectangle that represents the window's client area. </returns>
  public static RECT GetClientRect(this Win32Window window) {
    PInvoke.GetClientRect(window.Hwnd, out var rect);
    return rect;
  }


  /// <summary>
  ///   Gets the rectangle that represents the window's client area relative to the window itself.
  ///   This is useful for operations such as "cropping" the window's client area when capturing
  ///   images of the entire window.
  /// </summary>
  /// <param name="window"> The window to get the client rectangle of. </param>
  /// <param name="scaleByDpi"> Whether to scale the client area by the DPI of the monitor. </param>
  /// <returns>
  ///   The rectangle that represents the window's client area relative to the window itself.
  /// </returns>
  public static unsafe RECT GetClientRectRelativeToWindow(
    this Win32Window window,
    bool scaleByDpi = false
  ) {
    var hwnd       = window.Hwnd;
    var clientRect = window.GetClientRect();

    // The top-left corner of the client area is the origin of the window. Usually, (0, 0).
    var topLeft = new Point {
      X = clientRect.left,
      Y = clientRect.top
    };

    // The bottom-right corner of the client area is the width and height of the window.
    var bottomRight = new Point {
      X = clientRect.right,
      Y = clientRect.bottom
    };

    // Convert the client area's top-left corner to screen coordinates.
    ClientToScreen(hwnd, ref topLeft);
    // Convert the client area's bottom-right corner to screen coordinates.
    ClientToScreen(hwnd, ref bottomRight);

    RECT extendedFrameBounds;
    var result = DwmGetWindowAttribute(
      hwnd,
      DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
      &extendedFrameBounds,
      (uint)sizeof(RECT)
    );

    var dpi = hwnd.GetMonitor().GetDpi();
    // Calculate the DPI scale. No scaling is done if the scaleByDpi parameter is false.
    var dpiScale = scaleByDpi ? dpi / (float)baseDPI : 1;

    if (SUCCEEDED(result)) {
      // Calculate the offset of the client area within the extended frame bounds.
      // Scale down the extended frame bounds by the DPI scale to get the correct client area. The extended frame
      // bounds are in physical pixels, so we need to scale them down to logical pixels.
      clientRect.left = (int)(topLeft.X - extendedFrameBounds.left / dpiScale);
      clientRect.top  = (int)(topLeft.Y - extendedFrameBounds.top / dpiScale);
      var clientWidth  = bottomRight.X - topLeft.X;
      var clientHeight = bottomRight.Y - topLeft.Y;
      clientRect.right  = clientRect.left + clientWidth;
      clientRect.bottom = clientRect.top + clientHeight;
    }
    else {
      // Handle the error case where DwmGetWindowAttribute might fail on non-DWM platforms.
      // Fallback to using GetWindowRect for non-composited desktops.
      RECT windowRect;
      PInvoke.GetWindowRect(hwnd, &windowRect);

      clientRect.left   = topLeft.X - windowRect.left;
      clientRect.top    = topLeft.Y - windowRect.top;
      clientRect.right  = clientRect.left + (clientRect.right - clientRect.left);
      clientRect.bottom = clientRect.top + (clientRect.bottom - clientRect.top);
    }

    return clientRect;
  }


  /// <summary>
  ///   Gets the width of this window's client area.
  /// </summary>
  /// <param name="window"> The window to get the width of. </param>
  /// <returns> The width of the window's client area. </returns>
  public static int GetClientWidth(this Win32Window window) {
    var rect = window.GetClientRect();
    return rect.right - rect.left;
  }


  /// <summary>
  ///   Gets the x-coordinate of the window's client area.
  /// </summary>
  /// <param name="window"> The window to get the x-coordinate of. </param>
  /// <returns> The x-coordinate of the window's client area. </returns>
  public static int GetClientX(this Win32Window window) {
    var rect = window.GetClientRect();
    return rect.left;
  }


  /// <summary>
  ///   Gets the y-coordinate of the window's client area.
  /// </summary>
  /// <param name="window"> The window to get the y-coordinate of. </param>
  /// <returns> The y-coordinate of the window's client area. </returns>
  public static int GetClientY(this Win32Window window) {
    var rect = window.GetClientRect();
    return rect.top;
  }


  /// <summary>
  ///   Gets the DPI of the window. The DPI is the number of dots that fit into a linear inch. The
  ///   DPI of a window with a 1:1 pixel density is 96. For 150% scaling, the DPI is 144. The DPI of
  ///   the window may not be the same as the DPI of the monitor. It depends on whether the window
  ///   is DPI-aware or not.
  /// </summary>
  /// <returns> The DPI of the window. </returns>
  public static uint GetDpi(this Win32Window window) {
    return GetDpiForWindow(window.Hwnd);
  }


  /// <summary>
  ///   Gets the height of this window, including the window frame.
  /// </summary>
  /// <param name="window"> The window to get the height of. </param>
  /// <returns> The height of the window, including the window frame. </returns>
  public static int GetHeight(this Win32Window window) {
    var rect = window.GetRect();
    return rect.bottom - rect.top;
  }


  /// <summary>
  ///   Gets the monitor that this window is on.
  /// </summary>
  /// <param name="window"> The window to get the monitor of. </param>
  /// <returns> The monitor that this window is on. </returns>
  public static Win32Monitor GetMonitor(this Win32Window window) {
    var hMonitor    = window.Hwnd.GetMonitor();
    var monitorInfo = hMonitor.GetMonitorInfoEx();
    var szDevice    = monitorInfo.szDevice.ToString();
    var device      = hMonitor.GetDisplayDeviceById(szDevice);
    return new Win32Monitor {
      HMonitor     = hMonitor,
      DeviceId     = device.DeviceID.ToString(),
      DeviceName   = device.DeviceName.ToString(),
      DeviceString = device.DeviceString.ToString(),
      DeviceKey    = device.DeviceKey.ToString(),
      MonitorRect  = monitorInfo.monitorInfo.rcMonitor,
      WorkArea     = monitorInfo.monitorInfo.rcWork,
      Dpi          = hMonitor.GetDpi(),
      IsPrimary    = (monitorInfo.monitorInfo.dwFlags & MONITORINFOF_PRIMARY) != 0
    };
  }


  /// <summary>
  ///   Gets the rectangle that represents the window's position and size.
  /// </summary>
  /// <param name="window"> The window to get the rectangle of. </param>
  /// <returns> The rectangle that represents the window's position and size. </returns>
  public static RECT GetRect(this Win32Window window) {
    PInvoke.GetWindowRect(window.Hwnd, out var rect);
    return rect;
  }


  /// <summary>
  ///   Gets the width of this window, including the window frame.
  /// </summary>
  /// <param name="window"> The window to get the width of. </param>
  /// <returns> The width of the window, including the window frame. </returns>
  public static int GetWidth(this Win32Window window) {
    var rect = window.GetRect();
    return rect.right - rect.left;
  }


  public static unsafe RECT GetWindowRect(
    this Win32Window window,
    bool scaleByDpi = false
  ) {
    var  hwnd = window.Hwnd;
    RECT windowRect;

    // Get the extended frame bounds of the window. This is the window's position and size including
    // the window frame. However, this only works on DWM-composited desktops. If this fails, we fall
    // back to using GetWindowRect.
    RECT extendedFrameBounds;
    var result = DwmGetWindowAttribute(
      hwnd,
      DWMWINDOWATTRIBUTE.DWMWA_EXTENDED_FRAME_BOUNDS,
      &extendedFrameBounds,
      (uint)sizeof(RECT)
    );

    var dpi = hwnd.GetMonitor().GetDpi();
    // Calculate the DPI scale. No scaling is done if the scaleByDpi parameter is false.
    var dpiScale = scaleByDpi ? dpi / (float)baseDPI : 1;

    if (SUCCEEDED(result)) {
      // Calculate the offset of the client area within the extended frame bounds.
      // Scale down the extended frame bounds by the DPI scale to get the correct client area. The extended frame
      // bounds are in physical pixels, so we need to scale them down to logical pixels.
      windowRect.left   = (int)(extendedFrameBounds.left / dpiScale);
      windowRect.top    = (int)(extendedFrameBounds.top / dpiScale);
      windowRect.right  = (int)(extendedFrameBounds.right / dpiScale);
      windowRect.bottom = (int)(extendedFrameBounds.bottom / dpiScale);
    }
    else {
      // Handle the error case where DwmGetWindowAttribute might fail on non-DWM platforms.
      // Fallback to using GetWindowRect for non-composited desktops.
      PInvoke.GetWindowRect(hwnd, &windowRect);
    }

    return windowRect;
  }


  /// <summary>
  ///   Gets the x-coordinate of the window.
  /// </summary>
  /// <param name="window"> The window to get the x-coordinate of. </param>
  /// <returns> The x-coordinate of the window. </returns>
  public static int GetX(this Win32Window window) {
    var rect = window.GetRect();
    return rect.left;
  }


  /// <summary>
  ///   Gets the y-coordinate of the window.
  /// </summary>
  /// <param name="window"> The window to get the y-coordinate of. </param>
  /// <returns> The y-coordinate of the window. </returns>
  public static int GetY(this Win32Window window) {
    var rect = window.GetRect();
    return rect.top;
  }


  /// <summary>
  ///   Determines whether the window has focus.
  /// </summary>
  /// <param name="window"> The window to check. </param>
  /// <returns>
  ///   <see langword="true" /> if the window has focus; otherwise, <see langword="false" />.
  /// </returns>
  public static bool HasFocus(this Win32Window window) {
    return GetFocus() == window.Hwnd;
  }
}
