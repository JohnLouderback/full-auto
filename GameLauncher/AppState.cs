using Microsoft.ClearScript.V8;

namespace GameLauncher;

/// <summary>
///   Represents the global shared state of the application. This class is used to store global
///   state that can be accessed from anywhere in the application, such as the script engine used for
///   executing JavaScript code at runtime.
/// </summary>
public static class AppState {
  /// <summary>
  ///   The ClearScript engine used for executing JavaScript code at runtime. Note: The compiler
  ///   uses a different engine for compiling TypeScript files.
  /// </summary>
  public static V8ScriptEngine ScriptEngine { get; set; }
}
