using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;
using static Core.Utils.StringUtils;

namespace GameLauncher.Script.Objects;

public partial class Window {
  /// <summary>
  ///   <para>
  ///     Creates a new window that acts as a "mirror" of the current window. The new window will be
  ///     created using the passed configuration. This is useful for creating a new window that
  ///     is a scaled version of the current window. Handy for when a window is resistant to being
  ///     scaled down through other means, such as resizing or cannot be resized beyond a certain
  ///     size.
  ///   </para>
  ///   <para>
  ///     A compelling use-case for this is if you wanted to play a contemporary pixel art game at
  ///     a low resolution on a CRT TV. Imagine that the game utilizes 240p pixel art, but the game is
  ///     locked to a higher resolution. You could use this to create a new window that is a scaled
  ///     version of the current window but at an actual 240p resolution.
  ///   </para>
  /// </summary>
  /// <param name="downscaleFactor">
  ///   The factor to downscale the window by. For example, a factor of 2 will downscale the window
  ///   by 2x, meaning that a window of size 1920x1080 will be downscaled to 960x540. To upscale the
  ///   window, use a factor less than 1. For example, a factor of 0.5 will upscale the window by 2x,
  ///   meaning that a window of size 1920x1080 will be upscaled to 3840x2160.
  /// </param>
  /// <returns> A reference to the downscaler window. </returns>
  [ScriptMember(Name = "downscale")]
  public async Task<Window> Downscale(double downscaleFactor) {
    return await Downscale(
             new DownscaleOptions {
               DownscaleFactor = downscaleFactor
             }
           );
  }


  /// <summary>
  ///   <para>
  ///     Creates a new window that acts as a "mirror" of the current window. The new window will be
  ///     created using the passed configuration. This is useful for creating a new window that
  ///     is a scaled version of the current window. Handy for when a window is resistant to being
  ///     scaled down through other means, such as resizing or cannot be resized beyond a certain
  ///     size.
  ///   </para>
  ///   <para>
  ///     A compelling use-case for this is if you wanted to play a contemporary pixel art game at
  ///     a low resolution on a CRT TV. Imagine that the game utilizes 240p pixel art, but the game is
  ///     locked to a higher resolution. You could use this to create a new window that is a scaled
  ///     version of the current window but at an actual 240p resolution.
  ///   </para>
  /// </summary>
  /// <param name="width">
  ///   The width to scale the window to. The width is in device pixels, meaning that it will not
  ///   be scaled by the DPI of the monitor.
  /// </param>
  /// <param name="height">
  ///   The height to scale the window to. The height is in device pixels, meaning that it will
  ///   not be scaled by the DPI of the monitor.
  /// </param>
  /// <returns> A reference to the downscaler window. </returns>
  [ScriptMember(Name = "downscale")]
  public async Task<Window> Downscale(int width, int height) {
    return await Downscale(
             new DownscaleOptions {
               ScaleWidth  = width,
               ScaleHeight = height
             }
           );
  }


  /// <summary>
  ///   <para>
  ///     Creates a new window that acts as a "mirror" of the current window. The new window will be
  ///     created using the passed configuration. This is useful for creating a new window that
  ///     is a scaled version of the current window. Handy for when a window is resistant to being
  ///     scaled down through other means, such as resizing or cannot be resized beyond a certain
  ///     size.
  ///   </para>
  ///   <para>
  ///     A compelling use-case for this is if you wanted to play a contemporary pixel art game at
  ///     a low resolution on a CRT TV. Imagine that the game utilizes 240p pixel art, but the game is
  ///     locked to a higher resolution. You could use this to create a new window that is a scaled
  ///     version of the current window but at an actual 240p resolution.
  ///   </para>
  /// </summary>
  /// <param name="box">
  ///   The bounding box to scale the window to. The bounding box is in device pixels, meaning
  ///   that it will not be scaled by the DPI of the monitor. The bounding box is in screen
  ///   coordinates, meaning that the X and Y coordinates are relative to the top-left corner of the
  ///   screen.
  ///   <returns> A reference to the downscaler window. </returns>
  [ScriptMember(Name = "downscale")]
  public async Task<Window> Downscale(BoundingBox box) {
    return await Downscale(
             new DownscaleOptions {
               X           = box.X,
               Y           = box.Y,
               ScaleWidth  = box.Width,
               ScaleHeight = box.Height
             }
           );
  }


