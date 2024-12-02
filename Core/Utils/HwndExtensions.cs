using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using static Core.Utils.Macros;

#pragma warning disable CA1416

namespace Core.Utils;

public static class HwndExtensions {
  public static unsafe string GetClassName(this HWND hwnd) {
    var bufferSize = 1024;
    using (var classNameBuffer = new NativeBuffer<char>(bufferSize)) {
      PInvoke.GetClassName(hwnd, new PWSTR(classNameBuffer.GetPointer()), bufferSize);
      return classNameBuffer.ToManagedString(Encoding.Unicode).TrimEnd('\0');
    }
  }


  /// <summary>
  ///   Gets the DPI of the window. For example a 1:1 scaling would return 96. Scaling of 150% would
  ///   return 144. Due to applications not necessarily being DPI aware, it is often more reliable
  ///   to get the DPI of the monitor the window is on.
  /// </summary>
  /// <param name="hwnd"> The window to get the DPI of. </param>
  /// <returns> The DPI of the window. </returns>
  public static uint GetDpi(this HWND hwnd) {
    return PInvoke.GetDpiForWindow(hwnd);
  }


  /// <summary>
  ///   Get the monitor handle for the monitor on which this window currently resides.
  /// </summary>
  /// <param name="hwnd"> The window to get the monitor of. </param>
  /// <returns> The monitor handle for the monitor on which this window currently resides. </returns>
  public static HMONITOR GetMonitor(this HWND hwnd) {
    return PInvoke.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
  }


  public static string GetProcessName(this HWND hwnd) {
    return Cpp.Core.WindowUtils.GetProcessName(hwnd.Value).TrimEnd('\0');
  }


  public static WINDOW_EX_STYLE GetWindowExStyle(this HWND hwnd) {
    return (WINDOW_EX_STYLE)hwnd.GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
  }


  public static nint GetWindowLong(this HWND hwnd, WINDOW_LONG_PTR_INDEX index) {
    return Win32Ex.GetWindowLong(hwnd, index);
  }


  public static WINDOW_STYLE GetWindowStyle(this HWND hwnd) {
    return (WINDOW_STYLE)hwnd.GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE);
  }


  public static unsafe string GetWindowText(this HWND hwnd) {
    var bufferSize = 1024;
    using (var textBuffer = new NativeBuffer<char>(bufferSize)) {
      PInvoke.GetWindowText(hwnd, new PWSTR(textBuffer.GetPointer()), bufferSize);
      return textBuffer.ToManagedString(Encoding.Unicode).TrimEnd('\0');
    }
  }


  /// <summary>
  ///   Removes the owner from the given window. If the window has no owner, then this method does
  ///   nothing and returns the window.
  /// </summary>
  /// <param name="window"> The window to remove the owner from. </param>
  /// <returns> The same window this method was called on. </returns>
  public static HWND RemoveOwner(this HWND window) {
    // Get the owner of the window.
    var hOwner = PInvoke.GetWindow(window, GET_WINDOW_CMD.GW_OWNER);

    // If the window has no owner, then do nothing and return the window.
    if (hOwner == nullptr) return window;

    // Remove the owner from the window.
    window.SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_HWNDPARENT, NULL);

    // Update the window's position to reflect the change in ownership.
    window.SetWindowPosition();

    return window;
  }


  public static HWND SetParent(this HWND hwnd, HWND parent) {
    var result = PInvoke.SetParent(hwnd, parent);

    if (result == nint.Zero) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    return hwnd;
  }


  public static HWND SetWindowExStyle(this HWND hwnd, WINDOW_EX_STYLE style) {
    return hwnd.SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, (int)style);
  }


  public static HWND SetWindowLong(this HWND hwnd, WINDOW_LONG_PTR_INDEX index, int value) {
    var result = Win32Ex.SetWindowLong(hwnd, index, value);

    if (result == 0) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    return hwnd;
  }


  public static HWND SetWindowPosition(this HWND hwnd, int x, int y, int width, int height) {
    PInvoke.SetWindowPos(
      hwnd,
      (HWND)(nint)WindowZOrder.HWND_TOP,
      x,
      y,
      width,
      height,
      SET_WINDOW_POS_FLAGS.SWP_NOZORDER
    );
    return hwnd;
  }


  public static HWND SetWindowPosition(
    this HWND hwnd,
    int x,
    int y,
    int width,
    int height,
    SET_WINDOW_POS_FLAGS flags
  ) {
    PInvoke.SetWindowPos(hwnd, (HWND)(nint)WindowZOrder.HWND_TOP, x, y, width, height, flags);
    return hwnd;
  }


  public static HWND SetWindowPosition(
    this HWND hwnd,
    int x,
    int y,
    int width,
    int height,
    WindowZOrder zOrder
  ) {
    PInvoke.SetWindowPos(
      hwnd,
      (HWND)(nint)zOrder,
      x,
      y,
      width,
      height,
      SET_WINDOW_POS_FLAGS.SWP_NOZORDER
    );
    return hwnd;
  }


  public static HWND SetWindowPosition(
    this HWND hwnd,
    int x,
    int y,
    int width,
    int height,
    WindowZOrder zOrder,
    SET_WINDOW_POS_FLAGS flags
  ) {
    PInvoke.SetWindowPos(hwnd, (HWND)(nint)zOrder, x, y, width, height, flags);
    return hwnd;
  }


  public static HWND SetWindowPosition(
    this HWND hwnd,
    int x,
    int y,
    int width,
    int height,
    HWND zOrder
  ) {
    PInvoke.SetWindowPos(
      hwnd,
      zOrder,
      x,
      y,
      width,
      height,
      SET_WINDOW_POS_FLAGS.SWP_NOZORDER
    );
    return hwnd;
  }


  public static HWND SetWindowPosition(
    this HWND hwnd,
    int x,
    int y,
    int width,
    int height,
    HWND zOrder,
    SET_WINDOW_POS_FLAGS flags
  ) {
    var result = PInvoke.SetWindowPos(hwnd, zOrder, x, y, width, height, flags);
    return hwnd;
  }


  public static HWND SetWindowPosition(this HWND hwnd) {
    // This a no-op essentially, but it can be used update window relationships, like ownership.
    var result = PInvoke.SetWindowPos(
      hwnd,
      (HWND)0,
      0,
      0,
      0,
      0,
      SET_WINDOW_POS_FLAGS.SWP_NOMOVE |
      SET_WINDOW_POS_FLAGS.SWP_NOSIZE |
      SET_WINDOW_POS_FLAGS.SWP_NOZORDER
    );
    return hwnd;
  }


  public static HWND SetWindowStyle(this HWND hwnd, WINDOW_STYLE style) {
    return hwnd.SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)style);
  }


  public static HWND Show(this HWND hwnd) {
    PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_SHOW);
    return hwnd;
  }


  public static HWND Update(this HWND hwnd) {
    PInvoke.UpdateWindow(hwnd);
    return hwnd;
  }
}

#pragma warning restore CA1416
