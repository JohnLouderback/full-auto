using DownscalerV3.Core.Contracts.Models;

namespace DownscalerV3.Core.Models;

public class AppState : IAppState {
  /// <inheritdoc />
  public uint WindowWidth { get; set; }

  /// <inheritdoc />
  public uint WindowHeight { get; set; }

  /// <inheritdoc />
  public double DownscaleFactor { get; set; }

  /// <inheritdoc />
  public uint DownscaleWidth { get; set; }

  /// <inheritdoc />
  public uint DownscaleHeight { get; set; }

  /// <inheritdoc />
  public AspectRatio AspectRatio { get; set; }

  /// <inheritdoc />
  public Win32Window WindowToScale { get; set; }

  /// <inheritdoc />
  public Win32Window DownscaleWindow { get; set; }

  /// <inheritdoc />
  public IEnumerable<Win32Window> AllWindows { get; set; }
}
