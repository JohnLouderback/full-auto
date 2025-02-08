using Microsoft.ClearScript;

namespace GameLauncher.Script.Utils;

public static class ErrorUtils {
  /// <summary>
  ///   Remove extraneous stack trace lines from the error details that would not be useful
  ///   to an end user.
  /// </summary>
  /// <param name="stack"> The stack trace to clean. </param>
  /// <returns> The stack trace with extraneous lines removed. </returns>
  [ScriptMember("cleanStackTrace")]
  public static string CleanStackTrace(string stack) {
    var lines = stack.Split('\n');
    var filtered = lines.Where(
      line => !line.Contains("at tryInvoke") &&
              !line.Contains("at V8ScriptEngine") &&
              !line.Contains("<anonymous>")
    );
    return string.Join('\n', filtered);
  }
}
