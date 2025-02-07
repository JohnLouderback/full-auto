using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using Core.Models;
using static Windows.Win32.PInvoke;

namespace Core.Utils;

/// <summary>
///   The size of a pointer in bytes.
/// </summary>
public enum PointerSize {
  /// <summary>
  ///   The size of a pointer is 4 bytes - 32 bits.
  /// </summary>
  PointerSize32 = 4,

  /// <summary>
  ///   The size of a pointer is 8 bytes - 64 bits.
  /// </summary>
  PointerSize64 = 8
}

public static class NativeUtils {
  /// <summary>
  ///   Copies a string to a fixed-size buffer. This is useful taking managed strings and copying
  ///   them to fixed-size buffers in native blittable structs.
  /// </summary>
  /// <param name="source"> The string to copy. </param>
  /// <param name="destination"> The pointer to the buffer to copy the string to. </param>
  /// <param name="bufferSize"> The size of the buffer. </param>
  public static unsafe void CopyStringToFixedBuffer(
    string source,
    char* destination,
    int bufferSize
  ) {
    if (string.IsNullOrEmpty(source)) return;
    // Get the length of the string, but don't exceed the buffer size.
    var length = Math.Min(bufferSize - 1, source.Length);

    // Iterate over the characters of the string and copy them to the buffer.
    for (var i = 0; i < length; i++) {
      destination[i] = source[i];
    }

    destination[length] = '\0'; // Null-terminate the string
  }


  /// <summary>
  ///   Copies a string to a fixed-size buffer. This is useful taking managed strings and copying
  ///   them to fixed-size buffers in native blittable structs. This overload assumes that the
  ///   source string's length is less than or equal to the buffer size. Caution must be taken to
  ///   ensure that the source string's length is less than or equal to the buffer size, otherwise
  ///   the buffer will overflow.
  /// </summary>
  /// <param name="source"> The string to copy. </param>
  /// <param name="destination"> The pointer to the buffer to copy the string to. </param>
  public static unsafe void CopyStringToFixedBuffer(string source, char* destination) {
    if (string.IsNullOrEmpty(source)) return;

    var length = source.Length;
    for (var i = 0; i < length; i++) {
      destination[i] = source[i];
    }

    destination[length] = '\0'; // Null-terminate the string
  }


  public static unsafe HWND CreateNewWindow(
    string className,
    string windowName,
    WINDOW_STYLE style,
    WNDPROC windowProcedure,
    int x,
    int y,
    int width,
    int height,
    HWND? parent = null,
    HINSTANCE? instance = null,
    LPARAM? param = null
  ) {
    var windowClass = new WNDCLASSEXW {
      cbSize        = (uint)Marshal.SizeOf(typeof(WNDCLASSEXW)),
      style         = 0,
      lpfnWndProc   = windowProcedure,
      cbClsExtra    = 0,
      cbWndExtra    = 0,
      hInstance     = Win32Ex.GetModuleHandle(0),
      hIcon         = new HICON(nint.Zero),
      hCursor       = new HCURSOR(nint.Zero),
      hbrBackground = new HBRUSH(5), // BACKGROUND_WHITE,
      lpszMenuName  = null,
      lpszClassName = className.ToPWSTR(),
      hIconSm       = new HICON(nint.Zero)
    };

    var classAtom = RegisterClassEx(in windowClass);
    if (classAtom == 0) {
      throw new Win32Exception();
    }

    var parentHwnd = parent ?? new HWND(nint.Zero);

    var hwnd = CreateWindowEx(
      0,
      className,
      windowName,
      style,
      x,
      y,
      width,
      height,
      parentHwnd,
      null,
      Win32Ex.GetModuleHandle(),
      (void*)nint.Zero
    );

    if (hwnd == nint.Zero) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    return hwnd;
  }


  /// <summary>
  ///   Given a window handle, enumerates all child windows of that window.
  /// </summary>
  /// <param name="hwnd"> The handle of the window to enumerate the children of. </param>
  /// <returns>
  ///   An <see cref="IEnumerable{T}" /> of <see cref="Win32Window" /> that contains all child
  ///   windows of the window with the given handle.
  /// </returns>
  public static IEnumerable<Win32Window> EnumerateChildWindows(HWND hwnd) {
    var result = new List<Win32Window>();

    // Call the native EnumWindows function, passing the results list to the callback function
    // factory.
    EnumChildWindows(hwnd, GetWndEnumProc(result), nint.Zero);

    // Return the results.
    return result;
  }


