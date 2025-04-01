using Windows.Win32.UI.HiDpi;
using Core.Utils;
using GameLauncher.Cli;
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

var app = new CommandApp<RunCommand>();
return await app.RunAsync(args);
