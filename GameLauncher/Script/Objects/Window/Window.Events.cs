using Windows.Win32.UI.WindowsAndMessaging;
using Core.Utils;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript;
using static GameLauncher.Script.Utils.JSTypeConverter;

namespace GameLauncher.Script.Objects;

public partial class Window {
  private readonly CancellationTokenSource cancellationToken = new();
  private          bool                    isClosed;

  // The last known state of the window:
  private WindowState lastKnownWindowState;

  // Show/hide are repeating events; close is one-shot:
  private WindowEvent showEvent;
  private WindowEvent hideEvent;
  private WindowEvent closeEvent;
  private WindowEvent minimizeEvent;
  private WindowEvent maximizeEvent;
  private WindowEvent restoreEvent;
  private WindowEvent focusEvent;
  private WindowEvent boundsChangeEvent;

  /// <summary>
  ///   A callback function that is called when a window event occurs. It is passed the window that
  ///   the event occurred on.
  /// </summary>
  public delegate void WindowEventCallback(Window window);

  [ScriptMember("isClosed")]
  public bool IsClosed {
    get {
      if (!isClosed) {
        // If it's not forcibly marked closed, check the OS for safety:
        isClosed = !hwnd.IsWindow();
      }

      return isClosed;
    }
  }

  [ScriptMember("isMinimized")]
  public bool IsMinimized => hwnd.GetWindowPlacement().showCmd == SHOW_WINDOW_CMD.SW_MINIMIZE;

  [ScriptMember("isMaximized")]
  public bool IsMaximized => hwnd.GetWindowPlacement().showCmd == SHOW_WINDOW_CMD.SW_MAXIMIZE;

  [ScriptMember("isShowing")] public bool IsShowing => hwnd.IsWindowVisible();

  [ScriptMember("isFocused")] public bool IsFocused => hwnd.IsForegroundWindow();

  /// <summary>
  ///   Resolves the next time the window is shown.
  /// </summary>
  [ScriptMember("shown")]
  public Task Shown => showEvent.Signal;

  /// <summary>
  ///   Resolves the next time the window is hidden.
  /// </summary>
  [ScriptMember("hidden")]
  public Task Hidden => hideEvent.Signal;

  /// <summary>
  ///   Resolves the next time the window is minimized.
  /// </summary>
  [ScriptMember("minimized")]
  public Task Minimized => minimizeEvent.Signal;

  /// <summary>
  ///   Resolves the next time the window is maximized.
  /// </summary>
  [ScriptMember("maximized")]
  public Task Maximized => maximizeEvent.Signal;

  /// <summary>
  ///   Resolves the next time the window is restored. When the window is "un-minimized."
  /// </summary>
  [ScriptMember("restored")]
  public Task Restored => restoreEvent.Signal;

  [ScriptMember("focused")] public Task Focused => focusEvent.Signal;

  /// <summary>
  ///   Resolves the next time the window's bounds change.
  /// </summary>
  [ScriptMember("boundsChanged")]
  public Task BoundsChanged => boundsChangeEvent.Signal;

  /// <summary>
  ///   Resolves once when the window is closed.
  /// </summary>
  [ScriptMember("closed")]
  public Task Closed {
    get {
      if (IsClosed) {
        // If we discover it's already closed, we can just
        // complete the close event TCS (if not done yet).
        if (closeEvent.Signal.IsCompleted == false) {
          // There's a small nuance: if the task is already completed, this won't do anything.
          // But no harm in calling it.
          closeEvent.ForceComplete();
        }
      }

      return closeEvent.Signal;
    }
  }


  [HideFromTypeScript]
  public void On(string eventName, ScriptObject callback) {
    ArgumentNullException.ThrowIfNull(callback);

    if (IsFunction(callback)) {
      On(eventName, window => callback.Invoke(false, window));
    }
    else {
      throw new ArgumentException("Callback must be a function.");
    }
  }


  /// <summary>
  ///   Binds a callback to an event.
  /// </summary>
  /// <param name="eventName"> The name of the event to bind the callback to. </param>
  /// <param name="callback"> The callback to execute when the event occurs. </param>
  /// <exception cref="ArgumentException">
  ///   Thrown when the event name is not the name of a known event.
  /// </exception>
  [ScriptMember("on")]
  public void On(
    [TsTypeOverride(
      """ "shown" | "hidden" | "minimized" | "maximized" | "restored" | "focused" | "boundsChanged" | "closed" """
    )]
    string eventName,
    WindowEventCallback callback
  ) {
    switch (eventName) {
      case "shown":
      case "hidden":
      case "minimized":
      case "maximized":
      case "restored":
      case "focused":
      case "boundsChanged":
        BindEvent(eventName, callback);
        break;
      case "closed":
        // Close is a special case because it only occurs once.
        Closed.ContinueWith(
          t => {
            if (t.Status == TaskStatus.RanToCompletion) {
              callback(this);
            }
          },
          TaskScheduler.Default
        );
        break;
      default:
        throw new ArgumentException($"Unknown event name: {eventName}");
    }
  }


