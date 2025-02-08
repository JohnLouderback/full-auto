using Core.Utils;
using GameLauncher.Script;
using Microsoft.ClearScript.V8;

// Initialize a win 32 message loop.
MessageLoop.Start();

TaskScheduler.UnobservedTaskException += (sender, e) => {
  Console.WriteLine($"[Unobserved Task Exception] {e.Exception}");
  e.SetObserved();
};

using var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);

var exampleScriptPath = Path.Combine(
  AppContext.BaseDirectory,
  "ExampleScripts",
  "launch-example.ts"
);
var scriptRunner = new ScriptRunner(exampleScriptPath);
await scriptRunner.RunScript();
