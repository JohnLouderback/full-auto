using DownscalerV3.Core.Contracts.Models.Yaml;

namespace DownscalerV3.Core.Models.Yaml;

public class DebugConfig : IDebugConfig {
  /// <inheritdoc />
  public bool? Enabled { get; set; }

  /// <inheritdoc />
  public int? FontScale { get; set; }

  /// <inheritdoc />
  public bool? ShowFps { get; set; }

  /// <inheritdoc />
  public bool? ShowMouseCoordinates { get; set; }
}
