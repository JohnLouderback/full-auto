using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;

public delegate bool WindowCriteria(HWND hwnd);

/// <summary>
///   A utility class for awaiting window events using the Win32 API.
/// </summary>
public static class WinEventAwaiter {
  // Refers to the hwnd of the window that is the source of the event, but not any of its child objects.
  private const int CHILDID_SELF = 0;
  private const int OBJID_WINDOW = 0;

  // Constants for the hook and message loop.
  private const uint EVENT_MIN             = 0x00000001;
  private const uint EVENT_MAX             = 0x7FFFFFFF;
  private const uint WINEVENT_OUTOFCONTEXT = 0;
  private const uint WM_QUIT               = 0x0012;

  // The hook handle.
  private static HWINEVENTHOOK hook = HWINEVENTHOOK.Null;

  // A dedicated thread for the hook message loop.
  private static readonly Thread hookThread;

  // The native thread ID for the hook thread.
  private static uint hookThreadId;

  // Lock for protecting the list of pending awaiters.
  private static readonly object @lock = new();

  // The list of pending awaiters.
  private static readonly List<AwaiterEntry> awaiters = new();

  // The static delegate that remains pinned.
  private static readonly WINEVENTPROC winEventProc = WinEventProc;


  // In the static constructor we start the hook thread.
  static WinEventAwaiter() {
    hookThread = new Thread(HookAndPumpMessages) {
      IsBackground = true
    };
    // Set the thread to STA (required for many UI hook scenarios)
    hookThread.SetApartmentState(ApartmentState.STA);
    hookThread.Start();
  }


  /// <summary>
  ///   Registers a criteria callback and returns a task that will complete with the HWND
  ///   when an event matching that criteria occurs. If a timeout occurs, the task completes with null.
  /// </summary>
  /// <param name="events">
  ///   The list of events to wait for. If any event in this list occurs and matches the criteria,
  ///   the task will complete. See: <see cref="Core.Utils.WinEvent" /> for possible values.
  /// </param>
  /// <param name="criteria">
  ///   The criteria callback that determines if window from the event is the one we're waiting for.
  /// </param>
  /// <param name="timeout">
  ///   The maximum time to wait for the event to occur. If <c> null </c>, the method waits
  ///   indefinitely.
  /// </param>
  /// <param name="cancellationToken">
  ///   A cancellation token that can be used to cancel the operation.
  /// </param>
  /// <returns>
  ///   A task that will complete with the HWND of the window that matches the criteria, or
  ///   <see langword="null" /> if the timeout elapsed.
  /// </returns>
  public static Task<HWND?> AwaitEvent(
    IEnumerable<uint> events,
    WindowCriteria criteria,
    TimeSpan? timeout = null,
    CancellationToken cancellationToken = default
  ) {
    ArgumentNullException.ThrowIfNull(criteria);

    var entry = new AwaiterEntry(criteria, events);
    lock (@lock) {
      awaiters.Add(entry);
    }

    // Create a delay task (or never-ending task if no timeout is specified)
    var delayTask = timeout.HasValue
                      ? Task.Delay(timeout.Value, cancellationToken)
                      : Task.Delay(Timeout.Infinite, cancellationToken);

    // When either the criteria completes or the delay elapses, complete the returned task.
    return Task.WhenAny(entry.Tcs.Task, delayTask)
      .ContinueWith(
        t => {
          if (t.Result == entry.Tcs.Task) {
            return entry.Tcs.Task.Result;
          }

          // Timeout (or cancellation) occurred: remove the entry so it no longer waits.
          lock (@lock) {
            awaiters.Remove(entry);
          }

          return (HWND?)null;
        },
        cancellationToken
      );
  }


  /// <summary>
  ///   The dedicated hook thread registers the global event hook and runs a message loop.
  /// </summary>
  private static void HookAndPumpMessages() {
    hookThreadId = GetCurrentThreadId();

    hook = SetWinEventHook(
      EVENT_MIN,
      EVENT_MAX,
      (HMODULE)nint.Zero,
      winEventProc,
      0, // monitor all processes
      0, // monitor all threads
      WINEVENT_OUTOFCONTEXT
    );

    if (hook == nint.Zero) {
      Console.Error.WriteLine("Failed to set event hook.");
      return;
    }

    // Standard message loop. This is necessary for the hook to work as it relies on the message
    // pump.
    MSG msg;
    while (GetMessage(out msg, (HWND)nint.Zero, 0, 0) != 0) {
      TranslateMessage(ref msg);
      DispatchMessage(ref msg);
    }
  }


  /// <summary>
  ///   The hook callback that fires for any event in the specified range.
  ///   It iterates over all pending awaiters and completes any whose criteria match.
  /// </summary>
  private static void WinEventProc(
    HWINEVENTHOOK hWinEventHook,
    uint @event,
    HWND hWnd,
    int idObject,
    int idChild,
    uint idEventThread,
    uint dwmsEventTime
  ) {
    if (hWnd == nint.Zero ||
        idChild != CHILDID_SELF ||
        idObject != OBJID_WINDOW) {
      return;
    }

    // Check every registered awaiter.
    lock (@lock) {
      // Iterate backwards to allow removal.
      for (var i = awaiters.Count - 1; i >= 0; i--) {
        var entry = awaiters[i];
        try {
          if (entry.Events.Contains(@event) &&
              entry.Criteria(hWnd)) {
            entry.Tcs.TrySetResult(hWnd);
            awaiters.RemoveAt(i);
          }
        }
        catch (Exception ex) {
          // In case the criteria callback throws.
          entry.Tcs.TrySetException(ex);
          awaiters.RemoveAt(i);
        }
      }
    }
  }


  /// <summary>
  ///   Represents an awaiting registration.
  /// </summary>
  private class AwaiterEntry {
    public WindowCriteria             Criteria { get; }
    public TaskCompletionSource<HWND> Tcs      { get; }

    /// <summary>
    ///   The list of events to wait for. If any event in this list occurs and matches the criteria,
    ///   the task will complete. See: <see cref="Core.Utils.WinEvent" /> for possible values.
    /// </summary>
    public IEnumerable<uint> Events { get; }


    public AwaiterEntry(WindowCriteria criteria, IEnumerable<uint> events) {
      Criteria = criteria;
      Events   = events;
      // Using RunContinuationsAsynchronously to avoid potential deadlocks.
      Tcs = new TaskCompletionSource<HWND>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
  }
}
