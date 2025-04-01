using Core.Utils;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

public partial class Window {
  /// <summary>
  ///   Gets the bounding box of the window. This is the rectangle that contains the window's
  ///   position and size on the screen. The pixels are in screen coordinates, with the origin
  ///   in the top-left corner of the screen.
  /// </summary>
  /// <returns></returns>
  [ScriptMember("getBoundingBox")]
  public BoundingBox GetBoundingBox() {
    var rect = win32Window.GetWindowRect();

    var boundingBox = new BoundingBox {
      X      = rect.left,
      Y      = rect.top,
      Width  = rect.right - rect.left,
      Height = rect.bottom - rect.top
    };

    return boundingBox;
  }


  /// <summary>
  ///   Makes the window fullscreen. It does not automatically change the window's style to
  ///   borderless. This is done by the <see cref="MakeBorderless" /> method.
  /// </summary>
  /// <param name="method">
  ///   The method to use to make the window fullscreen. Valid values are the following:
  ///   <ul>
  ///     <li>
  ///       <c> "resize" </c> - Sets the window's width and height to cover the entire screen. This
  ///       will set the window to cover the entire screen of the monitor the window currently
  ///       resides on. The window will be resized to the monitor's current resolution. This is the
  ///       default method. To remove the window's borders, use the <see cref="MakeBorderless" />
  ///       method first, before calling this method.
  ///     </li>
  ///     <li>
  ///       <c> "alt enter" </c> - Sends the <c> Alt + Enter </c> key combination to the window. This
  ///       is the same as pressing <c> Alt + Enter </c> on the keyboard. This method may not work
  ///       if the window does not support it.
  ///     </li>
  ///   </ul>
  /// </param>
  [ScriptMember("makeFullscreen")]
  public void MakeFullscreen(
    [TsTypeOverride(""" "resize" | "alt enter" """)]
    string method = "resize"
  ) {
    // For now, we only implement the resize method.
    switch (method) {
      case "resize": {
        var win32Monitor = win32Window.GetMonitor();
        var displayMode  = win32Monitor.GetCurrentDisplayMode();

        var boundingBox = new BoundingBox {
          X      = 0,
          Y      = 0,
          Width  = (int)displayMode.dmPelsWidth,
          Height = (int)displayMode.dmPelsHeight
        };
        hwnd.SetWindowPosition(
          boundingBox.X,
          boundingBox.Y,
          boundingBox.Width,
          boundingBox.Height
        );
        return;
      }
      case "alt enter": {
        throw new NotImplementedException(
          "The method 'alt enter' is not implemented yet. Please use 'resize' instead."
        );
      }
      default: {
        throw new ArgumentException(
          $"The method of method makeFullscreen must be either 'resize' or 'alt enter'. Got '{
            method
          }'."
        );
      }
    }
  }


  /// <summary>
  ///   Sets the position and size of the window.
  /// </summary>
  /// <param name="boundingBox"> The bounding box to set the window to. </param>
  /// <returns> The same window this method was called on, for chaining. </returns>
  [ScriptMember("setBoundingBox")]
  public Window SetBoundingBox(BoundingBox boundingBox) {
    hwnd.SetWindowPosition(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
    return this;
  }


  /// <summary>
  ///   Sets the position and size of the window.
  /// </summary>
  /// <param name="x">
  ///   The x-coordinate of the window.
  /// </param>
  /// <param name="y">
  ///   The y-coordinate of the window.
  /// </param>
  /// <param name="width"> The width of the window. </param>
  /// <param name="height"> The height of the window. </param>
  /// <returns> The same window this method was called on, for chaining. </returns>
  [ScriptMember("setBoundingBox")]
  public Window SetBoundingBox(int x, int y, int width, int height) {
    hwnd.SetWindowPosition(x, y, width, height);
    return this;
  }
}
