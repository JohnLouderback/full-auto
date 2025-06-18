using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Core.Models;
using Core.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;
using static Windows.Win32.PInvoke;
using static Core.Utils.Macros;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents the Windows taskbar, which is the bar typically located at the bottom of the screen
///   that contains the Start button, open application icons, and system tray.
/// </summary>
/// <inheritdoc />
[TypeScriptExport]
public class Taskbar : Window {
  internal const int ABS_AUTOHIDE = 0x0000001;

  private const int ABM_GETSTATE    = 0x00000004;
  private const int ABM_SETSTATE    = 0x0000000A;
  private const int ABS_ALWAYSONTOP = 0x0000002;


  /// <inheritdoc />
  internal Taskbar(HWND hwnd) : base(hwnd) {}


  /// <inheritdoc />
  internal Taskbar(Win32Window window) : base(window) {}


  /// <summary>
  ///   Disables auto-hide for the taskbar. When disabled, the taskbar will remain visible
  ///   at all times, even when not in use.
  /// </summary>
  /// <param name="shouldPersist">
  ///   If <see langword="true" />, the change will persist after the script execution ends.
  ///   Otherwise, the change will only apply during the script execution and will be
  ///   reversed when the script finishes.
  /// </param>
  [ScriptMember("disableAutoHide")]
  public AutoHideTaskbarResult DisableAutoHide(bool shouldPersist = false) {
    var (settings, state) = GetTaskbarSettings();
    settings.lParam       = 0; // Turn off auto-hide
    SaveTaskbarSettings(settings);
    return new AutoHideTaskbarResult(
      (state & ABS_AUTOHIDE) != 0,
      !shouldPersist
    );
  }


  /// <summary>
  ///   Enables auto-hide for the taskbar. When enabled, the taskbar will automatically hide
  ///   when not in use, and will reappear when the mouse is moved to the edge of the screen
  ///   where the taskbar is located.
  /// </summary>
  /// <param name="shouldPersist">
  ///   If <see langword="true" />, the change will persist after the script execution ends.
  ///   Otherwise, the change will only apply during the script execution and will be
  ///   reversed when the script finishes.
  /// </param>
  [ScriptMember("enableAutoHide")]
  public AutoHideTaskbarResult EnableAutoHide(bool shouldPersist = false) {
    var (settings, state) = GetTaskbarSettings();
    settings.lParam       = ABS_AUTOHIDE; // Turn on auto-hide
    SaveTaskbarSettings(settings);
    return new AutoHideTaskbarResult(
      (state & ABS_AUTOHIDE) != 0,
      !shouldPersist
    );
  }


  /// <summary>
  ///   Checks if the taskbar is currently set to auto-hide. If auto-hide is enabled,
  /// </summary>
  /// <returns>
  ///   <see langword="true" /> if auto-hide is enabled; otherwise, <see langword="false" />.
  /// </returns>
  [ScriptMember("isAutoHideEnabled")]
  public bool IsAutoHideEnabled() {
    var (settings, state) = GetTaskbarSettings();
    return (state & ABS_AUTOHIDE) != 0;
  }


  /// <summary>
  ///   Checks if the taskbar is currently set to auto-hide. If auto-hide is enabled,
  ///   this method will toggle the setting to disable it, and vice versa.
  /// </summary>
  /// <param name="shouldPersist">
  ///   If <see langword="true" />, the change will persist after the script execution ends.
  ///   Otherwise, the change will only apply during the script execution and will be
  ///   reversed when the script finishes.
  /// </param>
  [ScriptMember("toggleAutoHide")]
  public AutoHideTaskbarResult ToggleAutoHide(bool shouldPersist = false) {
    var (settings, state) = GetTaskbarSettings();
    if ((state & ABS_AUTOHIDE) != 0) {
      settings.lParam = 0; // Turn off auto-hide
    }
    else {
      settings.lParam = ABS_AUTOHIDE; // Turn on auto-hide
    }

    SaveTaskbarSettings(settings);
    return new AutoHideTaskbarResult(
      (state & ABS_AUTOHIDE) != 0,
      !shouldPersist
    );
  }


  /// <summary>
  ///   Gets the current taskbar settings from the Windows registry.
  /// </summary>
  /// <returns>
  ///   A tuple containing the <see cref="APPBARDATA" /> structure with the taskbar settings.
  /// </returns>
  internal static (APPBARDATA, int) GetTaskbarSettings() {
    var abd = new APPBARDATA();
    abd.cbSize = Marshal.SizeOf<APPBARDATA>();

    var state = SHAppBarMessage(ABM_GETSTATE, ref abd);

    return (abd, state);
  }


  /// <summary>
  ///   Notifies the Windows shell that taskbar settings have changed.
  /// </summary>
  internal static unsafe void NotifyShell() {
    fixed (char* lParam = "TraySettings") {
      SendMessageTimeout(
        (HWND)HWND_BROADCAST,
        (uint)Msg.WM_SETTINGCHANGE,
        (nuint)NULL,
        (nint)lParam,
        SEND_MESSAGE_TIMEOUT_FLAGS.SMTO_ABORTIFHUNG,
        uTimeout: 100
      );
    }
  }


  internal static void SaveTaskbarSettings(APPBARDATA settings) {
    // Send the set state message to the taskbar.
    SHAppBarMessage(ABM_SETSTATE, ref settings);
    NotifyShell();
  }


  [DllImport("shell32.dll", CallingConvention = CallingConvention.StdCall)]
  private static extern int SHAppBarMessage(int dwMessage, ref APPBARDATA pData);


  [StructLayout(LayoutKind.Sequential)]
  internal struct APPBARDATA {
    public int    cbSize;
    public IntPtr hWnd;
    public uint   uCallbackMessage;
    public uint   uEdge;
    public RECT   rc;
    public int    lParam;
  }
}
