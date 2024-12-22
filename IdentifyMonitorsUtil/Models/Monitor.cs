using System.Text.RegularExpressions;
using BrightIdeasSoftware;
using Core.Models;
using IdentifyMonitorsUtil.Contracts.Models;

namespace IdentifyMonitorsUtil.Models;

/// <inheritdoc />
public partial class Monitor : IMonitor {
  /// <inheritdoc />
  public string DeviceName { get; }

  /// <inheritdoc />
  public string DeviceString { get; }

  /// <inheritdoc />
  public string DeviceID { get; }

  /// <summary>
  ///   Indicates whether the monitor is the primary monitor.
  /// </summary>
  [OLVColumn(
    Title = "Primary",
    DisplayIndex = 0,
    IsEditable = false,
    CheckBoxes = true,
    AspectToStringFormat = "",
    TextAlign = HorizontalAlignment.Center
  )]
  public bool IsPrimary { get; set; }


  public Monitor(Win32Monitor monitor) {
    DeviceName   = DeviceNameRegex().Replace(monitor.DeviceName, "");
    DeviceString = DeviceStringRegex().Replace(monitor.DeviceString, "");

    var match = DeviceIDRegex().Match(monitor.DeviceId);

    if (match.Success) {
      DeviceID = match.Groups[1].Value;
    }
    else {
      throw new ArgumentException(
        "Device ID does not match the expected pattern.",
        nameof(monitor)
      );
    }

    IsPrimary = monitor.IsPrimary;
  }


  // Matches "AUS32B4" in strings like "MONITOR\AUS32B4\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001"
  [GeneratedRegex(@"MONITOR\\(.*?)\\\{")]
  private static partial Regex DeviceIDRegex();


  [GeneratedRegex(@"\\Monitor\d+.*?$")] // Matches \Monitor0, \Monitor1, etc.
  private static partial Regex DeviceNameRegex();


  // Matches (HDMI), (DisplayPort) etc. at the end of the device string like "Generic PnP Monitor (HDMI)"
  [GeneratedRegex(@"\(.*?\)?$")] private static partial Regex DeviceStringRegex();
}
