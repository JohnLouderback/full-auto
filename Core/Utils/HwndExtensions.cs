using System.ComponentModel;
using System.Drawing;
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


  public static HWND? GetOwner(this HWND hwnd) {
    var result = PInvoke.GetWindow(hwnd, GET_WINDOW_CMD.GW_OWNER);

    if (result == nint.Zero) {
      return null;
    }

    return result;
  }


  /// <summary>
  ///   Get the parent of the window. If the window has no parent, then this method returns
  ///   <see langword="null" />.
  /// </summary>
  /// <param name="hwnd"> The window to get the parent of. </param>
  /// <returns> The parent of the window, or <see langword="null" /> if the window has no parent. </returns>
  public static HWND? GetParent(this HWND hwnd) {
    var result = PInvoke.GetParent(hwnd);

    if (result == nint.Zero) {
      return null;
    }

    return result;
  }


  /// <summary>
  ///   Gets the process ID of the window.
  /// </summary>
  /// <param name="hwnd"> The window to get the process ID of. </param>
  /// <returns> The identifier of the process that created the window. </returns>
  /// <exception cref="Win32Exception"> Thrown when the process ID could not be retrieved. </exception>
  public static unsafe uint GetProcessID(this HWND hwnd) {
    // Allocate a single uint on the stack to store the process ID.
    var processId = stackalloc uint[1];
    var result    = PInvoke.GetWindowThreadProcessId(hwnd, processId);

    if (result == 0) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    return *processId;
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


  /// <summary>
  ///   Get the window placement of the window. This includes information about the window's
  ///   position, size, and state.
  /// </summary>
  /// <param name="hwnd"> The window to get the placement of. </param>
  /// <returns> The window placement of the window. </returns>
  /// <exception cref="Win32Exception"> Thrown when the window placement could not be retrieved. </exception>
  public static WINDOWPLACEMENT GetWindowPlacement(this HWND hwnd) {
    var placement = new WINDOWPLACEMENT {
      length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>()
    };

    if (PInvoke.GetWindowPlacement(hwnd, ref placement)) {
      return placement;
    }

    throw new Win32Exception(Marshal.GetLastWin32Error());
  }


  public static Rectangle GetWindowRect(this HWND hwnd) {
    PInvoke.GetWindowRect(hwnd, out var rect);
    return rect;
  }


  public static WINDOW_STYLE GetWindowStyle(this HWND hwnd) {
    return (WINDOW_STYLE)hwnd.GetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE);
  }


  /// <summary>
  ///   Get the text of the window's titlebar.
  /// </summary>
  /// <param name="hwnd"> The window to get the text of. </param>
  /// <returns> The text of the window's titlebar. </returns>
  public static unsafe string GetWindowText(this HWND hwnd) {
    var bufferSize = 1024;
    using (var textBuffer = new NativeBuffer<char>(bufferSize)) {
      PInvoke.GetWindowText(hwnd, new PWSTR(textBuffer.GetPointer()), bufferSize);
      return textBuffer.ToManagedString(Encoding.Unicode).TrimEnd('\0');
    }
  }


  public static bool HasOwner(this HWND hwnd) {
    return hwnd.GetOwner() != null;
  }


  /// <summary>
  ///   Check if the window has a parent.
  /// </summary>
  /// <param name="hwnd"> The window to check. </param>
  /// <returns> Whether the window has a parent. </returns>
  public static bool HasParent(this HWND hwnd) {
    return hwnd.GetParent() != null;
  }


  public static HWND Hide(this HWND hwnd) {
    PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_HIDE);
    return hwnd;
  }


  /// <summary>
  ///   Whether the window is the foreground window.
  /// </summary>
  /// <param name="hwnd"> The window handle to check. </param>
  /// <returns> Whether the window is the foreground window. </returns>
  public static bool IsForegroundWindow(this HWND hwnd) {
    return hwnd == PInvoke.GetForegroundWindow();
  }


  /// <summary>
  ///   Determines if the given window handle is an existing window. Useful for checking if the
  ///   window is still open.
  /// </summary>
  /// <param name="hwnd"> The window handle to check. </param>
  /// <returns> Whether the window handle is an existing window. </returns>
  public static bool IsWindow(this HWND hwnd) {
    return PInvoke.IsWindow(hwnd);
  }


  /// <summary>
  ///   Whether the window is visible. This implies that the window is "shown" and not minimized.
  /// </summary>
  /// <param name="hwnd"> The window to check. </param>
  /// <returns> Whether the window is visible. </returns>
  public static bool IsWindowVisible(this HWND hwnd) {
    return PInvoke.IsWindowVisible(hwnd);
  }


  /// <summary>
  ///   Post a message to the window. This is a wrapper around the PostMessage function. This differs
  ///   from SendMessage in that it does not wait for the message to be processed before returning.
  ///   This is useful for sending messages that do not require a response or that can be processed
  ///   later.
  /// </summary>
  /// <param name="hwnd"> The window to post the message to. </param>
  /// <param name="message"> The message to post. </param>
  /// <param name="wParam">
  ///   The word parameter. Typically used for sending information such as the control ID or
  ///   the state of a control.
  /// </param>
  /// <param name="lParam">
  ///   The long parameter. Typically used for sending information such as the position of a mouse
  ///   click or the state of a key.
  /// </param>
  public static void PostMessage(
    this HWND hwnd,
    Msg message,
    WPARAM wParam = default,
    LPARAM lParam = default
  ) {
    if (PInvoke.PostMessage(hwnd, (uint)message, wParam, lParam) == 0) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
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


  /// <summary>
  ///   Send a message to the window. This is a wrapper around the SendMessage function. This differs
  ///   from PostMessage in that it waits for the message to be processed before returning. This is
  ///   useful for sending messages that require a response or that need to be processed immediately.
  /// </summary>
  /// <param name="hwnd"> The window to send the message to. </param>
  /// <param name="message"> The message to send. </param>
  /// <param name="wParam">
  ///   The word parameter. Typically used for sending information such as the control ID or
  ///   the state of a control.
  /// </param>
  /// <param name="lParam">
  ///   The long parameter. Typically used for sending information such as the position of a mouse
  ///   click or the state of a key.
  /// </param>
  public static void SendMessage(
    this HWND hwnd,
    Msg message,
    WPARAM wParam = default,
    LPARAM lParam = default
  ) {
    if (PInvoke.SendMessage(hwnd, (uint)message, wParam, lParam) == 0) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }
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


  /// <summary>
  ///   Set the window placement command of the window. Such as minimizing, maximizing, or restoring the
  ///   window.
  /// </summary>
  /// <param name="hwnd"> The window to set the placement command of. </param>
  /// <param name="showCmd"> The command to set the window to. </param>
  /// <exception cref="Win32Exception"> Thrown when the window placement could not be set. </exception>
  public static void SetWindowPlacementCommand(
    this HWND hwnd,
    SHOW_WINDOW_CMD showCmd
  ) {
    var placement = new WINDOWPLACEMENT {
      length  = (uint)Marshal.SizeOf<WINDOWPLACEMENT>(),
      showCmd = showCmd
    };

    if (!PInvoke.SetWindowPlacement(hwnd, in placement)) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }
  }


  public static HWND SetWindowPosition(this HWND hwnd, int x, int y) {
    if (PInvoke.SetWindowPos(
          hwnd,
          (HWND)0,
          x,
          y,
          cx: 0,
          cy: 0,
          SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOSIZE
        ) ==
        0) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    return hwnd;
  }


  public static HWND SetWindowPosition(this HWND hwnd, int x, int y, int width, int height) {
    PInvoke.SetWindowPos(
      hwnd,
      (HWND)0,
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
      X: 0,
      Y: 0,
      cx: 0,
      cy: 0,
      SET_WINDOW_POS_FLAGS.SWP_NOMOVE |
      SET_WINDOW_POS_FLAGS.SWP_NOSIZE |
      SET_WINDOW_POS_FLAGS.SWP_NOZORDER
    );
    return hwnd;
  }


  public static HWND SetWindowStyle(this HWND hwnd, WINDOW_STYLE style) {
    return hwnd.SetWindowLong(WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)style);
  }


  /// <summary>
  ///   Set the text of the window's titlebar.
  /// </summary>
  /// <param name="hwnd"> The window to set the text of. </param>
  /// <param name="text"> The text to set the titlebar to. </param>
  /// <returns> The same window this method was called on. </returns>
  public static unsafe HWND SetWindowText(this HWND hwnd, string text) {
    fixed (char* textPtr = text) {
      if (PInvoke.SetWindowText(hwnd, textPtr) == 0) {
        throw new Win32Exception(Marshal.GetLastWin32Error());
      }
    }

    return hwnd;
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
