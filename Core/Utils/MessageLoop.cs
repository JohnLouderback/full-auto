using Windows.Win32;
using Windows.Win32.Foundation;

namespace Core.Utils;

using static PInvoke;
using static Macros;

/// <summary>
///   Used for creating a Win32 message loop mechanism. This is useful for running
///   message-based operations such as window creation and manipulation. It is necessary for
///   any event hooks that rely on the message pump to function correctly.
/// </summary>
public static class MessageLoop {
  /// <summary>
  ///   Starts the message pump on a dedicated background thread.
  /// </summary>
  public static void Start() {
    var messagePumpThread = new Thread(
      () => {
        // This loop will continue until a WM_QUIT message is received.
        while (GetMessage(out var msg, (HWND)NULL, 0, 0) != 0) {
          TranslateMessage(ref msg);
          DispatchMessage(ref msg);
        }
      }
    ) {
      IsBackground = true // Ensures the thread does not block process termination.
    };

    messagePumpThread.Start();
  }
}
