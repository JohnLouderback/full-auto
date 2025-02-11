using Windows.Win32.Foundation;

namespace GameLauncher.Script.Objects;

public class WindowEvent {
  private readonly HWND              _hwnd; // The window handle we care about
  private readonly uint              _eventId; // WinEvent constant (e.g., EVENT_OBJECT_SHOW)
  private readonly bool              _allowMultiple; // True => re-arm after each event
  private readonly CancellationToken _token; // Used to cancel if window closes

  // We store a “current” TCS for this event, re-creating it each time we fulfill it (if multiple).
  private Lazy<TaskCompletionSource> _currentTcs;
  private bool                       _monitoring;

  /// <summary>
  ///   Returns a Task that completes when this event occurs.
  ///   If <see cref="allowMultiple" /> is true, you can await this repeatedly for multiple occurrences.
  /// </summary>
  public Task Signal {
    get {
      // If you want to handle the case “the window is already closed => never occurs,”
      // you'd pass in a 'canceled' task. But typically, the Window class will handle that.
      EnsureMonitoringStarted();
      return _currentTcs.Value.Task;
    }
  }


  /// <summary>
  ///   Creates a new WindowEvent for a specific WinEvent ID.
  /// </summary>
  /// <param name="hwnd">Window to monitor.</param>
  /// <param name="eventId">WinEvent constant (e.g. EVENT_OBJECT_SHOW).</param>
  /// <param name="allowMultiple">
  ///   If true, once the event is triggered, we reset the TCS so you can await it again.
  /// </param>
  /// <param name="token">
  ///   Cancellation token for stopping the monitor if the window closes or is otherwise canceled.
  /// </param>
  public WindowEvent(HWND hwnd, uint eventId, bool allowMultiple, CancellationToken token) {
    _hwnd          = hwnd;
    _eventId       = eventId;
    _allowMultiple = allowMultiple;
    _token         = token;
    _currentTcs    = CreateNewTcs();
  }


  /// <summary>
  ///   Completes the event immediately, regardless of whether it has actually occurred. This is
  ///   useful if we need to signal from outside the event that it has occurred through other
  ///   means.
  /// </summary>
  public void ForceComplete() {
    _currentTcs.Value.TrySetResult();
  }


  private static Lazy<TaskCompletionSource> CreateNewTcs() {
    return new Lazy<TaskCompletionSource>(
      () => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)
    );
  }


  /// <summary>
  ///   Actually starts listening for the WinEvent. We only do this once.
  /// </summary>
  private void EnsureMonitoringStarted() {
    if (!_monitoring) {
      _monitoring = true;
      MonitorEventAsync(); // Fire-and-forget
    }
  }


  private async void MonitorEventAsync() {
    while (!_token.IsCancellationRequested) {
      var result = await WinEventAwaiter.AwaitEvent(
                     new[] { _eventId },
                     hWnd => hWnd == _hwnd,
                     null,
                     _token
                   );

      // If the result is null => canceled/timeout => exit
      if (result == null) return;

      // We got the WinEvent => complete the TCS
      _currentTcs.Value.TrySetResult();

      if (_allowMultiple) {
        // Re-arm for the next occurrence
        _currentTcs = CreateNewTcs();
      }
      else {
        // One-shot => done
        return;
      }
    }
  }
}
