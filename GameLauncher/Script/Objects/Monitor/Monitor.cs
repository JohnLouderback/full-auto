using System.Text.RegularExpressions;
using Windows.Win32.Graphics.Gdi;
using Core.Models;
using Core.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a monitor (e.g. a display) connected to the system.
/// </summary>
[TypeScriptExport]
public partial class Monitor : ObjectBase {
  private readonly HMONITOR     hMonitor;
  private readonly Win32Monitor win32Monitor;

  /// <summary>
  ///   Represents the graphical display of a monitor. For example, the screen resolution, color
  ///   depth, refresh rate, and other display settings that determined in software.
  /// </summary>
  [ScriptMember("screen")]
  public Screen Screen { get; }

  /// <summary>
  ///   The handle of the monitor. This is a unique identifier representing the monitor.
  /// </summary>
  [ScriptMember("handle")]
  public int Handle => (int)hMonitor;

  /// <summary>
  ///   Indicates whether the monitor is the primary monitor. The primary monitor is the one that
  ///   is used to display the taskbar and the desktop. It is typically the monitor that fullscreen
  ///   applications are displayed on in the absence of a specified monitor.
  /// </summary>
  [ScriptMember("isPrimary")]
  public bool IsPrimary { get; }

  /// <summary>
  ///   The device ID of the monitor. The device ID is a unique identifier for the monitor. This
  ///   value is typically derived from the monitor's EDID and is not user-editable.
  ///   Example: <c>"ACME1234"</c>.
  /// </summary>
  [ScriptMember("deviceID")]
  public string DeviceID { get; }

  /// <summary>
  ///   The raw device ID of the monitor. The raw device ID is a unique identifier for the monitor.
  ///   This value is typically derived from the monitor's EDID and is not user-editable.
  ///   Example: <c>"MONITOR\ACME1234\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001"</c>.
  /// </summary>
  [ScriptMember("deviceIDRaw")]
  public string DeviceIDRaw => win32Monitor.DeviceId;

  /// <summary>
  ///   The device name for the monitor. This identifies the monitor as a display device in the
  ///   system. This value is based on the order the monitor was detected by the system and can be
  ///   influenced by which port the monitor is connected to.
  ///   Example: <c>"DISPLAY1"</c>.
  /// </summary>
  [ScriptMember("deviceName")]
  public string DeviceName { get; }

  /// <summary>
  ///   The device name for the monitor. This identifies the monitor as a display device in the
  ///   system. This value is based on the order the monitor was detected by the system and can be
  ///   influenced by which port the monitor is connected to.
  ///   Example: <c>"\\.\DISPLAY1\Monitor0"</c>.
  /// </summary>
  [ScriptMember("deviceNameRaw")]
  public string DeviceNameRaw => win32Monitor.DeviceName;

  /// <summary>
  ///   The device string for the monitor. The device name is a human-readable name for the monitor.
  ///   This value is influenced by monitor's EDID and can be changed by the user.
  ///   Example: <c>"Generic PnP Monitor"</c>.
  /// </summary>
  [ScriptMember("deviceString")]
  public string DeviceString { get; }

  /// <summary>
  ///   The device string for the monitor. The device name is a human-readable name for the monitor.
  ///   This value is influenced by monitor's EDID and can be changed by the user.
  ///   Example: <c>"Generic PnP Monitor(HDMI)"</c>.
  /// </summary>
  [ScriptMember("deviceStringRaw")]
  public string DeviceStringRaw => win32Monitor.DeviceString;

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
  [ScriptMember("deviceKey")]
  public string DeviceKey { get; }


  internal Monitor(Win32Monitor monitor) {
    win32Monitor = monitor;
    hMonitor     = monitor.HMonitor;

    // Get the short device name. For example "\\.\DISPLAY1\Monitor0" becomes "DISPLAY1".
    var deviceNameMatch = DeviceNameRegex().Match(monitor.DeviceName);

    if (deviceNameMatch.Success) {
      DeviceName = deviceNameMatch.Groups[1].Value;
    }
    else {
      throw new ArgumentException(
        "Device name does not match the expected pattern.",
        nameof(monitor)
      );
    }

    // Get the short device string. For example "Generic PnP Monitor(HDMI)" becomes
    // "Generic PnP Monitor".
    DeviceString = DeviceStringRegex().Replace(monitor.DeviceString, "");

    // Get the device ID. For example "MONITOR\ACME1234\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001"
    // becomes "ACME1234".
    var deviceIDMatch = DeviceIDRegex().Match(monitor.DeviceId);

    if (deviceIDMatch.Success) {
      DeviceID = deviceIDMatch.Groups[1].Value;
    }
    else {
      throw new ArgumentException(
        "Device ID does not match the expected pattern.",
        nameof(monitor)
      );
    }

    DeviceKey = monitor.DeviceKey;

    IsPrimary = monitor.IsPrimary;
    Screen    = new Screen(monitor);
  }


  /// <summary>
  ///   Sets this monitor as the primary monitor. The primary monitor is the one that is used to display
  ///   the taskbar and the desktop. It is typically the monitor that fullscreen applications are
  ///   displayed
  ///   on by default.
  /// </summary>
  /// <param name="shouldPersist">
  ///   Whether to persist the change to the registry. If <c>false</c>, the change is temporary and
  ///   will be reset when the script finishes executing. If <c>true</c>, the change is permanent
  ///   and will persist after the script finishes executing and across system restarts.
  /// </param>
  /// <returns>
  ///   The result of making the monitor primary. If you need to revert back to the previous primary
  ///   monitor, you can call the <see cref="MakeMonitorPrimaryResult.Undo" /> method on the result.
  /// </returns>
  [ScriptMember("makePrimary")]
  public MakeMonitorPrimaryResult MakePrimary(bool shouldPersist = false) {
    var previousPrimary = win32Monitor.SetAsPrimaryMonitor();

    return new MakeMonitorPrimaryResult(
      previousPrimary,
      !shouldPersist
    );
  }


  // Matches "AUS32B4" in strings like "MONITOR\AUS32B4\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001"
  // Used to extract the device ID from the monitor's device ID like "AUS32B4".
  [GeneratedRegex(@"MONITOR\\(.*?)\\\{")]
  private static partial Regex DeviceIDRegex();


  // Matches \Monitor0, \Monitor1, etc. at the end of the device name like "\\.\DISPLAY1\Monitor0".
  // Used to remove the monitor number from the device name so it becomes "DISPLAY1".
  [GeneratedRegex(@"\\\\.\\(.*?)\\Monitor\d+.*?$")]
  private static partial Regex DeviceNameRegex();


  // Matches (HDMI), (DisplayPort) etc. at the end of the device string like "Generic PnP Monitor (HDMI)"
  [GeneratedRegex(@"\(.*?\)?$")] private static partial Regex DeviceStringRegex();
}
