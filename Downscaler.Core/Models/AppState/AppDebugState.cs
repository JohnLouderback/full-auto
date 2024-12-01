using Downscaler.Core.Contracts.Models.AppState;

namespace Downscaler.Core.Models.AppState;

public struct AppDebugState : IAppDebugState {
  /// <inheritdoc />
  public bool Enabled { get; set; }

  /// <inheritdoc />
  public int? FontScale { get; set; }

  /// <inheritdoc />
  public bool ShowFps { get; set; }

  /// <inheritdoc />
  public bool ShowMouseCoordinates { get; set; }

  /// <inheritdoc />
  public FontFamily FontFamily { get; set; }
}