  [HideFromTypeScript]
  public async Task<Window> Downscale(ScriptObject obj) {
    if (obj is null) {
      throw new ArgumentNullException(nameof(obj));
    }

    var width  = obj.GetProperty<int?>("width");
    var height = obj.GetProperty<int?>("height");
    var x      = obj.GetProperty<int?>("x");
    var y      = obj.GetProperty<int?>("y");

    // If width, height, x, and y are set, we were passed a bounding box.
    if (width is {} iwidth &&
        height is {} iheight &&
        x is {} ix &&
        y is {} iy) {
      return await Downscale(
               new BoundingBox {
                 X      = ix,
                 Y      = iy,
                 Width  = iwidth,
                 Height = iheight
               }
             );
    }

    var downscaleDebugOptions = obj.GetProperty<double?>("debug") is not null
                                  ? new DownscaleDebugOptions {
                                    Enabled   = obj.GetProperty<bool?>("debugEnabled"),
                                    FontScale = obj.GetProperty<int?>("debugFontScale"),
                                    ShowFps   = obj.GetProperty<bool?>("debugShowFps"),
                                    ShowMouseCoordinates =
                                      obj.GetProperty<bool?>("debugShowMouseCoordinates"),
                                    FontFamily = obj.GetProperty<string>("debugFontFamily")
                                  }
                                  : null;

    var downscaleOptions = new DownscaleOptions {
      X               = x,
      Y               = y,
      DownscaleFactor = obj.GetProperty<double?>("downscaleFactor"),
      ScaleWidth      = obj.GetProperty<int?>("scaleWidth"),
      ScaleHeight     = obj.GetProperty<int?>("scaleHeight"),
      Debug           = downscaleDebugOptions
    };

    return await Downscale(downscaleOptions);
  }


  /// <summary>
  ///   <para>
  ///     Creates a new window that acts as a "mirror" of the current window. The new window will be
  ///     created using the passed configuration. This is useful for creating a new window that
  ///     is a scaled version of the current window. Handy for when a window is resistant to being
  ///     scaled down through other means, such as resizing or cannot be resized beyond a certain
  ///     size.
  ///   </para>
  ///   <para>
  ///     A compelling use-case for this is if you wanted to play a contemporary pixel art game at
  ///     a low resolution on a CRT TV. Imagine that the game utilizes 240p pixel art, but the game is
  ///     locked to a higher resolution. You could use this to create a new window that is a scaled
  ///     version of the current window but at an actual 240p resolution.
  ///   </para>
  /// </summary>
  /// <param name="options">
  ///   The options to use when creating the downscaler window. This includes the position of the
  ///   window, the downscale factor, and the width and height to scale to. The downscale factor is
  ///   the factor to downscale the window by. For example, a factor of 2 will downscale the window
  ///   by 2x, meaning that a window of size 1920x1080 will be downscaled to 960x540. To upscale the
  ///   window, use a factor less than 1. For example, a factor of 0.5 will upscale the window by 2x,
  ///   meaning that a window of size 1920x1080 will be upscaled to 3840x2160.
  /// </param>
  /// <returns> A reference to the downscaler window. </returns>
  [ScriptMember(Name = "downscale")]
  public async Task<Window> Downscale(DownscaleOptions options) {
    var downscalerPath = GetDownscalerAppPath();
    var downscalerApp = Tasks.Launch(
      downscalerPath,
      [
        "yaml",
        DownscaleOptionsToYaml(options)
      ]
    );

    if (downscalerApp is null) {
      throw new Exception("Failed to launch Downscaler application.");
    }

    // Get or await the downscaler window. This is the window that will be created by the
    // Downscaler application. It will be a new window that is a scaled version of the current
    // window.
    var downscalerWindow = await Tasks.FindOrAwaitWindow(
                             (window, process) =>
                               window.ClassName is "WinUIDesktopWin32WindowClass" &&
                               process.Pid == downscalerApp.Process.Pid,
                             timeout: 10000
                           );

    if (!downscalerWindow.Any()) {
      throw new Exception("Failed to find Downscaler window.");
    }

    return downscalerWindow.First();
  }


