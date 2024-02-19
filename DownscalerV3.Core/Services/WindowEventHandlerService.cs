using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using DownscalerV3.Contracts.Services;
using DownscalerV3.Core.Utils;
using static Windows.Win32.PInvoke;
using static DownscalerV3.Core.Utils.Win32Ex;
using static DownscalerV3.Core.Utils.Macros;
using static DownscalerV3.Core.Utils.NativeUtils;

namespace DownscalerV3.Core.Services;

/// <inheritdoc />
public class WindowEventHandlerService : IWindowEventHandlerService {
  private static readonly HOOKPROC MouseHookProcInstance = MouseHookProc;
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
    // InstallHostWindow(hwnd);
    // InstallChildWindow(hwnd);
    InstallMouseHook();
    InstallEventHandlers();
  }


  public void MessageLoop() {
    var thread = new Thread(
      () => {
        MSG msg;
        while (GetMessage(out msg, hwnd, 0, 0)) {
          Console.WriteLine($"Message received: {Enum.GetName(typeof(Msg), msg.message)}");
          TranslateMessage(ref msg);
          DispatchMessage(ref msg);
        }
      }
    );

    thread.SetApartmentState(ApartmentState.STA);
    thread.Start();
  }


  private static LRESULT HostWndProc(
    HWND hWnd,
    uint msg,
    WPARAM wParam,
    LPARAM lParam
  ) {
    var message = (Msg)msg;
    // Handle messages
    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
    switch (message) {
      case Msg.WM_QUIT:
        Console.WriteLine("WM_QUIT received.");
        break;
      case Msg.WM_MOUSEMOVE:
        Console.WriteLine($"Mouse moved to: ({GET_X_LPARAM(lParam)}, {GET_Y_LPARAM(lParam)})");
        break;
      case Msg.WM_NCMOUSEMOVE:
        Console.WriteLine(
          $"Mouse moved to non-client area: ({GET_X_LPARAM(lParam)}, {GET_Y_LPARAM(lParam)})"
        );
        break;
    }

    return DefWindowProc(hWnd, msg, wParam, lParam);
  }


  private static LRESULT MouseHookProc(int nCode, WPARAM wParam, LPARAM lParam) {
    if (nCode >= 0) {
      Console.WriteLine($"Mouse hook called with nCode: {nCode}");
      // Log the mouse coordinates.
      var mouseHookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
      Console.WriteLine(
        $"Mouse coordinates: ({mouseHookStruct.pt.X}, {mouseHookStruct.pt.Y})"
      );
    }

    return CallNextHookEx(null, nCode, wParam, lParam);
  }


  private static LRESULT SubclassWndProc(
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
      default:
        Console.WriteLine(
          $"{Enum.GetName(typeof(Msg), message)} received on class: {hWnd.GetClassName()}"
        );
        break;
    }

    return DefSubclassProc(hWnd, msg, wParam, lParam);
  }


  private unsafe HWND CreateHostForWindow() {
    const string className = "DownscalerHost";
    var wc = new WNDCLASSEXW {
      cbSize        = (uint)Marshal.SizeOf(typeof(WNDCLASSEXW)),
      style         = 0,
      lpfnWndProc   = HostWndProc,
      cbClsExtra    = 0,
      cbWndExtra    = 0,
      hInstance     = GetModuleHandle(0),
      hIcon         = new HICON(nint.Zero),
      hCursor       = new HCURSOR(nint.Zero),
      hbrBackground = new HBRUSH(5), // BACKGROUND_WHITE,
      lpszMenuName  = null,
      lpszClassName = className.ToPWSTR(),
      hIconSm       = new HICON(nint.Zero)
    };

    var classAtom = RegisterClassEx(in wc);
    if (classAtom == 0) {
      throw new Win32Exception();
    }

    var host = CreateWindowEx(
      0,
      className,
      "Downscaler Host Window",
      WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
      0,
      0,
      640,
      480,
      new HWND(nint.Zero),
      null,
      GetModuleHandle(),
      (void*)nint.Zero
    );

    if (host == nint.Zero) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    return host;
  }


  private HWND InstallChildWindow(HWND hwnd) {
    var child = CreateNewWindow(
      "test",
      "test",
      WINDOW_STYLE.WS_CHILD,
      (hwnd, msg, wParam, lParam) => {
        Console.WriteLine(
          $"{Enum.GetName(typeof(Msg), (Msg)msg)} received on class: {hwnd.GetClassName()}"
        );
        return DefWindowProc(hwnd, msg, wParam, lParam);
      },
      25,
      25,
      640,
      480,
      hwnd
    );
    child.Show().Update().SetWindowPosition(25, 25, 640, 480, WindowZOrder.HWND_TOPMOST);
    return child;
  }


  private void InstallEventHandlers() {
    // Install the event handlers into the main window.
    InstallWindowSubclass(hwnd);

    // Get all child windows of the main window and install the event handlers into them.
    foreach (var child in EnumerateChildWindowsRecursively(hwnd)) {
      InstallWindowSubclass(child.Hwnd);
    }
  }


  /// <summary>
  ///   For a given window, creates a host window that will be used to host that window by way of
  ///   making it a child of the host window.
  /// </summary>
  /// <param name="hwnd"> The window handle of the window to be hosted. </param>
  /// <returns> The window handle of the host window. </returns>
  private HWND InstallHostWindow(HWND hwnd) {
    var host = CreateHostForWindow();
    host.Show().Update().SetWindowPosition(0, 0, 640, 480);
    hwnd.SetWindowStyle(
        WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_POPUP | WINDOW_STYLE.WS_VISIBLE
      )
      .SetParent(host)
      .SetWindowPosition(0, 0, 640, 480);
    return host;
  }


  private void InstallMouseHook() {
    var mouseHook = SetWindowsHookEx(
      WINDOWS_HOOK_ID.WH_MOUSE_LL,
      MouseHookProcInstance,
      null,
      0
    );

    if (mouseHook == null) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }
  }


  private void InstallWindowSubclass(HWND hwnd) {
    if (!SetWindowSubclass(hwnd, SubclassWndProc, 1, nuint.Zero)) {
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
