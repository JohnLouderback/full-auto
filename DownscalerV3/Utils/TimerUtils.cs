using Microsoft.UI.Dispatching;

namespace DownscalerV3.Utils;

public static class TimerUtils {
  /// <summary>
  ///   Executes an action immediately on the dispatcher queue.
  /// </summary>
  /// <param name="queue"> The dispatcher queue to use. </param>
  /// <param name="action"> The action to execute. </param>
  public static void SetImmediate(this DispatcherQueue queue, Action action) {
    queue.TryEnqueue(() => action());
  }


  /// <summary>
  ///   Creates a timer that executes an action after a delay.
  /// </summary>
  /// <param name="queue"> The dispatcher queue to use. </param>
  /// <param name="action"> The action to execute when the timer has elapsed. </param>
  /// <param name="delay"> The amount of time to wait before executing the action. </param>
  public static void SetTimeout(this DispatcherQueue queue, Action action, TimeSpan delay) {
    var timer = queue.CreateTimer();
    timer.Interval = delay;
    timer.Tick += (sender, e) => {
      action.Invoke();
      timer.Stop(); // Stop the timer after the action is executed
    };
    timer.Start();
  }


  /// <summary>
  ///   Creates a timer that executes an action after a delay.
  /// </summary>
  /// <param name="queue"> The dispatcher queue to use. </param>
  /// <param name="action"> The action to execute when the timer has elapsed. </param>
  /// <param name="delay"> The amount of time in milliseconds to wait before executing the action. </param>
  public static void SetTimeout(this DispatcherQueue queue, Action action, int delay) {
    SetTimeout(queue, action, TimeSpan.FromMilliseconds(delay));
  }
}
