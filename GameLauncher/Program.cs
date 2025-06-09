using Windows.Win32.UI.HiDpi;
using Core.Utils;
using GameLauncher.Cli;
using GameLauncher.Script.Objects;
using GameLauncher.Services;
using Spectre.Console.Cli;
using static Windows.Win32.PInvoke;

var PerMonitorAwareV2 = new DPI_AWARENESS_CONTEXT(new nint(-4));

// Set the DPI awareness context to DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2. This important for
// Windows APIs to work correctly with high DPI displays. This is required for Windows 10 and later.
SetProcessDpiAwarenessContext(PerMonitorAwareV2);

// Initialize a win 32 message loop.
MessageLoop.Start();

TaskScheduler.UnobservedTaskException += (sender, e) => {
  Console.WriteLine($"[Unobserved Task Exception] {e.Exception}");
  e.SetObserved();
};

AppDomain.CurrentDomain.ProcessExit += (sender, e) => {
  // Perform any necessary cleanup here before the application exits.
  // This is a good place to reverse any undoable results or release resources.
  UndoableResult.ReverseAll().GetAwaiter().GetResult();
  GuiService.Instance.Stop().GetAwaiter().GetResult();
};

var app = new CommandApp<RunCommand>();
return await app.RunAsync(args);
