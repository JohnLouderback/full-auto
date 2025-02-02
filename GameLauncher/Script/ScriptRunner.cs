using GameLauncher.Script.Globals;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;
using TypeScriptCompiler = GameLauncher.TypeScript.Compiler;

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
    engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion);
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
        Console.WriteLine(CleanStackTrace(scriptException.ErrorDetails));
      }
      else {
        throw;
      }
    }
  }


  /// <summary>
  ///   Remove extraneous stack trace lines from the error details that would not be useful
  ///   to an end user.
  /// </summary>
  /// <param name="stack"> The stack trace to clean. </param>
  /// <returns> The stack trace with extraneous lines removed. </returns>
  private string CleanStackTrace(string stack) {
    var lines = stack.Split('\n');
    var filtered = lines.Where(
      line => !line.Contains("at tryInvoke") && !line.Contains("at V8ScriptEngine")
    );
    return string.Join('\n', filtered);
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
