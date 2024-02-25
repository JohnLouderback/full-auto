using System.Drawing;

namespace DownscalerV3.Core.Contracts.Models;

/// <summary>
///   Represents the coordinates of the mouse in different coordinate systems.
/// </summary>
public interface IMouseCoords {
  /// <summary>
  ///   The absolute position of the mouse on the screen where (0, 0) is the top-left corner of the
  ///   primary monitor.
  /// </summary>
  public Point Absolute { get; }

  /// <summary>
  ///   The mouse position relative to the downscaled (i.e. this) window. This is the position of the
  ///   mouse relative to the client area of the downscaled window.
  /// </summary>
  public Point RelativeToDownscaledWindow { get; }

  /// <summary>
  ///   The mouse position relative to the source window. This is the position of the mouse relative
  ///   to the client area of the source window. These coordinates are scaled based on the current
  ///   X and Y downscaling factors. If the downscaled window is half the height of the source
  ///   window, and the cursor is moving over the downscaled window, the Y-coordinate will be
  ///   doubled from its value in the downscaled window - mapping the cursor to its correct position
  ///   on the source window.
  /// </summary>
  public Point RelativeToSourceWindow { get; }

  /// <summary>
  ///   Indicates whether the mouse coordinates show that the cursor is within the downscaled
  ///   window's client area.
  /// </summary>
  public bool IsWithinDownscaledWindow { get; }
}
