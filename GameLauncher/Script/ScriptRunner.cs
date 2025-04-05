using GameLauncher.Script.Globals;
using GameLauncher.Script.Objects;
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
  ///   The JavaScript code to be executed.
  /// </summary>
  private string jsScriptContent;


  public ScriptRunner(string scriptPath) {
    this.scriptPath = scriptPath;
    AppState.ScriptEngine = new V8ScriptEngine(
      V8ScriptEngineFlags.EnableTaskPromiseConversion |
      V8ScriptEngineFlags.EnableDebugging /* |
      V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart*/
    );
    AppState.ScriptEngine.DefaultAccess           = ScriptAccess.Full;
    AppState.ScriptEngine.DocumentSettings.Loader = new ScriptDocumentLoader();
  }


  /// <summary>
  ///   Runs the script. If the script requires pre-processing (e.g. TypeScript), it will be
  ///   processed before execution.
  /// </summary>
  public async Task RunScript() {
    ProcessSourceFile();
    InjectGlobals(AppState.ScriptEngine);

    try {
      var obj = AppState.ScriptEngine.Evaluate(
        new DocumentInfo(new Uri(scriptPath)) {
          Category = ModuleCategory.Standard
        },
        jsScriptContent
      );

      // If the script uses top-level await, we need to wait for the promise to resolve before
      // continuing.
      //if (obj is not null and not VoidResult && IsPromise(obj)) {
      if (obj is Task promise) {
        await promise.ToTask();
      }

      // Once the script has completed running, we need to undo any tasks that were executed that
      // were intended to be reversed. For example, if the user sets the screen resolution, we need
      // to restore the original resolution when the script completes (unless they chose to persist
      // the change).
      await UndoableResult.ReverseAll();
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
    Lib.Utils.InjectIntoEngine(engine);
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
