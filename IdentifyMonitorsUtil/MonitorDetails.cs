namespace IdentifyMonitorsUtil;

/// <summary>
///   A struct to hold the details of a monitor.
/// </summary>
public struct MonitorDetails {
  /// <summary>
  ///   The device ID for the monitor such as "\.\\DISPLAY1".
  /// </summary>
  public string DeviceName { get; init; }

  /// <summary>
  ///   The index for the monitor as determine by iterating through the screens.
  /// </summary>
  public int Index { get; init; }
}
