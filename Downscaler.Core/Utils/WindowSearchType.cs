namespace Downscaler.Core.Utils;

/// <summary>
///   The type of search to perform when looking for a window.
/// </summary>
public enum WindowSearchType {
  /// <summary>
  ///   Search by the window's title.
  /// </summary>
  Title,

  /// <summary>
  ///   Search by the window's process name.
  /// </summary>
  ProcessName
}
