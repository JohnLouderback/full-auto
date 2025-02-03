using Microsoft.ClearScript.V8;

namespace GameLauncher.Script.Async;

/// <summary>
///   The host event loop that drives asynchronous callbacks (including promise continuations).
/// </summary>
public class EventLoop : IDisposable {
  private readonly V8ScriptEngine          engine;
  private readonly AsyncQueue<Func<Task>>  taskQueue = new();
  private readonly CancellationTokenSource cts       = new();
  private          bool                    isDone;


  public EventLoop(V8ScriptEngine engine) {
    this.engine = engine ?? throw new ArgumentNullException(nameof(engine));
  }


  public void Dispose() {
    SignalDone();
    cts.Dispose();
    engine.Dispose();
  }


  /// <summary>
  ///   Runs the event loop until SignalDone is called or cancellation is requested.
  /// </summary>
  public async Task RunAsync() {
    try {
      while (!cts.Token.IsCancellationRequested &&
             !isDone) {
        // Wait for the next scheduled callback.
        var callback = await taskQueue.DequeueAsync(cts.Token).ConfigureAwait(false);
        try {
          await callback().ConfigureAwait(false);
        }
        catch (Exception ex) {
          Console.Error.WriteLine("Error executing scheduled event: " + ex);
        }
      }
    }
    catch (OperationCanceledException) {
      // Expected when cancellation occurs.
    }
  }


  /// <summary>
  ///   Schedules a callback (represented as a Func&lt;Task&gt;) to run on the event loop.
  /// </summary>
  public void ScheduleEvent(Func<Task> callback) {
    if (callback == null) throw new ArgumentNullException(nameof(callback));
    taskQueue.Enqueue(callback);
  }


  /// <summary>
  ///   Schedules a callback to run after the specified delay.
  /// </summary>
  public void SetTimeout(int delayMilliseconds, Func<Task> callback) {
    if (callback == null) throw new ArgumentNullException(nameof(callback));
    Task.Delay(delayMilliseconds, cts.Token)
      .ContinueWith(
        t => {
          if (!t.IsCanceled) {
            ScheduleEvent(callback);
          }
        },
        TaskScheduler.Default
      );
  }


  /// <summary>
  ///   Signals the event loop to stop.
  /// </summary>
  public void SignalDone() {
    isDone = true;
    cts.Cancel();
  }
}
