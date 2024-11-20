namespace DownscalerV3.Core.Contracts.Models.AppState;

/// <summary>
///   Represents the debug state per the application configuration as requested by the user.
/// </summary>
public interface IAppDebugState {
  /// <summary>
  ///   If <c>true</c>, debug information will be displayed in the downscaler window.
  /// </summary>
  bool Enabled { get; set; }

  /// <summary>
  ///   The font size to use for the debug information. A value of <c>1</c> means
  ///   the pixel font has a 1:1 ratio with the screen pixels. A value of <c>2</c>
  ///   means the pixel font has a 2:1 ratio with the screen pixels.
  /// </summary>
  int? FontSize { get; set; }

  /// <summary>
  ///   Whether to show the frames per second (FPS) in the debug information.
  ///   The FPS represents the FPS of the downscaler window, not the source
  ///   window.
  /// </summary>
  bool ShowFps { get; set; }

  /// <summary>
  ///   Whether to show the mouse coordinates in the debug information. This
  ///   is useful for debugging that the mouse coordinates are being correctly
  ///   transformed from the downscaler window to the source window.
  /// </summary>
  bool ShowMouseCoordinates { get; set; }
}
