using Core.Utils;
using GameLauncher.Cli;
using Spectre.Console.Cli;

// Initialize a win 32 message loop.
MessageLoop.Start();

TaskScheduler.UnobservedTaskException += (sender, e) => {
  Console.WriteLine($"[Unobserved Task Exception] {e.Exception}");
  e.SetObserved();
};

var app = new CommandApp<RunCommand>();
return await app.RunAsync(args);
