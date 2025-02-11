using Core.Utils;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

public partial class Window {
  private readonly CancellationTokenSource cancellationToken = new();
  private          bool                    isClosed;

  // Show/hide are repeating events; close is one-shot:
  private WindowEvent showEvent;
  private WindowEvent hideEvent;
  private WindowEvent closeEvent;

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

  /// <summary>
  ///   Resolves each time the window is shown (multiple awaits allowed).
  /// </summary>
  [ScriptMember("shownSignal")]
  public Task ShownSignal {
    get {
      if (IsClosed) {
        return Task.FromCanceled(new CancellationToken(true));
      }

      return showEvent.Signal;
    }
  }

  /// <summary>
  ///   Resolves each time the window is hidden (multiple awaits allowed).
  /// </summary>
  [ScriptMember("hiddenSignal")]
  public Task HiddenSignal {
    get {
      if (IsClosed) {
        return Task.FromCanceled(new CancellationToken(true));
      }

      return hideEvent.Signal;
    }
  }

  /// <summary>
  ///   Resolves once when the window is closed.
  /// </summary>
  [ScriptMember("closeSignal")]
  public Task CloseSignal {
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


  private void InitializeEvents() {
    // Use the same class for all signals, just with different event IDs & multiple flags.
    showEvent = new WindowEvent(hwnd, WinEvent.EVENT_OBJECT_SHOW, true, cancellationToken.Token);
    hideEvent = new WindowEvent(hwnd, WinEvent.EVENT_OBJECT_HIDE, true, cancellationToken.Token);
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

          // Cancel ongoing show/hide monitors
          cancellationToken.Cancel();
        }
      },
      TaskScheduler.Default
    );
  }
}
