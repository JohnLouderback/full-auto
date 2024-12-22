using System.ComponentModel;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;
using static Windows.Win32.PInvoke;
using static Core.Utils.Macros;

namespace Core.Utils;

public static class HMonitorExtensions {
  public static unsafe DISPLAY_DEVICEW GetDisplayDeviceById(
    this HMONITOR monitor,
    string deviceId
  ) {
    var device = new DISPLAY_DEVICEW {
      cb = (uint)sizeof(DISPLAY_DEVICEW)
    };

    var deviceIdPCWSTR = deviceId.ToPCWSTR();

    if (EnumDisplayDevices(deviceIdPCWSTR, 0, &device, 0)) {
      return device;
    }

    throw new Win32Exception(
      $"Failed to get the display device for the monitor with handle \"{monitor}\"."
    );
  }


  /// <summary>
  ///   Gets the DPI of the monitor. The DPI is the number of dots that fit into a linear inch. The
  ///   DPI of a monitor with a 1:1 pixel density is 96. For 150% scaling, the DPI is 144.
  /// </summary>
  /// <param name="monitor"> The monitor to get the DPI of. </param>
  /// <returns> The DPI of the monitor. </returns>
  /// <exception cref="Exception"> The DPI could not be retrieved. </exception>
  public static uint GetDpi(this HMONITOR monitor) {
    if (SUCCEEDED(
          GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out _)
        )) {
      return dpiX;
    }

    throw new Win32Exception($"Failed to get the DPI for the monitor with handle \"{monitor}\".");
  }


  /// <summary>
  ///   Gets the extended monitor information for the monitor. The monitor information includes the
  ///   monitor's rectangle, work area, and device name.
  /// </summary>
  /// <param name="monitor"> The monitor to get the information for. </param>
  /// <returns> The extended monitor information for the monitor. </returns>
  public static unsafe MONITORINFOEXW GetMonitorInfoEx(this HMONITOR monitor) {
    var info = new MONITORINFOEXW {
      monitorInfo = {
        cbSize = (uint)sizeof(MONITORINFOEXW)
      },
      szDevice = default
    };

    if (GetMonitorInfo(monitor, (MONITORINFO*)&info)) {
      return info;
    }

    throw new Win32Exception(
      $"Failed to get the monitor information for the monitor with handle \"{monitor}\"."
    );
  }
}
