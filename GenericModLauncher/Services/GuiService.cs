using System.Windows;
using System.Windows.Threading;
using GenericModLauncher.Models;
using GenericModLauncher.ViewModels;

namespace GenericModLauncher.Services;

/// <summary>
///   Provides GUI-related services for WPF, hosting a persistent STA thread
///   and message loop that allows programmatic invocation and display of WPF windows.
/// </summary>
public sealed class GuiService {
  private static   GuiService? instance;
  private readonly object      syncLock = new();

  private Thread?               thread;
  private Dispatcher?           dispatcher;
  private Task?                 startupTask;
  private TaskCompletionSource? shutdownTcs;

  /// <summary>
  ///   Gets the singleton instance of the <see cref="GuiService" />.
  /// </summary>
  public static GuiService Instance => instance ??= new GuiService();

  /// <summary>
  ///   Indicates whether the GUI service has been started and is ready to process GUI tasks.
  /// </summary>
  public static bool IsStarted => instance is { dispatcher: not null, thread: not null };


  /// <summary>
  ///   Initializes a new instance of the <see cref="GuiService" /> class.
  ///   This constructor is private to enforce singleton usage via <see cref="Instance" />.
  /// </summary>
  private GuiService() {}


  /// <summary>
  ///   Invokes the specified action on the WPF dispatcher thread.
  /// </summary>
  public async Task InvokeAsync(Action action) {
    await EnsureStarted().ConfigureAwait(false);
    dispatcher!.InvokeAsync(action);
  }


  /// <summary>
  ///   Invokes the specified function on the WPF dispatcher thread and returns the result.
  /// </summary>
  public async Task<T> InvokeAsync<T>(Func<T> func) {
    await EnsureStarted().ConfigureAwait(false);
    var tcs = new TaskCompletionSource<T>();

    dispatcher!.InvokeAsync(
      () => {
        try {
          var result = func();
          tcs.SetResult(result);
        }
        catch (Exception ex) {
          tcs.SetException(ex);
        }
      }
    );

    return await tcs.Task.ConfigureAwait(false);
  }


  /// <summary>
  ///   Shows a new instance of the <see cref="ModLauncher" /> window.
  /// </summary>
  /// <returns>
  ///   The mod launcher window that was shown.
  /// </returns>
  public async Task<ModLauncher> ShowModLauncher(ILauncherConfiguration config) {
    await EnsureStarted().ConfigureAwait(false);
    var tcs = new TaskCompletionSource<ModLauncher>();

    dispatcher!.InvokeAsync(
      () => {
        var window = new ModLauncher(new LauncherViewModel(config));
        window.Show();
        tcs.SetResult(window);
      }
    );

    return await tcs.Task.ConfigureAwait(false);
  }


  /// <summary>
  ///   Stops the GUI service by terminating the dispatcher loop and joining the thread.
  /// </summary>
  public async Task Stop() {
    if (thread == null ||
        dispatcher == null) {
      return;
    }

    shutdownTcs = new TaskCompletionSource();

    dispatcher.BeginInvoke(
      () => {
        dispatcher.InvokeShutdown();
        shutdownTcs.SetResult(); // ensures .Task completes
      }
    );

    await shutdownTcs.Task.ConfigureAwait(false);
    thread.Join();

    lock (syncLock) {
      thread      = null;
      dispatcher  = null;
      startupTask = null;
      shutdownTcs = null;
    }
  }


  /// <summary>
  ///   Starts the WPF message loop if not already started.
  ///   Ensures that the dispatcher is available for invoking UI operations.
  /// </summary>
  private Task EnsureStarted() {
    lock (syncLock) {
      if (startupTask != null) {
        return startupTask;
      }

      var tcs = new TaskCompletionSource();

      thread = new Thread(
        () => {
          var app = new Application {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
          };

          dispatcher = Dispatcher.CurrentDispatcher;
          tcs.SetResult(); // Signal ready
          Dispatcher.Run(); // Start message loop

          shutdownTcs?.SetResult(); // Called on Dispatcher shutdown
        }
      );

      thread.SetApartmentState(ApartmentState.STA);
      thread.IsBackground = true;
      thread.Start();

      startupTask = tcs.Task;
      return startupTask;
    }
  }
}
