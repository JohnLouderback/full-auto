using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

[TypeScriptExport]
public class LaunchOptions : ObjectBase {
  /// <summary>
  ///   Whether the application's standard output should be redirected. This is useful for capturing
  ///   the output of the application for logging or debugging purposes.
  /// </summary>
  [ScriptMember("redirectStdOut")]
  public bool RedirectStdOut { get; set; } = false;

  /// <summary>
  ///   Whether the application's standard error should be redirected. This is useful for capturing
  ///   the error output of the application for logging or debugging purposes.
  /// </summary>
  [ScriptMember("redirectStdErr")]
  public bool RedirectStdErr { get; set; } = false;


  public static implicit operator LaunchOptions(ScriptObject obj) {
    if (JSTypeConverter.MatchesShape<LaunchOptions>(obj, out var errors)) {
      return JSTypeConverter.ConvertTo<LaunchOptions>(obj);
    }

    throw new ScriptEngineException(
      "Could not convert to LaunchOptions due to conversion errors:\n  " +
      string.Join("\n  ", errors)
    );
  }
}