  private string DownscaleOptionsToYaml(DownscaleOptions options) {
    var debugOptionsYaml = options.Debug?.Enabled is true
                             ? $$"""
                               debug: 
                                 {{
                                   (options.Debug.Enabled is true ? "enabled: true" : string.Empty)
                                 }}
                                 {{
                                   (options.Debug.FontScale is not null
                                      ? $"font-scale: {options.Debug.FontScale}"
                                      : string.Empty)
                                 }}
                                 {{
                                   (options.Debug.ShowFps is not null
                                      ? $"show-fps: {options.Debug.ShowFps}"
                                      : string.Empty)
                                 }}
                                 {{
                                   (options.Debug.ShowMouseCoordinates is not null
                                      ? $"show-mouse-coordinates: {
                                        options.Debug.ShowMouseCoordinates
                                      }"
                                      : string.Empty)
                                 }}
                                 {{
                                   (!string.IsNullOrEmpty(options.Debug.FontFamily)
                                      ? $"font-family: {options.Debug.FontFamily}"
                                      : string.Empty)
                                 }}
                             """
                             : string.Empty;

    var optionsYaml = CollapseConsecutiveNewlines(
      $$"""
      hwnd: {{
        hwnd
      }}
      {{
        (options.X is not null ? $"x: {options.X}" : string.Empty)
      }}
      {{
        (options.Y is not null ? $"y: {options.Y}" : string.Empty)
      }}
      {{
        (options.DownscaleFactor is not null
           ? $"downscale-factor: {options.DownscaleFactor}"
           : string.Empty)
      }}
      {{
        (options.ScaleWidth is not null ? $"scale-width: {options.ScaleWidth}" : string.Empty)
      }}
      {{
        (options.ScaleHeight is not null
           ? $"scale-height: {options.ScaleHeight}"
           : string.Empty)
      }}
      {{
        (!string.IsNullOrEmpty(debugOptionsYaml) ? debugOptionsYaml : string.Empty)
      }}
      """,
      includeWhitespaceBetweenNewlines: true
    );
    return optionsYaml;
  }


  private string GetDownscalerAppPath() {
    // First, check if the `Downscaler.exe` exists in the same directory as the current
    // executable. In a published build, this will be the same directory as the executable.
    var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
    var downscalerPath   = Path.Combine(currentDirectory, "Downscaler.exe");
    if (System.IO.File.Exists(downscalerPath)) {
      return downscalerPath;
    }

#if DEBUG
    // If not, check the directory it would be in, in a development build.
    var downscalerDevPath = Path.GetFullPath(
      Path.Combine(
        currentDirectory,
        "..",
        "..",
        "..",
        "..",
        "Downscaler",
        "bin",
        "x64",
        "Debug",
        "net8.0-windows10.0.22621.0",
        "win10-x64",
        "Downscaler.exe"
      )
    );
    if (System.IO.File.Exists(downscalerDevPath)) {
      return downscalerDevPath;
    }
#else
    // If not, check the directory it would be in, in a release build.
    var downscalerRelPath = Path.GetFullPath(
      Path.Combine(
        currentDirectory,
        "..",
        "..",
        "..",
        "..",
        "Downscaler",
        "bin",
        "x64",
        "Release",
        "net8.0-windows10.0.22621.0",
        "win10-x64",
        "Downscaler.exe"
      )
    );
    if (File.Exists(downscalerRelPath)) {
      return downscalerRelPath;
    }
#endif

    // If the file is not found in either location, throw an exception.
    throw new FileNotFoundException("Downscaler.exe not found in the expected locations.");
  }
}
