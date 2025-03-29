using GameLauncherTaskGenerator;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

[TypeScriptExport]
public class DisplayMode : ObjectBase {
  /// <summary>
  ///   The width of the display mode in device pixels.
  /// </summary>
  [ScriptMember("width")]
  public required int Width { get; init; }

  /// <summary>
  ///   The height of the display mode in device pixels.
  /// </summary>
  [ScriptMember("height")]
  public required int Height { get; init; }

  /// <summary>
  ///   The number of bits used to store the color of each pixel in this display mode.
  ///   This includes all color information and may also include extra bits such as transparency (alpha).
  ///   <para>
  ///     For example:
  ///     <list type="bullet">
  ///       <item>
  ///         <description><c>8</c> – 256 colors (palette-based).</description>
  ///       </item>
  ///       <item>
  ///         <description><c>16</c> – 65,536 colors (high color).</description>
  ///       </item>
  ///       <item>
  ///         <description><c>24</c> – Over 16 million colors (true color).</description>
  ///       </item>
  ///       <item>
  ///         <description>
  ///           <c>32</c> – Same as 24-bit color, but with extra bits (usually for
  ///           transparency).
  ///         </description>
  ///       </item>
  ///     </list>
  ///   </para>
  ///   <para>
  ///     This value reflects how the display mode is configured by the system, not how the physical
  ///     screen
  ///     is built or what it is capable of. A display might support high dynamic range (HDR) or more
  ///     precise colors (like 10 bits per channel), but that information is not captured here.
  ///   </para>
  /// </summary>

  [ScriptMember("colorDepth")]
  [TsTypeOverride("8 | 16 | 24 | 32")]
  public required int ColorDepth { get; init; }

  /// <summary>
  ///   The refresh rate of the display mode in Hertz. The refresh rate is the number of times the
  ///   display is updated per second. For example, a refresh rate of 60 Hz means the display is
  ///   updated 60 times per second. Higher refresh rates can reduce motion blur and flicker, and
  ///   generally make motion appear smoother, but require more processing power and bandwidth.
  /// </summary>

  [ScriptMember("refreshRate")]
  public required int RefreshRate { get; init; }

  /// <summary>
  ///   Indicates whether the display mode is interlaced. Interlaced display modes display every other
  ///   line of the image in each frame. For example 480i displays 240 lines in one frame and 240 lines
  ///   in the next frame. Non-interlaced display modes display all lines in each frame.
  /// </summary>

  [ScriptMember("isInterlaced")]
  public required bool IsInterlaced { get; init; }
}