  /// <summary>
  ///   Ensures that the event remains bound to the signal after every occurrence by rebinding it.
  /// </summary>
  /// <param name="signal">
  ///   The signal to bind the event to. The name maps like:
  ///   <c> shown => Shown, hidden => Hidden, close => Close. </c>
  /// </param>
  /// <param name="callback"> The callback to execute when the event occurs. </param>
  private void BindEvent(string signal, WindowEventCallback callback) {
    // Dynamically get the signal by name.
    var signalTask = GetSignalByName(signal);

    // Ensures that the event remains bound to the signal after every occurrence.
    signalTask.ContinueWith(
      t => {
        if (t.Status == TaskStatus.RanToCompletion) {
          callback(this);
          BindEvent(signal, callback);
        }
      },
      TaskScheduler.Default
    );
  }


  /// <summary>
  ///   Gets the current state of the window.
  /// </summary>
  /// <returns> The current state of the window. </returns>
  private WindowState GetCurrentWindowState() {
    var state = WindowState.NONE;

    if (IsShowing) {
      state |= WindowState.SHOWN;
    }
    else {
      state |= WindowState.HIDDEN;
    }

    if (IsMinimized) {
      state |= WindowState.MINIMIZED;
    }

    if (IsMaximized) {
      state |= WindowState.MAXIMIZED;
    }

    return state;
  }


  /// <summary>
  ///   Gets the signal by name. This is useful for binding events to signals because the signals
  ///   are lazily evaluated and, when they resolve, they create a new TaskCompletionSource. This
  ///   means that the signal will not be the same object every time it resolves.
  /// </summary>
  /// <param name="signalName"> The name of the signal to get. </param>
  /// <returns> The Task represented signal requested. </returns>
  /// <exception cref="ArgumentException"> Thrown when the signal name is not the name of a known signal. </exception>
  private Task GetSignalByName(string signalName) {
    return signalName switch {
      "shown"         => Shown,
      "hidden"        => Hidden,
      "closed"        => Closed,
      "minimized"     => Minimized,
      "maximized"     => Maximized,
      "restored"      => Restored,
      "focused"       => Focused,
      "boundsChanged" => BoundsChanged,
      _               => throw new ArgumentException($"Unknown signal name: {signalName}")
    };
  }


  private void InitializeEvents() {
    lastKnownWindowState = GetCurrentWindowState();

    // Use the same class for all signals, just with different event IDs & multiple flags.
    minimizeEvent = new WindowEvent(
      hwnd,
      WinEvent.EVENT_SYSTEM_MINIMIZESTART,
      true,
      cancellationToken.Token
    );

    maximizeEvent = new WindowEvent(
      hwnd,
      WinEvent.EVENT_OBJECT_LOCATIONCHANGE,
      true,
      cancellationToken.Token,
      // Only trigger if the window is maximized.
      _ => IsMaximized
    );

    restoreEvent = new WindowEvent(
      hwnd,
      WinEvent.EVENT_SYSTEM_MINIMIZEEND,
      true,
      cancellationToken.Token
    );

    focusEvent = new WindowEvent(
      hwnd,
      WinEvent.EVENT_SYSTEM_FOREGROUND,
      true,
      cancellationToken.Token
    );

    boundsChangeEvent = new WindowEvent(
      hwnd,
      WinEvent.EVENT_OBJECT_LOCATIONCHANGE,
      true,
      cancellationToken.Token
    );

    showEvent = new WindowEvent(
      hwnd,
      WinEvent.EVENT_OBJECT_SHOW,
      true,
      cancellationToken.Token
    );

    hideEvent = new WindowEvent(
      hwnd,
      WinEvent.EVENT_OBJECT_HIDE,
      true,
      cancellationToken.Token
    );

    closeEvent = new WindowEvent(
      hwnd,
      WinEvent.EVENT_OBJECT_DESTROY,
      false,
      cancellationToken.Token
    );

    // Whenever close completes, run extra logic in .ContinueWith():
    closeEvent.Signal.ContinueWith(
      t => {
        if (t.Status == TaskStatus.RanToCompletion) {
          // Mark as closed
          isClosed = true;
        }
      },
      TaskScheduler.Default
    );
  }
}
