namespace DownscalerV3.Core.Contracts.Models.Yaml;

/// <summary>
///   Represents the configuration for an instance of the application that can be parsed from a YAML
///   file. Note that this represents all possible options, however not all options are valid
///   together. For this reason, many options are nullable. For example, you may not specify a
///   downscale factor and a scale width or height at the same time.
/// </summary>
public interface IYamlConfig {
  /// <summary>
  ///   The X position of the top-left corner of the downscaler window. This will be relative
  ///   to the monitor that the window is on. If this is not specified, the window will be
  ///   positioned automatically.
  /// </summary>
  int? X { get; set; }

  /// <summary>
  ///   The Y position of the top-left corner of the downscaler window. This will be relative
  ///   to the monitor that the window is on. If this is not specified, the window will be
  ///   positioned automatically.
  /// </summary>
  int? Y { get; set; }

  /// <summary>
  ///   The window title to use to search for the window to downscale. This is exclusive with
  ///   <see cref="ProcessName" />.
  /// </summary>
  string? WindowTitle { get; set; }

  /// <summary>
  ///   The name of a running process to search for to use as the window to downscale. This is
  ///   exclusive with <see cref="WindowTitle" />. Process names are case-insensitive and should
  ///   always end with the ".exe" extension.
  /// </summary>
  string? ProcessName { get; set; }

  /// <summary>
  ///   Class name of the window to mirror. You can use a tool like "Spy++", "Window Detective",
  ///   or similar to find the class name of a window. Class names are case-sensitive. This is
  ///   useful for finding windows that have the same title, but different class names, particularly
  ///   child windows of a parent window.
  /// </summary>
  string? ClassName { get; set; }

  /// <summary>
  ///   The factor to downscale the window by. For example, a factor of 2 will downscale the window
  ///   by 2x, meaning that a window of size 1920x1080 will be downscaled to 960x540. To upscale the
  ///   window, use a factor less than 1. For example, a factor of 0.5 will upscale the window by 2x,
  ///   meaning that a window of size 1920x1080 will be upscaled to 3840x2160.
  /// </summary>
  double? DownscaleFactor { get; set; }

  /// <summary>
  ///   The width to scale the window to. This is exclusive with <see cref="DownscaleFactor" />. If
  ///   this is specified, but height is not, the height will be scaled proportionally to maintain
  ///   the aspect ratio of the window.
  /// </summary>
  int? ScaleWidth { get; set; }

  /// <summary>
  ///   The height to scale the window to. This is exclusive with <see cref="DownscaleFactor" />. If
  ///   this is specified, but width is not, the width will be scaled proportionally to maintain the
  ///   aspect ratio of the window.
  /// </summary>
  int? ScaleHeight { get; set; }

  /// <summary>
  ///   A namespace where debug configurations can be specified.
  /// </summary>
  IDebugConfig? Debug { get; set; }
}
