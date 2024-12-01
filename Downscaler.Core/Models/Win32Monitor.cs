using Windows.Win32.Graphics.Gdi;
using Downscaler.Core.Utils;

namespace Downscaler.Core.Models;

/// <summary>
///   Represents a monitor in the Windows operating system.
/// </summary>
public struct Win32Monitor {
  private uint? dpi;

  /// <summary>
  ///   The handle of the monitor. The handle is a unique identifier for the monitor.
  /// </summary>
  public required HMONITOR HMonitor { get; init; }


  /// <summary>
  ///   Gets the DPI of the monitor. The DPI is the number of dots that fit into a linear inch. The
  ///   DPI of a monitor with a 1:1 pixel density is 96. For 150% scaling, the DPI is 144.
  /// </summary>
  /// <returns> The DPI of the monitor. </returns>
  public uint GetDpi() {
    return dpi ??= HMonitor.GetDpi();
  }
}
