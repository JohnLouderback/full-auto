using Windows.Win32.Foundation;
using Core.Models;
using Core.Utils;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a window of an application or another window.
/// </summary>
[TypeScriptExport]
public class Window {
  private readonly HWND         hwnd;
  private readonly Win32Window  win32Window;
  private          ScriptEngine engine;

  /// <summary>
  ///   The handle of the window. This is a unique identifier representing the window.
  /// </summary>
  [ScriptMember("handle")]
  public int Handle => (int)hwnd;

  /// <summary>
  ///   The text of the window's titlebar. For example: <c> "Untitled - Notepad" </c>. It may be used
  ///   to either get or set the title of the window.
  /// </summary>
  [ScriptMember("title")]
  public string Title {
    get => hwnd.GetWindowText();
    set => hwnd.SetWindowText(value);
  }

  /// <summary>
  ///   The class name of the window. For example: <c> "Notepad" </c>. Class names are generally
  ///   used to identify the type of window within the application. They are not necessarily unique.
  /// </summary>
  [ScriptMember("className")]
  public string ClassName => hwnd.GetClassName();


  internal Window(Win32Window window) {
    engine      = ScriptEngine.Current;
    win32Window = window;
    hwnd        = window.Hwnd;
  }


  [ScriptMember("getBoundingBox")]
  public BoundingBox GetBoundingBox() {
    var rect = hwnd.GetWindowRect();
    
    var boundingBox = new BoundingBox {
      X      = rect.Left,
      Y      = rect.Top,
      Width  = rect.Right - rect.Left,
      Height = rect.Bottom - rect.Top
    };

    return boundingBox;
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
