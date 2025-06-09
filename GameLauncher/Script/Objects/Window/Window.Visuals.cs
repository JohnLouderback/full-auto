using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Core.Utils;
using GameLauncher.Services;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

public partial class Window {
  /// <summary>
  ///   Sets the window to be borderless. This removes the window's title bar and borders, making it
  ///   appears as purely rectangular rendering surface.
  /// </summary>
  [ScriptMember("makeBorderless")]
  public void MakeBorderless() {
    // Get the current window styles
    var style   = hwnd.GetWindowStyle();
    var exStyle = hwnd.GetWindowExStyle();

    // Modify the styles to remove borders and title bar.
    style &= ~(WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_THICKFRAME);
    style |= WINDOW_STYLE.WS_POPUP | WINDOW_STYLE.WS_VISIBLE;

    // Extended styles can retain compositing, but don't *require* WS_EX_LAYERED unless the window
    // is doing transparency.
    exStyle &= ~(WINDOW_EX_STYLE.WS_EX_DLGMODALFRAME |
                 WINDOW_EX_STYLE.WS_EX_CLIENTEDGE |
                 WINDOW_EX_STYLE.WS_EX_STATICEDGE);
    exStyle |= WINDOW_EX_STYLE.WS_EX_APPWINDOW;

    hwnd.SetWindowStyle(style);
    hwnd.SetWindowExStyle(exStyle);

    // Force the window to redraw with the new styles
    PInvoke.SetWindowPos(
      hwnd,
      (HWND)0,
      X: 0,
      Y: 0,
      cx: 0,
      cy: 0,
      SET_WINDOW_POS_FLAGS.SWP_NOMOVE |
      SET_WINDOW_POS_FLAGS.SWP_NOSIZE |
      SET_WINDOW_POS_FLAGS.SWP_NOZORDER |
      SET_WINDOW_POS_FLAGS.SWP_FRAMECHANGED
    );
  }


  /// <summary>
  ///   Applies a matte effect to the window, embedding it within a colored backdrop. This is useful for
  ///   focusing attention on the window's content by surrounding it with a solid color, effectively
  ///   "blacking out" the rest of the screen around the window.
  /// </summary>
  /// <param name="backdropColor">
  ///   The color to use for the backdrop matte. This should be a hex color code (e.g., "#000000" for
  ///   black). Defaults to black if not specified.
  /// </param>
  [ScriptMember("matte")]
  public async Task Matte(string backdropColor = "#000000") {
    var color = ColorTranslator.FromHtml(backdropColor);
    await GuiService.Instance.ShowMatteWindow(hwnd, color);
  }
}
