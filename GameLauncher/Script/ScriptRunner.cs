using GameLauncher.Script.Globals;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;

namespace GameLauncher.Script;

public class ScriptRunner {
  /// <summary>
  ///   The path to the script file.
  /// </summary>
  private readonly string scriptPath;

  /// <summary>
  ///   The ClearScript engine used to run the script.
  /// </summary>
  private readonly V8ScriptEngine engine;

  /// <summary>
  ///   The JavaScript code to be executed.
  /// </summary>
  private string jsScriptContent;


  public ScriptRunner(string scriptPath) {
    this.scriptPath = scriptPath;
    engine          = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);
  }


  /// <summary>
  ///   Runs the script. If the script requires pre-processing (e.g. TypeScript), it will be
  ///   processed before execution.
  /// </summary>
  public async Task RunScript() {
    ProcessSourceFile();
    InjectGlobals(engine);
    await engine.Evaluate(
        new DocumentInfo {
          Category = ModuleCategory.Standard
        },
        jsScriptContent
      )
      .ToTask();
  }


  /// <summary>
  ///   Gets and stores the JavaScript code from a TypeScript source file.
  /// </summary>
  private void GetJSFromTypeScript() {
    var tsPath = Path.Combine(AppContext.BaseDirectory, "TypeScript", "typescript.js");
    var tsCode = File.ReadAllText(tsPath);
    // The TypeScript compiler will use its own engine for execution.
    using var tsEngine = new V8ScriptEngine();

    tsEngine.Execute(tsCode);

    var compileTsCode = @"
      function compileTypeScript(source) {
        return ts.transpileModule(source, { compilerOptions: {
          module: ts.ModuleKind.ESNext,
          target: ts.ScriptTarget.ESNext
        }}).outputText;
      }
    ";
    tsEngine.Execute(compileTsCode);

    var tsSource = File.ReadAllText(scriptPath);
    jsScriptContent = tsEngine.Script.compileTypeScript(tsSource);
  }


  /// <summary>
  ///   Injects global functions into the given engine.
  /// </summary>
  /// <param name="engine"></param>
  private void InjectGlobals(V8ScriptEngine engine) {
    ConsoleWrapper.InjectIntoEngine(engine);
    Timers.InjectIntoEngine(engine);
  }


  /// <summary>
  ///   Extracts the JavaScript code from the source file. If the file is a TypeScript
  ///   file, it will be compiled to JavaScript first.
  /// </summary>
  private void ProcessSourceFile() {
    // If the script is a TypeScript file, compile it to JavaScript.
    if (scriptPath.EndsWith(".ts")) {
      GetJSFromTypeScript();
    }
    // Otherwise, read the JavaScript code directly from the file.
    else {
      jsScriptContent = File.ReadAllText(scriptPath);
    }
  }
}
