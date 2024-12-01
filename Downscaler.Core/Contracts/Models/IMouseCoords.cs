using System.Drawing;

namespace Downscaler.Core.Contracts.Models;

/// <summary>
///   Represents the coordinates of the mouse in different coordinate systems.
/// </summary>
public interface IMouseCoords {
  /// <summary>
  ///   The absolute position of the mouse on the screen where (0, 0) is the top-left corner of the
  ///   primary monitor.
  /// </summary>
  Point Absolute { get; }

  /// <summary>
  ///   The mouse position relative to the downscaled (i.e. this) window. This is the position of the
  ///   mouse relative to the client area of the downscaled window.
  /// </summary>
  Point RelativeToDownscaledWindow { get; }

  /// <summary>
  ///   The mouse position relative to the source window. This is the position of the mouse relative
  ///   to the client area of the source window. These coordinates are scaled based on the current
  ///   X and Y downscaling factors. If the downscaled window is half the height of the source
  ///   window, and the cursor is moving over the downscaled window, the Y-coordinate will be
  ///   doubled from its value in the downscaled window - mapping the cursor to its correct position
  ///   on the source window.
  /// </summary>
  Point RelativeToSourceWindow { get; }

  /// <summary>
  ///   Indicates whether the mouse coordinates show that the cursor is within the downscaled
  ///   window's client area.
  /// </summary>
  bool IsWithinDownscaledWindow { get; }

  /// <summary>
  ///   Indicates whether the mouse coordinates show that the cursor is within the source window's
  ///   client area.
  /// </summary>
  bool IsWithinSourceWindow { get; }

  /// <summary>
  ///   The mouse position relative to the downscaled window as a percentage of the downscaled window's
  ///   client area. X at the left-most edge is 0.0, X at the right-most edge is 1.0. Y at the top-most
  ///   edge is 0.0, Y at the bottom-most edge is 1.0.
  /// </summary>
  (float X, float Y) RelativeToDownscaledWindowPercent { get; }

  /// <summary>
  ///   The mouse position relative to the source window as a percentage of the source window's
  ///   client area. X at the left-most edge is 0.0, X at the right-most edge is 1.0. Y at the top-most
  ///   edge is 0.0, Y at the bottom-most edge is 1.0.
  /// </summary>
  (float X, float Y) RelativeToSourceWindowPercent { get; }
}
