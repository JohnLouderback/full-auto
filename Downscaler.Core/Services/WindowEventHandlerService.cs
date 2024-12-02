using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;
using Core.Utils;
using Downscaler.Core.Contracts.Models.AppState;
using Downscaler.Core.Contracts.Services;
using static Windows.Win32.PInvoke;
using static Core.Utils.Macros;
using static Core.Utils.NativeUtils;

namespace Downscaler.Core.Services;

/// <inheritdoc />
public class WindowEventHandlerService : IWindowEventHandlerService {
  private static readonly HOOKPROC MouseHookProcInstance = MouseHookProc;

  /// <summary>
  ///   The buffer used to store the raw input data. Points to a buffer allocated with
  ///   <see cref="RAWINPUT" /> structures.
  /// </summary>
  private static nint rawInputBuffer = nint.Zero;

  /// <summary>
  ///   The size of the buffer used to store the raw input data. This is the size of the buffer
  ///   allocated with <see cref="RAWINPUT" /> structures. If the buffer size needs to be increased,
  ///   the buffer will be reallocated.
  /// </summary>
  private static uint rawInputBufferSize;

  private static IAppState?          AppState;
  private static IMouseEventService? MouseEventService;

  private HWND          hwnd;
  private bool          isInitialized;
  private SUBCLASSPROC? subclassProc;


  /// <inheritdoc />
  public WindowEventHandlerService(IAppState appState, IMouseEventService mouseEventService) {
    AppState          = appState;
    MouseEventService = mouseEventService;
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Point GetMousePosition() {
    Point lpPoint;
    if (GetCursorPos(out lpPoint) == 0) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    return lpPoint;
  }


  /// <summary>
  ///   Performs cleanup operations for the window event handler service. This includes freeing any
  ///   resources that were allocated during initialization and unregistering any event handlers.
  /// </summary>
  public void CleanUp() {
    if (!isInitialized) return;
    if (rawInputBuffer != nint.Zero) {
      Marshal.FreeHGlobal(rawInputBuffer);
      rawInputBuffer = nint.Zero;
    }
  }


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
    // InstallMouseHook();
    InstallEventHandlers();
  }


  /// <summary>
  ///   Registers the window to receive messages for raw input from a mouse.
  /// </summary>
  /// <param name="hwnd"> </param>
  /// <exception cref="Win32Exception"> </exception>
  public unsafe void RegisterForRawInput(HWND hwnd) {
    var devices = new RAWINPUTDEVICE[1];

    devices[0].usUsagePage = HID_USAGE_PAGE_GENERIC;
    devices[0].usUsage     = HID_USAGE_GENERIC_MOUSE;
    devices[0].dwFlags     = RAWINPUTDEVICE_FLAGS.RIDEV_INPUTSINK;
    devices[0].hwndTarget  = hwnd;

    fixed (RAWINPUTDEVICE* devicePtr = &devices[0]) {
      if (!RegisterRawInputDevices(
            devicePtr,
            (uint)devices.Length,
            (uint)Marshal.SizeOf<RAWINPUTDEVICE>()
          )) {
        throw new Win32Exception(Marshal.GetLastWin32Error());
      }
    }
  }


  /// <summary>
  ///   Allocates a buffer for raw input data. If the buffer already exists, it will be freed and a
  ///   new buffer will be allocated if the requested size is larger than the current buffer size.
  /// </summary>
  /// <param name="size"> The required size of the buffer. </param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void EnsureRawInputBufferSize(uint size) {
    if (rawInputBuffer == nullptr ||
        rawInputBufferSize < size) {
      if (rawInputBuffer != nint.Zero) {
        Console.WriteLine("Freeing previous buffer.");
        Marshal.FreeHGlobal(rawInputBuffer); // Free previous buffer if it exists
      }

      Console.WriteLine($"Allocating new buffer of size: {size}");
      rawInputBuffer     = Marshal.AllocHGlobal((int)size); // Allocate new buffer
      rawInputBufferSize = size;
    }
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


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static unsafe void ProcessRawInput(LPARAM lParam) {
    uint sizeNeeded = 0;

    // First call to GetRawInputData: retrieves the size of the data.
    GetRawInputData(
      new HRAWINPUT(lParam),
      RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT,
      (void*)0,
      ref sizeNeeded,
      (uint)sizeof(RAWINPUTHEADER)
    );

    EnsureRawInputBufferSize(sizeNeeded); // Ensure the buffer is large enough for the data

    // If there is data in the buffer, process it.
    if (sizeNeeded > 0) {
      // Second call to GetRawInputData: retrieves the data and stores it in the buffer.
      if (GetRawInputData(
            new HRAWINPUT(lParam),
            RAW_INPUT_DATA_COMMAND_FLAGS.RID_INPUT,
            (void*)rawInputBuffer,
            ref sizeNeeded,
            (uint)sizeof(RAWINPUTHEADER)
          ) ==
          sizeNeeded) {
        // Get the raw input data from the buffer as a RAWINPUT struct.
        var rawInput = (RAWINPUT*)rawInputBuffer.ToPointer();

        if (rawInput -> header.dwType == (uint)RawInputMethod.RIM_TYPEMOUSE) {
          // var xPosRelative = rawInput -> data.mouse.lLastX;
          // var yPosRelative = rawInput -> data.mouse.lLastY;

          var absolutePos = GetMousePosition();
          // Signal to the mouse event service that the mouse position has been updated.
          MouseEventService?.UpdateMousePosition(absolutePos.X, absolutePos.Y);
        }
      }
    }
  }


  private static LRESULT SubclassDownscalerWindowProc(
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
      // case Msg.WM_MOUSEMOVE:
      //   Console.WriteLine($"Mouse moved to: ({GET_X_LPARAM(lParam)}, {GET_Y_LPARAM(lParam)})");
      //   break;
      // case Msg.WM_NCMOUSEMOVE:
      //   Console.WriteLine(
      //     $"Mouse moved to non-client area: ({GET_X_LPARAM(lParam)}, {GET_Y_LPARAM(lParam)})"
      //   );
      //   break;
      case Msg.WM_INPUT:
        ProcessRawInput(lParam);
        break;
    }

    return DefSubclassProc(hWnd, msg, wParam, lParam);
  }


  private static LRESULT SubclassSourceWindowProc(
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


  private void InstallEventHandlers() {
    // Install the event handlers into the main window - the downscaler window.
    InstallWindowSubclassForDownscalerWindow(hwnd);

    // Install the event handler into the source window - the window that is being downscaled.
    // InstallWindowSubclassForSourceWindow(hwnd);

    // Listen for raw input from the mouse globally.
    RegisterForRawInput(hwnd);

    // Get all child windows of the main window and install the event handlers into them.
    foreach (var child in EnumerateChildWindowsRecursively(hwnd)) {
      InstallWindowSubclassForDownscalerWindow(child.Hwnd);
      //RegisterForRawInput(child.Hwnd);
    }
  }


  private void InstallWindowSubclassForDownscalerWindow(HWND hwnd) {
    var installSucceeded = SetWindowSubclass(hwnd, SubclassDownscalerWindowProc, 1, nuint.Zero);
    if (!installSucceeded) {
      throw new Win32Exception(Marshal.GetLastWin32Error());
    }
  }


  private void InstallWindowSubclassForSourceWindow(HWND hwnd) {
    var installSucceeded = SetWindowSubclass(hwnd, SubclassSourceWindowProc, 1, nuint.Zero);
    if (!installSucceeded) {
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
