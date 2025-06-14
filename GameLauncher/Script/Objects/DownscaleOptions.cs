using GameLauncher.Core.CodeGenAttributes;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

[TypeScriptExport]
public class DownscaleOptions : ObjectBase {
  /// <summary>
  ///   The X position of the top-left corner of the downscaler window. This will be relative
  ///   to the monitor that the window is on. If this is not specified, the window will be
  ///   positioned automatically.
  /// </summary>
  [ScriptMember("x")]
  public int? X { get; set; }

  /// <summary>
  ///   The Y position of the top-left corner of the downscaler window. This will be relative
  ///   to the monitor that the window is on. If this is not specified, the window will be
  ///   positioned automatically.
  /// </summary>
  [ScriptMember("y")]
  public int? Y { get; set; }

  /// <summary>
  ///   The factor to downscale the window by. For example, a factor of 2 will downscale the window
  ///   by 2x, meaning that a window of size 1920x1080 will be downscaled to 960x540. To upscale the
  ///   window, use a factor less than 1. For example, a factor of 0.5 will upscale the window by 2x,
  ///   meaning that a window of size 1920x1080 will be upscaled to 3840x2160.
  /// </summary>
  [ScriptMember("downscaleFactor")]
  public double? DownscaleFactor { get; set; }

  /// <summary>
  ///   The width to scale the window to. This is exclusive with <see cref="DownscaleFactor" />. If
  ///   this is specified, but height is not, the height will be scaled proportionally to maintain
  ///   the aspect ratio of the window.
  /// </summary>
  [ScriptMember("scaleWidth")]
  public int? ScaleWidth { get; set; }

  /// <summary>
  ///   The height to scale the window to. This is exclusive with <see cref="DownscaleFactor" />. If
  ///   this is specified, but width is not, the width will be scaled proportionally to maintain the
  ///   aspect ratio of the window.
  /// </summary>
  [ScriptMember("scaleHeight")]
  public int? ScaleHeight { get; set; }

  /// <summary>
  ///   A namespace where debug configurations can be specified.
  /// </summary>
  [ScriptMember("debug")]
  public DownscaleDebugOptions? Debug { get; set; }
}
