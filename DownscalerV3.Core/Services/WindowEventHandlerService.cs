using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;
using DownscalerV3.Contracts.Services;
using DownscalerV3.Core.Utils;
using static Windows.Win32.PInvoke;
using static DownscalerV3.Core.Utils.Macros;
using static DownscalerV3.Core.Utils.NativeUtils;

namespace DownscalerV3.Core.Services;

/// <inheritdoc />
public class WindowEventHandlerService : IWindowEventHandlerService {
  private HWND hwnd;
  private bool isInitialized;
  private SUBCLASSPROC? subclassProc;


  /// <inheritdoc />
  public void InitializeForWindow(HWND hwnd) {
    if (isInitialized) {
      // Check if the window handle is the same as the one we are already initialized for.
      // If it is not, throw an exception.
      if (hwnd != this.hwnd) {
        throw new InvalidOperationException(
          "The window event handler service is already initialized with a different window handle."
        );
      }

      // Otherwise, the initialization was only redundant, so we can return.
      return;
    }

    this.hwnd     = hwnd;
    isInitialized = true;
    InstallEventHandlers();
  }


  private static LRESULT WndProc(
    HWND hWnd,
    uint msg,
    WPARAM wParam,
    LPARAM lParam,
    nuint uIdSubclass,
    nuint dwRefData
  ) {
    var message = (Msg)msg;
    // Handle messages
    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
    switch (message) {
      case Msg.WM_MOUSEMOVE:
        Console.WriteLine($"Mouse moved to: ({GET_X_LPARAM(lParam)}, {GET_Y_LPARAM(lParam)})");
        break;
      case Msg.WM_NCMOUSEMOVE:
        Console.WriteLine(
          $"Mouse moved to non-client area: ({GET_X_LPARAM(lParam)}, {GET_Y_LPARAM(lParam)})"
        );
        break;
    }

    return DefSubclassProc(hWnd, msg, wParam, lParam);
  }


  private void InstallEventHandlers() {
    // Install the event handlers into the main window.
    InstallWindowSubclass(hwnd);

    // Get all child windows of the main window and install the event handlers into them.
    foreach (var child in EnumerateChildWindowsRecursively(hwnd)) {
      InstallWindowSubclass(child.Hwnd);
    }
  }


  private void InstallWindowSubclass(HWND hwnd) {
    if (!SetWindowSubclass(hwnd, WndProc, 1, nuint.Zero)) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }
  }


  private delegate nint WindowProc(HWND hWnd, Msg msg, WPARAM wParam, LPARAM lParam);

  private delegate nint SubclassProc(
    HWND hWnd,
    Msg msg,
    WPARAM wParam,
    LPARAM lParam,
    nint uIdSubclass,
    nint dwRefData
  );
}
