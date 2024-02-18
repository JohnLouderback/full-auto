using System.Diagnostics.CodeAnalysis;
using Windows.Win32.Foundation;
using static DownscalerV3.Core.Utils.NativeUtils;

namespace DownscalerV3.Core.Models;

/// <summary>
///   Represents a window in the Windows operating system. This is a blittable version of the
///   <see cref="Win32Window" /> struct that can be used in native interop. "Blittable" means that the
///   struct can be used in native interop without any special marshalling because it has the same
///   memory layout as the native struct. All strings are fixed-size char arrays - essentially C
///   strings allowing up to 1024 characters or 1KB of text.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public unsafe struct Win32WindowBlittable {
  public const int StringLength = 1024;
  public HWND Hwnd;
  public fixed char Title[StringLength];
  public fixed char ClassName[StringLength];
  public fixed char ProcessName[StringLength];


  /// <summary>
  ///   Converts a <see cref="Win32WindowBlittable" /> to a <see cref="Win32Window" />.
  /// </summary>
  /// <param name="window"> The window to convert. </param>
  /// <returns> The converted window. </returns>
  public static explicit operator Win32Window(Win32WindowBlittable window) {
    return new Win32Window {
      Hwnd        = window.Hwnd,
      Title       = new string(window.Title).TrimEnd('\0'),
      ClassName   = new string(window.ClassName).TrimEnd('\0'),
      ProcessName = new string(window.ProcessName).TrimEnd('\0')
    };
  }
}

/// <summary>
///   Represents a window in the Windows operating system.
/// </summary>
public struct Win32Window {
  /// <summary>
  ///   The handle of the window. The handle is a unique identifier for the window.
  /// </summary>
  public HWND Hwnd { get; set; }

  /// <summary>
  ///   The title of the window. This is the text that is displayed in the title bar of the window.
  /// </summary>
  public string Title { get; set; }

  /// <summary>
  ///   The class name of the window. This is the name of the window class that the window belongs
  ///   to. These values are used to create the window and are set by the application that creates
  ///   the window. An example of a class name is "Chrome_WidgetWin_1".
  /// </summary>
  public string ClassName { get; set; }

  /// <summary>
  ///   The process name of the window. This is the name of the process that created the window.
  /// </summary>
  public string ProcessName { get; set; }


  public static unsafe explicit operator Win32WindowBlittable(Win32Window window) {
    var blittable = new Win32WindowBlittable();
    blittable.Hwnd = window.Hwnd;

    // Convert and copy the Title string
    CopyStringToFixedBuffer(window.Title, blittable.Title, Win32WindowBlittable.StringLength);

    // Convert and copy the ClassName string
    CopyStringToFixedBuffer(
      window.ClassName,
      blittable.ClassName,
      Win32WindowBlittable.StringLength
    );

    // Convert and copy the ProcessName string
    CopyStringToFixedBuffer(
      window.ProcessName,
      blittable.ProcessName,
      Win32WindowBlittable.StringLength
    );

    return blittable;
  }
}
