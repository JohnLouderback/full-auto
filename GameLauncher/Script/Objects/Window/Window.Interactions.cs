using Windows.Win32.UI.WindowsAndMessaging;
using Core.Utils;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

public partial class Window {
  /// <summary>
  ///   Requests that the window be closed. This sends a close message to the window, which may or
  ///   may not result in the window being closed. The window may choose to ignore the request.
  /// </summary>
  [ScriptMember("close")]
  public void Close() {
    hwnd.SendMessage(Msg.WM_CLOSE);
  }


  // [ScriptMember("destroy")]
  // public void Destroy() {
  //   // Windows can only be destroyed by their owner thread. This will need to be implemented with
  //   // a hook into the window's thread.
  //   hwnd.DestroyWindow();
  // }


  /// <summary>
  ///   Focuses the window. This brings the window to the front and makes it the active window.
  /// </summary>
  [ScriptMember("focus")]
  public void Focus() {
    win32Window.Focus();
  }


  /// <summary>
  ///   Maximizes the window. This expands the window to fill the entire screen.
  /// </summary>
  [ScriptMember("maximize")]
  public void Maximize() {
    hwnd.SetWindowPlacementCommand(SHOW_WINDOW_CMD.SW_MAXIMIZE);
  }


  /// <summary>
  ///   Minimizes the window. This reduces the window to an icon on the taskbar or otherwise
  ///   hides it from view.
  /// </summary>
  [ScriptMember("minimize")]
  public void Minimize() {
    hwnd.SetWindowPlacementCommand(SHOW_WINDOW_CMD.SW_MINIMIZE);
  }


  /// <summary>
  ///   Restores the window. This restores the window to its previous size and position after being
  ///   minimized or maximized.
  /// </summary>
  [ScriptMember("restore")]
  public void Restore() {
    hwnd.SetWindowPlacementCommand(SHOW_WINDOW_CMD.SW_RESTORE);
  }
}