  /// <summary>
  ///   Given a window handle, enumerates all child windows of that window recursively, deeply
  ///   traversing the window hierarchy.
  /// </summary>
  /// <param name="hwnd"> The handle of the window to enumerate the children of. </param>
  /// <returns>
  ///   An <see cref="IEnumerable{T}" /> of <see cref="Win32Window" /> that contains all child
  ///   windows of the window with the given handle.
  /// </returns>
  public static IEnumerable<Win32Window> EnumerateChildWindowsRecursively(HWND hwnd) {
    var result = new List<Win32Window>();
    // Get the direct children of the window.
    var children = EnumerateChildWindows(hwnd);
    // Add the direct children to the result.
    result.AddRange(children);

    // For each child, get its children and add them to the result.
    foreach (var child in children) {
      result.AddRange(EnumerateChildWindowsRecursively(child.Hwnd));
    }

    // Return the aggregated result.
    return result;
  }


  /// <summary>
  ///   Enumerates all connected monitors on the system at the time of the call.
  /// </summary>
  /// <returns>
  ///   An <see cref="IEnumerable{T}" /> of <see cref="Win32Monitor" /> that contains all connected
  ///   monitors on the system at the time of the call.
  /// </returns>
  public static IEnumerable<Win32Monitor> EnumerateMonitors() {
    var result          = new List<Win32Monitor>();
    var monitorEnumProd = GetMonitorEnumProc(result);
    if (!EnumDisplayMonitors((HDC)nint.Zero, null as RECT?, monitorEnumProd, nint.Zero)) {
      throw new Win32Exception("Failed to enumerate monitors.");
    }

    return result;
  }


  /// <summary>
  ///   Enumerates all top-level windows on the system at the time of the call.
  /// </summary>
  /// <returns>
  ///   An <see cref="IEnumerable{T}" /> of <see cref="Win32Window" /> that contains all top-level
  ///   windows on the system at the time of the call.
  /// </returns>
  public static IEnumerable<Win32Window> EnumerateWindows() {
    var result = new List<Win32Window>();

    // Call the native EnumWindows function, passing the results list to the callback function
    // factory.
    EnumWindows(GetWndEnumProc(result), nint.Zero);

    // Return the results.
    return result;
  }


  /// <summary>
  ///   Returns a <see cref="MONITORENUMPROC" /> that can be used to enumerate monitors and add them to
  ///   a list given a list to store the results in.
  /// </summary>
  /// <param name="result"> The list to store the results in. </param>
  /// <returns> A <see cref="MONITORENUMPROC" /> that can be used to enumerate monitors. </returns>
  public static unsafe MONITORENUMPROC GetMonitorEnumProc(ICollection<Win32Monitor> result) {
    return (hMonitor, hdcMonitor, lprcMonitor, dwData) => {
      var monitorInfo = hMonitor.GetMonitorInfoEx();
      var szDevice    = monitorInfo.szDevice.ToString();
      var device      = hMonitor.GetDisplayDeviceById(szDevice);

      result.Add(
        new Win32Monitor {
          HMonitor     = hMonitor,
          DeviceId     = device.DeviceID.ToString(),
          DeviceName   = device.DeviceName.ToString(),
          DeviceString = device.DeviceString.ToString(),
          DeviceKey    = device.DeviceKey.ToString(),
          MonitorRect  = monitorInfo.monitorInfo.rcMonitor,
          WorkArea     = monitorInfo.monitorInfo.rcWork,
          Dpi          = hMonitor.GetDpi(),
          IsPrimary    = (monitorInfo.monitorInfo.dwFlags & MONITORINFOF_PRIMARY) != 0
        }
      );
      return true;
    };
  }


  /// <summary>
  ///   Gets the size of a pointer for the current platform.
  /// </summary>
  /// <returns> The size of a pointer for the current platform. </returns>
  public static PointerSize GetPointerSize() {
    return nint.Size == 4 ? PointerSize.PointerSize32 : PointerSize.PointerSize64;
  }


  /// <summary>
  ///   Returns a <see cref="WNDENUMPROC" /> that can be used to enumerate windows and add them to
  ///   a list given a list to store the results in.
  /// </summary>
  /// <param name="result"> The list to store the results in. </param>
  /// <returns> A <see cref="WNDENUMPROC" /> that can be used to enumerate windows. </returns>
  private static WNDENUMPROC GetWndEnumProc(ICollection<Win32Window> result) {
    return (hwnd, lParam) => {
      var title       = hwnd.GetWindowText();
      var className   = hwnd.GetClassName();
      var processName = hwnd.GetProcessName();
      var processID   = hwnd.GetProcessID();

      result.Add(
        new Win32Window {
          Hwnd        = hwnd,
          Title       = title,
          ClassName   = className,
          ProcessName = processName,
          ProcessID   = processID
        }
      );

      // Continue the enumeration.
      return true;
    };
  }
}
