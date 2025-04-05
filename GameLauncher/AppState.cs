using Microsoft.ClearScript.V8;

namespace GameLauncher;

public static class AppState {
  /// <summary>
  ///   The ClearScript engine used for executing JavaScript code at runtime. Note: The compiler
  ///   uses a different engine for compiling TypeScript files.
  /// </summary>
  public static V8ScriptEngine ScriptEngine { get; set; }
}
