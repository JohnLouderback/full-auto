using DownscalerV3.Core.Contracts.Models.Yaml;

namespace DownscalerV3.Core.Models.Yaml;

public class YamlConfig : IYamlConfig {
  /// <inheritdoc />
  public int? X { get; set; }

  /// <inheritdoc />
  public int? Y { get; set; }

  /// <inheritdoc />
  public string? WindowTitle { get; set; }

  /// <inheritdoc />
  public string? ProcessName { get; set; }

  /// <inheritdoc />
  public string? ClassName { get; set; }

  /// <inheritdoc />
  public double? DownscaleFactor { get; set; }

  /// <inheritdoc />
  public int? ScaleWidth { get; set; }

  /// <inheritdoc />
  public int? ScaleHeight { get; set; }

  /// <inheritdoc />
  public IDebugConfig? Debug { get; set; }
}
