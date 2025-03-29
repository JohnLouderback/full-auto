using Windows.Win32.Graphics.Gdi;

namespace Core.Utils;

public static class DevModeWExtensions {
  private const uint DM_INTERLACED = 0x00000002;


  /// <summary>
  ///   Determines whether the display mode is interlaced.
  /// </summary>
  /// <param name="mode"> The display mode. </param>
  /// <returns>
  ///   Whether the display mode is interlaced. Interlaced display modes display every other line of
  ///   the image in each frame. For example 480i displays 240 lines in one frame and 240 lines in
  ///   the next frame. Non-interlaced display modes display all lines in each frame.
  /// </returns>
  public static bool IsInterlaced(this DEVMODEW mode) {
    return (mode.Anonymous2.dmDisplayFlags & DM_INTERLACED) != 0;
  }
}
