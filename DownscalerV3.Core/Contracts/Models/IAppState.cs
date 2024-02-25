using DownscalerV3.Core.Models;

namespace DownscalerV3.Core.Contracts.Models;

/// <summary>
///   The AspectRatio enum represents options for either maintaining or stretching the aspect ratio
///   of the mirrored
///   window.
/// </summary>
public enum AspectRatio {
  Stretch,
  Maintain
}

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
}
