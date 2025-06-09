using Windows.Win32.Foundation;
using GameLauncher.Forms;

namespace GameLauncher.Services;

/// <summary>
///   Provides GUI-related services for the Game Launcher such as creating and managing
///   windows, dialogs, and other GUI components. This service encapsulates a dedicated
///   single-threaded apartment (STA) thread that hosts a Windows Forms message loop,
///   enabling safe cross-thread invocation of GUI operations from automation or script contexts.
/// </summary>
public class GuiService {
  private static   GuiService? instance;
  private readonly object      syncLock = new();

  private SynchronizationContext? context;
  private Thread?                 thread;
  private Task?                   startupTask;
  private ApplicationContext?     applicationContext;

  /// <summary>
  ///   Gets the singleton instance of the <see cref="GuiService" />.
  /// </summary>
  public static GuiService Instance => instance ??= new GuiService();

  /// <summary>
  ///   Indicates whether the GUI service has been started and is ready to process GUI tasks.
  /// </summary>
  public static bool IsStarted => instance is { thread: not null, context: not null };


  /// <summary>
  ///   Initializes a new instance of the <see cref="GuiService" /> class.
  ///   This constructor is private to enforce singleton usage via <see cref="Instance" />.
  /// </summary>
  private GuiService() {}


  /// <summary>
  ///   Invokes the specified action asynchronously on the GUI thread. This method is safe to call
  ///   from any thread and ensures the delegate runs in the Windows Forms synchronization context.
  /// </summary>
  /// <param name="action">
  ///   The <see cref="Action" /> to execute on the GUI thread.
  /// </param>
  public async Task InvokeAsync(Action action) {
    await EnsureStarted().ConfigureAwait(false);

    context!.Post(_ => action(), state: null);
  }


  /// <summary>
  ///   Displays a fullscreen, borderless matte overlay window on the GUI thread,
  ///   optionally surrounding a specified target window. The window remains on top
  ///   of other windows and is intended for visual framing or focus.
  /// </summary>
  /// <param name="targetWindow">
  ///   The handle (<see cref="HWND" />) of the window to matte. The matte window may be
  ///   used to visually surround or parent this window.
  /// </param>
  /// <param name="color">
  ///   The background color to use for the matte window.
  /// </param>
  public async Task ShowMatteWindow(HWND targetWindow, Color color) {
    await InvokeAsync(
      () => {
        var matte = new MatteForm(targetWindow, color);
        matte.Show();
      }
    );
  }


  /// <summary>
  ///   Stops the GUI service by terminating the message loop and joining the thread.
  /// </summary>
  public async Task Stop() {
    if (thread == null ||
        context == null ||
        applicationContext == null) {
      return;
    }

    var tcs = new TaskCompletionSource();

    context.Post(
      _ => {
        applicationContext.ExitThread();
        tcs.SetResult();
      },
      state: null
    );

    await tcs.Task.ConfigureAwait(false);
    thread.Join();

    lock (syncLock) {
      thread             = null;
      context            = null;
      applicationContext = null;
      startupTask        = null;
    }
  }


  /// <summary>
  ///   Lazily and asynchronously starts the GUI service, ensuring a dedicated
  ///   STA thread with a message loop is available for posting GUI tasks.
  /// </summary>
  private Task EnsureStarted() {
    lock (syncLock) {
      if (startupTask != null) {
        return startupTask;
      }

      var tcs = new TaskCompletionSource();

      thread = new Thread(
        () => {
          Application.EnableVisualStyles();
          Application.SetCompatibleTextRenderingDefault(false);

          var ctx = new WindowsFormsSynchronizationContext();
          SynchronizationContext.SetSynchronizationContext(ctx);
          context = ctx;

          applicationContext = new ApplicationContext();
          tcs.SetResult(); // Signal readiness

          Application.Run(applicationContext);
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
