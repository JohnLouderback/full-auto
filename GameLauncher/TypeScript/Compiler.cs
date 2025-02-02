using GameLauncher.Script;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using Microsoft.ClearScript.V8;

namespace GameLauncher.TypeScript;

public class Compiler {
  /// <summary>
  ///   The ClearScript engine used for compiling TypeScript files.
  /// </summary>
  private readonly V8ScriptEngine engine = new(
    V8ScriptEngineFlags.EnableDynamicModuleImports |
    V8ScriptEngineFlags.EnableDebugging /*|
    V8ScriptEngineFlags.AwaitDebuggerAndPauseOnStart*/
  );


  public Compiler() {
    engine.DocumentSettings.AccessFlags |= DocumentAccessFlags.EnableFileLoading |
                                           DocumentAccessFlags.UseAsyncLoadCallback;
    engine.DocumentSettings.Loader = new CompilerDocumentLoader();
  }


  public string Compile(string scriptPath) {
    var tsPath = Path.Combine(AppContext.BaseDirectory, "TypeScript", "typescript.js");
    var tsCode = File.ReadAllText(tsPath);

    var compilerHost = new CompilerHost();
    engine.AddHostObject("compilerHost", compilerHost);

    var compileTsCode =
      tsCode +
      """
      // noinspection JSUnresolvedReference,JSUnusedLocalSymbols
      var compilerOptions;
      try {
        compilerOptions = ts.parseJsonConfigFileContent(require('./tsconfig.json'), compilerHost, './tsconfig.json');
      } catch (e) {
        throw e;
      }

      globalThis.compileTypeScript = (source) => {
        try {
          return ts.transpileModule(source, { compilerOptions: compilerOptions.options }).outputText;
        } catch (e) {
          throw e;
        }
      }
      """;

    engine.Execute(
      new DocumentInfo {
        Category = ModuleCategory.CommonJS
      },
      compileTsCode
    );

    var tsSource = File.ReadAllText(scriptPath);
    return engine.Script.compileTypeScript(tsSource);
  }
}
