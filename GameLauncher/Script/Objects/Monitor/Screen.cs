using System.Runtime.InteropServices;
using Windows.Win32.Graphics.Gdi;
using Core.Models;
using Core.Utils;
using GameLauncher.Script.Utils;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents the graphical display of a monitor. For example, the screen resolution, color depth,
///   refresh rate, and other display settings that determined in software.
/// </summary>
[TypeScriptExport]
public class Screen : ObjectBase {
  private readonly HMONITOR     hMonitor;
  private readonly Win32Monitor win32Monitor;


  internal Screen(Win32Monitor monitor) {
    win32Monitor = monitor;
    hMonitor     = monitor.HMonitor;
  }


  /// <summary>
  ///   <para>
  ///     Gets the current display mode of the monitor. The display mode includes the screen resolution,
  ///     color depth, and refresh rate.
  ///   </para>
  ///   <para>
  ///     The value returned is a "snapshot" of the current display mode at the time of calling this
  ///     method. If the display mode changes after this method is called, you will need to call this
  ///     method again to get the updated display mode.
  ///   </para>
  /// </summary>
  /// <returns> The current display mode of the monitor. </returns>
  [ScriptMember("currentDisplayMode")]
  public DisplayMode GetCurrentDisplayMode() {
    var displayMode = win32Monitor.GetCurrentDisplayMode();
    return new DisplayMode {
      Width        = (int)displayMode.dmPelsWidth,
      Height       = (int)displayMode.dmPelsHeight,
      ColorDepth   = (int)displayMode.dmBitsPerPel,
      RefreshRate  = (int)displayMode.dmDisplayFrequency,
      IsInterlaced = displayMode.IsInterlaced()
    };
  }


  /// <summary>
  ///   Gets the available display modes for the monitor. The display modes include available
  ///   combinations of screen resolutions, color depths, and refresh rates supported by the monitor.
  /// </summary>
  /// <returns> The available display modes for the monitor. </returns>
  [ScriptMember("listDisplayModes")]
  public IEnumerable<DisplayMode> ListDisplayModes() {
    return JSArray<Window>.FromIEnumerable(
      win32Monitor.GetAvailableDisplayModes()
        .Select(
          displayMode => new DisplayMode {
            Width        = (int)displayMode.dmPelsWidth,
            Height       = (int)displayMode.dmPelsHeight,
            ColorDepth   = (int)displayMode.dmBitsPerPel,
            RefreshRate  = (int)displayMode.dmDisplayFrequency,
            IsInterlaced = displayMode.IsInterlaced()
          }
        )
    );
  }


  /// <summary>
  ///   Sets the display mode of the monitor to the specified display mode. The display mode includes
  ///   the screen resolution, color depth, and refresh rate. The display mode can be set temporarily
  ///   or permanently. Temporary display modes are not saved to the registry and are reset when the
  ///   script finishes executing. Permanent display modes are saved to the registry and persist
  ///   after the script finishes executing and across system restarts.
  /// </summary>
  /// <param name="displayMode"> The display mode to set the monitor to. </param>
  /// <param name="shouldPersist">
  ///   Whether the display mode should remain after the script finishes executing.
  /// </param>
  /// <returns>
  ///   The result of changing the display mode. If you need to revert the display mode back to the
  ///   original display mode, you can call the <see cref="ChangeDisplayModeResult.Undo" /> method on
  ///   the result.
  /// </returns>
  [ScriptMember("setDisplayMode")]
  public ChangeDisplayModeResult SetDisplayMode(
    DisplayMode displayMode,
    bool shouldPersist = false
  ) {
    var dm = new DEVMODEW();
    dm.dmSize             = (ushort)Marshal.SizeOf(typeof(DEVMODEW));
    dm.dmPelsWidth        = (uint)displayMode.Width;
    dm.dmPelsHeight       = (uint)displayMode.Height;
    dm.dmBitsPerPel       = (uint)displayMode.ColorDepth;
    dm.dmDisplayFrequency = (uint)displayMode.RefreshRate;

    // If the display mode is interlaced, set the interlaced flag.
    if (displayMode.IsInterlaced) {
      dm.dmFields |= DEVMODE_FIELD_FLAGS.DM_INTERLACED;
    }

    // We need to store the current display mode before changing it so we can revert back to it
    // later.
    var currentDisplayMode = GetCurrentDisplayMode();

    // Set the display mode to the specified display mode and as temporary. Temporary display modes
    // are not saved to the registry and are reset when the system is restarted.
    win32Monitor.SetDisplayModeOrThrow(dm, shouldPersist ? CDS_TYPE.CDS_UPDATEREGISTRY : 0);

    return new ChangeDisplayModeResult(win32Monitor, currentDisplayMode, shouldPersist);
  }


