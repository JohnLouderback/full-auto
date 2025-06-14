using Windows.Win32.Foundation;
using Core.Models;
using Core.Utils;
using GameLauncher.Core.CodeGenAttributes;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a window of an application.
/// </summary>
[TypeScriptExport]
public partial class Window : ObjectBase {
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


  internal Window(HWND hwnd) {
    win32Window = new Win32Window {
      Hwnd        = hwnd,
      Title       = hwnd.GetWindowText(),
      ClassName   = hwnd.GetClassName(),
      ProcessName = hwnd.GetProcessName(),
      ProcessID   = hwnd.GetProcessID()
    };
    this.hwnd = hwnd;

    Init();
  }


  internal Window(Win32Window window) {
    win32Window = window;
    hwnd        = window.Hwnd;
    Init();
  }


  private void Init() {
    engine = AppState.ScriptEngine;

    // Ensure the window still exists at this point before trying to initialize events.
    if (!IsClosed) {
      InitializeEvents();
    }
  }
}
