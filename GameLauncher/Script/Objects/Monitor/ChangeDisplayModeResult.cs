using System.Runtime.InteropServices;
using Windows.Win32.Graphics.Gdi;
using Core.Models;
using Core.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents the result of changing the display mode of a monitor. Calling the
///   <see cref="ChangeDisplayModeResult.Undo" /> method will revert the display mode to the previous
///   mode.
/// </summary>
[TypeScriptExport]
public class ChangeDisplayModeResult : UndoableResult {
  private readonly DisplayMode  originalDisplayMode;
  private readonly Win32Monitor win32Monitor;


  internal ChangeDisplayModeResult(
    Win32Monitor win32Monitor,
    DisplayMode originalDisplayMode,
    bool shouldPersist
  ) {
    this.win32Monitor        = win32Monitor;
    this.originalDisplayMode = originalDisplayMode;
    ShouldUndo               = !shouldPersist;
  }


  /// <inheritdoc />
  protected override async Task Reverse() {
    Console.WriteLine($"Reverting to original display mode: {originalDisplayMode}");
    var dm = new DEVMODEW();
    dm.dmSize             = (ushort)Marshal.SizeOf(typeof(DEVMODEW));
    dm.dmPelsWidth        = (uint)originalDisplayMode.Width;
    dm.dmPelsHeight       = (uint)originalDisplayMode.Height;
    dm.dmBitsPerPel       = (uint)originalDisplayMode.ColorDepth;
    dm.dmDisplayFrequency = (uint)originalDisplayMode.RefreshRate;

    // Set the display mode to the specified display mode and as temporary. Temporary display modes
    // are not saved to the registry and are reset when the system is restarted.
    win32Monitor.SetDisplayModeOrThrow(dm, cdsType: 0);
  }
}
