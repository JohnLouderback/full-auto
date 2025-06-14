using GameLauncher.Core.CodeGenAttributes;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

[TypeScriptExport]
public class DownscaleDebugOptions : ObjectBase {
  /// <summary>
  ///   If <c>true</c>, debug information will be displayed in the downscaler window.
  /// </summary>
  [ScriptMember("enabled")]
  public bool? Enabled { get; set; }

  /// <summary>
  ///   The font scaling factor to use for the debug information. A value of <c>1</c> means
  ///   the pixel font has a 1:1 ratio with the screen pixels. A value of <c>2</c>
  ///   means the pixel font has a 2:1 ratio with the screen pixels.
  /// </summary>
  [ScriptMember("fontScale")]
  public int? FontScale { get; set; }

  /// <summary>
  ///   Whether to show the frames per second (FPS) in the debug information.
  ///   The FPS represents the FPS of the downscaler window, not the source
  ///   window.
  /// </summary>
  [ScriptMember("showFPS")]
  public bool? ShowFps { get; set; }

  /// <summary>
  ///   Whether to show the mouse coordinates in the debug information. This
  ///   is useful for debugging that the mouse coordinates are being correctly
  ///   transformed from the downscaler window to the source window.
  /// </summary>
  [ScriptMember("showMouseCoordinates")]
  public bool? ShowMouseCoordinates { get; set; }

  /// <summary>
  ///   The font family to use for the debug information. The font family can be
  ///   one of the following:
  ///   <ul>
  ///     <li>
  ///       <c>extra-small</c>: Useful for small windows like 240p. (font: Pixel Rocks, 3px tall)
  ///     </li>
  ///     <li>
  ///       <c>small</c>: Useful for small windows like 240p. (font: Pixel Millennium, 5px tall)
  ///     </li>
  ///     <li>
  ///       <c>normal</c>: Useful for most windows. (font: Dogica Pixel, 7px tall)
  ///     </li>
  ///     <li>
  ///       <c>large</c>: Useful for larger windows like 1024x768 and up. (font: Fixedsys, 9px tall)
  ///     </li>
  ///   </ul>
  /// </summary>
  [ScriptMember("fontFamily")]
  [TsTypeOverride(""" "extra-small" | "small" | "normal" | "large" | null | undefined """)]
  public string? FontFamily { get; set; }
}