  /// <summary>
  ///   Sets the display mode of the monitor to the specified display mode. The display mode includes
  ///   the screen resolution, color depth, and refresh rate. The display mode can be set temporarily
  ///   or permanently. Temporary display modes are not saved to the registry and are reset when the
  ///   script finishes executing. Permanent display modes are saved to the registry and persist
  ///   after the script finishes executing and across system restarts.
  /// </summary>
  /// <param name="displayMode">
  ///   The display mode to set the monitor to. You must provide a width and height for the display
  ///   mode. The color depth and refresh rate are optional and will default to the current display
  ///   mode if not provided. The display mode can also be set to interlaced if desired and the
  ///   display supports it. Finally, you can specify whether the display mode should persist after
  ///   the script finishes executing.
  /// </param>
  /// <returns>
  ///   The result of changing the display mode. If you need to revert the display mode back to the
  ///   original display mode, you can call the <see cref="ChangeDisplayModeResult.Undo" /> method on
  ///   the result.
  /// </returns>
  /// <exception cref="ArgumentException">
  ///   Thrown if the display mode does not have a width and/or height.
  /// </exception>
  [ScriptMember("setDisplayMode")]
  public ChangeDisplayModeResult SetDisplayMode(
    [TsTypeOverride(
      "{ width: number, height: number, /** Whether the display mode should persist after the script finishes executing. */ shouldPersist?: boolean } & Partial<DisplayMode>"
    )]
    ScriptObject displayMode
  ) {
    if (!displayMode.HasProperty("width") ||
        !displayMode.HasProperty("height")) {
      throw new ArgumentException("The display mode must have a width and height.");
    }

    var currentDisplayMode = GetCurrentDisplayMode();

    var width  = displayMode.GetProperty<int>("width");
    var height = displayMode.GetProperty<int>("height");

    var colorDepth =
      displayMode.GetProperty<int?>("colorDepth") ?? currentDisplayMode.ColorDepth;
    var refreshRate =
      displayMode.GetProperty<int?>("refreshRate") ?? currentDisplayMode.RefreshRate;

    var isInterlaced = displayMode.GetProperty<bool?>("isInterlaced") ?? false;

    var shouldPersist = displayMode.GetProperty<bool?>("shouldPersist") ?? false;

    return SetDisplayMode(
      new DisplayMode {
        Width        = width,
        Height       = height,
        ColorDepth   = colorDepth,
        RefreshRate  = refreshRate,
        IsInterlaced = isInterlaced
      },
      shouldPersist
    );
  }


  /// <summary>
  ///   <para>
  ///     Sets the display mode of the monitor to the specified display mode. The display mode includes
  ///     the screen resolution, color depth, and refresh rate. The display mode can be set temporarily
  ///     or permanently. Temporary display modes are not saved to the registry and are reset when the
  ///     script finishes executing. Permanent display modes are saved to the registry and persist
  ///     after the script finishes executing and across system restarts.
  ///   </para>
  ///   <para>
  ///     The values passed to this method may not be arbitrary. The display mode must be supported by
  ///     the monitor. If the display mode is not supported, the system will not apply the change and
  ///     an exception will be thrown. You can query the available display modes using the
  ///     <see cref="ListDisplayModes" /> method.
  ///   </para>
  /// </summary>
  /// <param name="width"> The width of the display mode in device pixels. </param>
  /// <param name="height"> The height of the display mode in device pixels. </param>
  /// <param name="refreshRate"> The refresh rate of the display mode in Hz. </param>
  /// <param name="colorDepth"> The color depth of the display mode in bits per pixel. </param>
  /// <param name="shouldPersist">
  ///   Whether the display mode should remain after the script finishes
  ///   executing.
  /// </param>
  /// <returns>
  ///   The result of changing the display mode. If you need to revert the display mode back to the
  ///   original display mode, you can call the <see cref="ChangeDisplayModeResult.Undo" /> method on
  ///   the result.
  /// </returns>
  [ScriptMember("setDisplayMode")]
  public ChangeDisplayModeResult SetDisplayMode(
    int width,
    int height,
    int? refreshRate = null,
    int? colorDepth = null,
    bool shouldPersist = false
  ) {
    var currentDisplayMode = GetCurrentDisplayMode();

    return SetDisplayMode(
      new DisplayMode {
        Width        = width,
        Height       = height,
        ColorDepth   = colorDepth ?? currentDisplayMode.ColorDepth,
        RefreshRate  = refreshRate ?? currentDisplayMode.RefreshRate,
        IsInterlaced = false
      },
      shouldPersist
    );
  }
}
