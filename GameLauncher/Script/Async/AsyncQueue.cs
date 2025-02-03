namespace GameLauncher.Script.Async;

/// <summary>
///   A lightweight asynchronous queue. Producers enqueue items and consumers await items as they
///   become available.
/// </summary>
public class AsyncQueue<T> {
  private readonly Queue<T>                       _queue   = new();
  private readonly Queue<TaskCompletionSource<T>> _waiters = new();


  public Task<T> DequeueAsync(CancellationToken cancellationToken = default) {
    lock (_queue) {
      if (_queue.Count > 0) {
        return Task.FromResult(_queue.Dequeue());
      }

      var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
      if (cancellationToken != default) {
        cancellationToken.Register(() => tcs.TrySetCanceled());
      }

      _waiters.Enqueue(tcs);
      return tcs.Task;
    }
  }


  public void Enqueue(T item) {
    lock (_queue) {
      if (_waiters.Count > 0) {
        var waiter = _waiters.Dequeue();
        waiter.SetResult(item);
      }
      else {
        _queue.Enqueue(item);
      }
    }
  }
}
