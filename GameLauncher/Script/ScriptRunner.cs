using GameLauncher.Script.Globals;
using GameLauncher.Utils;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using Task = System.Threading.Tasks.Task;
using TypeScriptCompiler = GameLauncher.TypeScript.Compiler;
using static GameLauncher.Script.Utils.ErrorUtils;

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
    engine = new V8ScriptEngine(
      V8ScriptEngineFlags.EnableTaskPromiseConversion |
      V8ScriptEngineFlags.EnableDebugging /* |
      V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart*/
    );
    engine.DefaultAccess           = ScriptAccess.Full;
    engine.DocumentSettings.Loader = new ScriptDocumentLoader();
  }


  /// <summary>
  ///   Runs the script. If the script requires pre-processing (e.g. TypeScript), it will be
  ///   processed before execution.
  /// </summary>
  public async Task RunScript() {
    ProcessSourceFile();
    InjectGlobals(engine);

    try {
      await engine.Evaluate(
          new DocumentInfo(scriptPath) {
            Category = ModuleCategory.Standard
          },
          jsScriptContent
        )
        .ToTask();
    }
    catch (Exception exception) {
      if (exception is IScriptEngineException scriptException) {
        // AnsiConsole.WriteException(exception);
        Logger.Exception(
          CleanStackTrace(scriptException.ErrorDetails)
        );
        //Console.WriteLine(CleanStackTrace(scriptException.ErrorDetails));
      }
      else {
        throw;
      }
    }
  }


  /// <summary>
  ///   Gets and stores the JavaScript code from a TypeScript source file.
  /// </summary>
  private void GetJSFromTypeScript() {
    jsScriptContent = new TypeScriptCompiler().Compile(scriptPath);
  }


  /// <summary>
  ///   Injects global functions into the given engine.
  /// </summary>
  /// <param name="engine"></param>
  private void InjectGlobals(V8ScriptEngine engine) {
    PromiseRejectionHandler.InjectIntoEngine(engine);
    ConsoleWrapper.InjectIntoEngine(engine);
    Timers.InjectIntoEngine(engine);
    Tasks.InjectIntoEngine(engine);
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
