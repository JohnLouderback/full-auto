using GameLauncher.Script;
using Microsoft.ClearScript.V8;

using var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);

var exampleScriptPath = Path.Combine(AppContext.BaseDirectory, "ExampleScripts", "test1.ts");
var scriptRunner      = new ScriptRunner(exampleScriptPath);
await scriptRunner.RunScript();
