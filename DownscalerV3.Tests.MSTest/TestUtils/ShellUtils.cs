using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace DownscalerV3.Tests.MSTest.TestUtils;

public static class ShellUtils {
  // Constants for window styles
  private const int WS_OVERLAPPEDWINDOW = 0x00CF0000;
  private const int SW_SHOW = 5;

  // Window procedure delegate
  public delegate nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam);

  public static string ClassName { get; } = "MyWindowClass";
  public static string WindowName { get; } = "My Test Window";
  public static string ProcessName { get; } = "MyTestProcess";


  public static HWND CreateAndShowTestWindow() {
    var hInstance = GetModuleHandle(null);
    var className = ClassName;
    var wc = new WNDCLASSEX {
      cbSize        = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
      style         = 0,
      lpfnWndProc   = MyWndProc,
      cbClsExtra    = 0,
      cbWndExtra    = 0,
      hInstance     = hInstance,
      hIcon         = nint.Zero,
      hCursor       = nint.Zero,
      hbrBackground = 5, // BACKGROUND_WHITE,
      lpszMenuName  = null,
      lpszClassName = className,
      hIconSm       = nint.Zero
    };

    var classAtom = RegisterClassEx(ref wc);
    if (classAtom == 0) {
      throw new Win32Exception();
    }

    var hWnd = CreateWindowEx(
      0,
      className,
      WindowName,
      WS_OVERLAPPEDWINDOW,
      0,
      0,
      800,
      600,
      nint.Zero,
      nint.Zero,
      hInstance,
      nint.Zero
    );

    if (hWnd == nint.Zero) {
      throw new Win32Exception();
    }

    ShowWindow(hWnd, SW_SHOW);
    UpdateWindow(hWnd);

    // The window will need to be manually closed
    // This is a basic example and does not pump messages

    return new HWND(hWnd);
  }


  [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  public static extern nint GetModuleHandle(string? lpModuleName);


  // Import necessary functions from user32.dll
  [DllImport("user32.dll", SetLastError = true)]
  private static extern nint CreateWindowEx(
    uint dwExStyle,
    string lpClassName,
    string lpWindowName,
    uint dwStyle,
    int x,
    int y,
    int nWidth,
    int nHeight,
    nint hWndParent,
    nint hMenu,
    nint hInstance,
    nint lpParam
  );


  [DllImport("user32.dll", SetLastError = true)]
  private static extern nint DefWindowProc(nint hWnd, uint uMsg, nint wParam, nint lParam);


  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool DestroyWindow(nint hWnd);


  private static nint MyWndProc(nint hWnd, uint msg, nint wParam, nint lParam) {
    // Handle window messages here if needed
    return DefWindowProc(hWnd, msg, wParam, lParam);
  }


  [DllImport("user32.dll", SetLastError = true)]
  private static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);


  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool ShowWindow(nint hWnd, int nCmdShow);


  [DllImport("user32.dll", SetLastError = true)]
  private static extern bool UpdateWindow(nint hWnd);


  // Define the window class structure
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
  public struct WNDCLASSEX {
    public uint cbSize;
    public uint style;
    public WndProc lpfnWndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public IntPtr hInstance;
    public IntPtr hIcon;
    public IntPtr hCursor;
    public IntPtr hbrBackground;
    public string lpszMenuName;
    public string lpszClassName;
    public IntPtr hIconSm;
  }
}
