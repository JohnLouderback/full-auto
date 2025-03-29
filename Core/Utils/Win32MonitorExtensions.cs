using System.Runtime.InteropServices;
using Windows.Win32.Graphics.Gdi;
using Core.Models;
using static Windows.Win32.PInvoke;
using static Core.Utils.Macros;

namespace Core.Utils;

public static class Win32MonitorExtensions {
  /// <summary>
  ///   Gets the available display modes for the monitor including the screen resolution, color depth,
  ///   and refresh rate.
  /// </summary>
  /// <param name="monitor"> The monitor to get the available display modes of. </param>
  /// <returns> The available display modes for the monitor. </returns>
  public static IEnumerable<DEVMODEW> GetAvailableDisplayModes(this Win32Monitor monitor) {
    var deviceName = monitor.GetDeviceName();

    var dm = new DEVMODEW();
    dm.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODEW));

    var modeIndex = 0;
    while (EnumDisplaySettings(deviceName, (ENUM_DISPLAY_SETTINGS_MODE)modeIndex, ref dm)) {
      yield return dm;
      modeIndex++;
    }
  }


  /// <summary>
  ///   Gets the current display mode for the monitor including the screen resolution, color depth,
  ///   and refresh rate.
  /// </summary>
  /// <param name="monitor"> The monitor to get the current display mode of. </param>
  /// <returns> The current display mode for the monitor. </returns>
  /// <exception cref="InvalidOperationException"> The current display mode could not be retrieved. </exception>
  public static DEVMODEW GetCurrentDisplayMode(this Win32Monitor monitor) {
    var deviceName = monitor.GetDeviceName();

    var dm = new DEVMODEW();
    dm.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODEW));

    if (!EnumDisplaySettings(
          deviceName,
          ENUM_DISPLAY_SETTINGS_MODE.ENUM_CURRENT_SETTINGS,
          ref dm
        )) {
      throw new InvalidOperationException($"Unable to retrieve current settings for {deviceName}");
    }

    return dm;
  }


  /// <summary>
  ///   Sets the display mode for the monitor. This will throw if the specified mode is not valid for
  ///   the monitor or if the system fails to apply the change.
  /// </summary>
  /// <param name="monitor">The monitor to update.</param>
  /// <param name="mode">The desired display mode.</param>
  /// <param name="cdsType">
  ///   <para>
  ///     The type of change to apply. This can be used to test the change without actually applying
  ///     it or to update the registry with the new settings, among other options.
  ///   </para>
  ///   <para>
  ///     See:
  ///     <see
  ///       href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-changedisplaysettingsexa" />
  ///   </para>
  /// </param>
  /// <exception cref="InvalidOperationException">Thrown if the mode is not valid or could not be set.</exception>
  public static unsafe void SetDisplayModeOrThrow(
    this Win32Monitor monitor,
    DEVMODEW mode,
    CDS_TYPE cdsType = CDS_TYPE.CDS_UPDATEREGISTRY
  ) {
    var deviceName = monitor.GetDeviceName();

    // Ensure the mode sets the correct fields to be updated.
    mode.dmFields |= DEVMODE_FIELD_FLAGS.DM_PELSHEIGHT |
                     DEVMODE_FIELD_FLAGS.DM_PELSWIDTH |
                     DEVMODE_FIELD_FLAGS.DM_BITSPERPEL |
                     DEVMODE_FIELD_FLAGS.DM_DISPLAYFREQUENCY;

    // Ensure the mode is one of the supported options
    var validModes = monitor.GetAvailableDisplayModes();
    var isValid = validModes.Any(
      m =>
        m.dmPelsWidth == mode.dmPelsWidth &&
        m.dmPelsHeight == mode.dmPelsHeight &&
        m.dmBitsPerPel == mode.dmBitsPerPel &&
        m.dmDisplayFrequency == mode.dmDisplayFrequency
    );

    if (!isValid) {
      throw new InvalidOperationException(
        $"Display mode {mode.dmPelsWidth}x{mode.dmPelsHeight} @ {mode.dmDisplayFrequency}Hz " +
        $"{mode.dmBitsPerPel}-bit is not valid for {deviceName}."
      );
    }

    var result = ChangeDisplaySettingsEx(
      deviceName,
      mode,
      cdsType,
      (void*)NULL
    );

    if (result != DISP_CHANGE.DISP_CHANGE_SUCCESSFUL) {
      throw new InvalidOperationException(
        $"Failed to apply mode {
          mode.dmPelsWidth
        }x{
          mode.dmPelsHeight
        } @ {
          mode.dmDisplayFrequency
        }Hz " +
        $"({mode.dmBitsPerPel}-bit) to {deviceName}. Result: {result}"
      );
    }
  }


  /// <summary>
  ///   Gets a cleaned version of the device name. The device name is formatted as
  ///   "\.\\DISPLAY1\Monitor0"
  ///   and the EnumDisplaySettings function expects only the "\.\\DISPLAY1" part.
  /// </summary>
  /// <param name="monitor"> The monitor to get the device name of. </param>
  /// <returns> The cleaned device name. </returns>
  private static string GetDeviceName(this Win32Monitor monitor) {
    var deviceName = monitor.DeviceName;

    // Slice off "\Monitor0" from the device name. This is necessary because the device name is
    // formatted as "DISPLAY1\Monitor0" and the EnumDisplaySettings function expects only the
    // "DISPLAY1" part.
    var index = deviceName.IndexOf("\\Monitor", StringComparison.Ordinal);
    if (index != -1) {
      deviceName = deviceName.Substring(0, index);
    }

    return deviceName;
  }
}
