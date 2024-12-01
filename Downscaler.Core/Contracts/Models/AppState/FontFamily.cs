namespace Downscaler.Core.Contracts.Models.AppState;

/// <summary>
///   The font family to use for the debug information
/// </summary>
public enum FontFamily {
  /// <summary>
  ///   Useful for small windows like 240p. (font: Pixel Rocks, 3px tall)
  /// </summary>
  ExtraSmall,

  /// <summary>
  ///   Useful for small windows like 240p. (font: Pixel Millennium, 5px tall)
  /// </summary>
  Small,

  /// <summary>
  ///   Useful for most windows. (font: Dogica Pixel, 7px tall)
  /// </summary>
  Normal,

  /// <summary>
  ///   Useful for larger windows like 1024x768 and up. (font: Fixedsys, 9px tall)
  /// </summary>
  Large
}
