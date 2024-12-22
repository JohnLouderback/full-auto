namespace IdentifyMonitorsUtil.Contracts.Models;

/// <summary>
///   Abstractly represents a monitor on the user's system.
/// </summary>
public interface IMonitor {
  /// <summary>
  ///   The device name for the monitor. This identifies the monitor as a display device in the
  ///   system. This value is based on the order the monitor was detected by the system and can be
  ///   influenced by which port the monitor is connected to.
  ///   Example: <c>"\\.\DISPLAY1"</c>.
  /// </summary>
  string DeviceName { get; }

  /// <summary>
  ///   The device string for the monitor. The device name is a human-readable name for the monitor.
  ///   This value is influenced by monitor's EDID and can be changed by the user.
  ///   Example: <c>"Generic PnP Monitor"</c>.
  /// </summary>
  string DeviceString { get; }

  /// <summary>
  ///   The device ID of the monitor. The device ID is a unique identifier for the monitor. This
  ///   value is typically derived from the monitor's EDID and is not user-editable.
  ///   Example: <c>"MONITOR\ACME1234\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001"</c>.
  /// </summary>
  string DeviceID { get; }
}
