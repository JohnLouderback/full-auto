using System.Drawing;
using Windows.Win32.Foundation;

namespace Downscaler.Core.Utils;

public static class RectExtensions {
  /// <summary>
  ///   Determines whether the given point is contained within the rectangle.
  /// </summary>
  /// <param name="rect"> The rectangle to check. </param>
  /// <param name="x"> The x-coordinate of the point. </param>
  /// <param name="y"> The y-coordinate of the point. </param>
  /// <returns>
  ///   <see langword="true" /> if the point is contained within the rectangle; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  public static bool Contains(this RECT rect, int x, int y) {
    return x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom;
  }


  /// <summary>
  ///   Determines whether the given point is contained within the rectangle.
  /// </summary>
  /// <param name="rect"> The rectangle to check. </param>
  /// <param name="point"> The point to check. </param>
  /// <returns>
  ///   <see langword="true" /> if the point is contained within the rectangle; otherwise,
  ///   <see langword="false" />.
  /// </returns>
  public static bool Contains(this RECT rect, Point point) {
    return rect.Contains(point.X, point.Y);
  }
}
