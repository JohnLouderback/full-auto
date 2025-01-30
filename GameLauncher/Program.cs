using GameLauncher.Script.Globals;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using Console = System.Console;

using var engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);

// Load TypeScript compiler from output folder
var tsPath            = Path.Combine(AppContext.BaseDirectory, "TypeScript", "typescript.js");
var exampleScriptPath = Path.Combine(AppContext.BaseDirectory, "ExampleScripts", "test.ts");
var tsCode            = File.ReadAllText(tsPath);
engine.Execute(tsCode);

// Load compile function
var compileTsCode = @"
    function compileTypeScript(source) {
        return ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.CommonJS } }).outputText;
    }
";
engine.Execute(compileTsCode);

// Test TypeScript compilation
var tsSource   = File.ReadAllText(exampleScriptPath);
var compiledJs = engine.Script.compileTypeScript(tsSource);

Console.WriteLine("Compiled JavaScript:\n" + compiledJs);

// Inject global functions

// Inject console for conole.log, console.warn, console.error, etc.
ConsoleWrapper.InjectIntoEngine(engine);

// Inject timers for setTimeout, clearTimeout, setInterval, clearInterval, etc.
Timers.InjectIntoEngine(engine);

// Execute the compiled JavaScript code
await engine.Evaluate(
  new DocumentInfo {
    Category = ModuleCategory.Standard
  },
  compiledJs
);
