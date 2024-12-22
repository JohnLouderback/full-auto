using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Core.Models;

/// <summary>
///   Represents a monitor in the Windows operating system.
/// </summary>
public struct Win32Monitor {
  /// <summary>
  ///   The handle of the monitor. The handle is a unique identifier for the monitor.
  /// </summary>
  public required HMONITOR HMonitor { get; init; }

  /// <summary>
  ///   The device ID of the monitor. The device ID is a unique identifier for the monitor. This
  ///   value is typically derived from the monitor's EDID and is not user-editable.
  ///   Example: <c>"MONITOR\ACME1234\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001"</c>.
  /// </summary>
  public required string DeviceId { get; init; }

  /// <summary>
  ///   The device name for the monitor. This identifies the monitor as a display device in the
  ///   system. This value is based on the order the monitor was detected by the system and can be
  ///   influenced by which port the monitor is connected to.
  ///   Example: <c>"\\.\DISPLAY1"</c>.
  /// </summary>
  public required string DeviceName { get; init; }

  /// <summary>
  ///   The device string for the monitor. The device name is a human-readable name for the monitor.
  ///   This value is influenced by monitor's EDID and can be changed by the user.
  ///   Example: <c>"Generic PnP Monitor"</c>.
  /// </summary>
  public required string DeviceString { get; init; }

  /// <summary>
  ///   The device key for the monitor. The device key is a unique identifier for the monitor. This
  ///   value is the registry key for the monitor in the system.
  ///   Example:
  ///   <c>
  ///     "\REGISTRY\MACHINE\SYSTEM\ControlSet001\Enum\DISPLAY\ACME1234\4&amp;12345678&amp;0&amp;
  ///     \UID123456"
  ///   </c>
  ///   .
  /// </summary>
  public required string DeviceKey { get; init; }

  /// <summary>
  ///   The monitor rectangle. The rectangle is the bounding box of the monitor in screen
  ///   coordinates. For example, a monitor with a resolution of 1920x1080 pixels has a rectangle
  ///   from (0, 0) to (1919, 1079).
  /// </summary>
  public required RECT MonitorRect { get; init; }

  /// <summary>
  ///   The work area rectangle. The work area is the bounding box of the monitor excluding the
  ///   taskbar and other docked windows. For example, a monitor with a resolution of 1920x1080
  ///   pixels and a taskbar of 40 pixels has a work area from (0, 0) to (1919, 1039).
  /// </summary>
  public required RECT WorkArea { get; init; }

  /// <summary>
  ///   The DPI of the monitor. The DPI is the number of dots that fit into a linear inch. The
  ///   DPI of a monitor with a 1:1 pixel density is 96. For 150% scaling, the DPI is 144.
  /// </summary>
  public required uint Dpi { get; init; }

  /// <summary>
  ///   Indicates whether the monitor is the primary monitor in the system. This is a user-defined
  ///   setting in the system settings.
  /// </summary>
  public required bool IsPrimary { get; init; }
}
