using DownscalerV3.Core.Models;

namespace DownscalerV3.Core.Contracts.Models.AppState;

/// <summary>
///   The AspectRatio enum represents options for either maintaining or stretching the aspect ratio
///   of the mirrored
///   window.
/// </summary>
public enum AspectRatio {
  Stretch,
  Maintain
}

/// <summary>
///   Represents the overarching global state of the application. Generally this stores the
///   processed results of parsing the command line arguments and the current state of the
///   application.
/// </summary>
public interface IAppState {
  uint WindowWidth { get; set; }

  uint WindowHeight { get; set; }

  double DownscaleFactor { get; set; }

  uint DownscaleWidth { get; set; }

  uint DownscaleHeight { get; set; }

  AspectRatio AspectRatio { get; set; }

  Win32Window              WindowToScale   { get; set; }
  Win32Window              DownscaleWindow { get; set; }
  IEnumerable<Win32Window> AllWindows      { get; set; }

  /// <summary>
  ///   Represents the debug state per the application configuration as requested by the user.
  /// </summary>
  IAppDebugState DebugState { get; set; }

  /// <summary>
  ///   The initial X position of the downscaler window as specified by the user.
  /// </summary>
  int? InitialX { get; set; }

  /// <summary>
  ///   The initial Y position of the downscaler window as specified by the user.
  /// </summary>
  int? InitialY { get; set; }
}
