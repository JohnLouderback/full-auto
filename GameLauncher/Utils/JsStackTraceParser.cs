using System.Text.RegularExpressions;

namespace GameLauncher.Utils;

/// <summary>
///   Represents a single frame in a JavaScript stack trace.
/// </summary>
public readonly struct JsStackFrame {
  /// <summary>
  ///   The frame number. Like <c> #1 </c>.
  /// </summary>
  public string Frame { get; init; }

  /// <summary>
  ///   The type of the method, if present. Like <c> MyClass </c>.
  /// </summary>
  public string Type { get; init; }

  /// <summary>
  ///   The method name. Like <c> MyMethod </c>.
  /// </summary>
  public string Method { get; init; }

  /// <summary>
  ///   The parameter list as it appears in the stack trace. Like <c> (arg1, arg2) </c>.
  /// </summary>
  public string ParameterList { get; init; }

  /// <summary>
  ///   The parameters as they appear in the stack trace. Like <c> arg1, arg2 </c>.
  /// </summary>
  public string Parameters { get; init; }

  /// <summary>
  ///   The file name or path. Like <c> launch-example.ts </c>. It may include the directory path and
  ///   additional information like <c> [temp] </c>.
  /// </summary>
  public string File { get; init; }

  /// <summary>
  ///   The line number and column number, if present. Like <c> 3:13 </c>.
  /// </summary>
  public string Line { get; init; }
}

public static class JsStackTraceParser {
  /// <summary>
  ///   Parses a JavaScript stack trace string and yields a sequence of <see cref="JsStackFrame" />
  ///   structs.
  /// </summary>
  /// <param name="stackTrace">The full JS stack trace as a string.</param>
  /// <returns>An enumerable of <see cref="JsStackFrame" />.</returns>
  public static IEnumerable<JsStackFrame> Parse(string stackTrace) {
    if (string.IsNullOrWhiteSpace(stackTrace)) {
      yield break;
    }

    // Split the input into lines.
    var lines      = stackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    var frameIndex = 0;

    // This regex matches a line beginning with "at " and then either:
    //   - a function part followed by a parenthesized location, optionally with trailing extra data (e.g. "-> ..."), or
    //   - a location only.
    // Example matches:
    //   at launch (C:\path\Tasks.js:10:24) -> Object.defineProperty(...)
    //   at (launch-example.ts [temp]:3:19)
    var frameRegex = new Regex(
      @"^at\s+(?:(?<function>[^(]+?)\s*\((?<location>[^)]+)\)(?:\s*->.*)?|(?<location>.+))$",
      RegexOptions.Compiled
    );

    // This regex extracts the file, line, and optional column from the location.
    // It will ignore any trailing extra details.
    // Examples:
    //   C:\path\Tasks.js:10:24
    //   launch-example.ts [temp]:3:19
    var locationRegex = new Regex(
      @"^(?<file>.*?):(?<line>\d+)(?::(?<col>\d+))?(?:\s*->.*)?$",
      RegexOptions.Compiled
    );

    foreach (var line in lines) {
      var trimmed = line.Trim();
      if (!trimmed.StartsWith("at ")) {
        continue;
      }

      var match = frameRegex.Match(trimmed);
      if (!match.Success) {
        continue;
      }

      // Extract the function part (if any) and the location part.
      var funcPart     = match.Groups["function"].Value.Trim();
      var locationPart = match.Groups["location"].Value.Trim();

      // Parse the location into file, line, and column.
      var file     = string.Empty;
      var lineInfo = string.Empty;
      var locMatch = locationRegex.Match(locationPart);
      if (locMatch.Success) {
        file = locMatch.Groups["file"].Value.Trim();
        var lineNumber = locMatch.Groups["line"].Value.Trim();
        var col        = locMatch.Groups["col"].Success ? locMatch.Groups["col"].Value.Trim() : "";
        lineInfo = string.IsNullOrEmpty(col) ? lineNumber : $"{lineNumber}:{col}";
      }
      else {
        file = locationPart;
      }

      // Process the function part to separate type from method and inline parameters.
      var typePart      = string.Empty;
      var method        = string.Empty;
      var parameterList = string.Empty;
      var parameters    = string.Empty;
      if (!string.IsNullOrEmpty(funcPart)) {
        // Look for an inline parameter list.
        var paramStart = funcPart.IndexOf('(');
        if (paramStart != -1) {
          method        = funcPart.Substring(0, paramStart).Trim();
          parameterList = funcPart.Substring(paramStart).Trim();
          // Remove the surrounding parentheses for the Parameters property.
          if (parameterList.StartsWith("(") &&
              parameterList.EndsWith(")")) {
            parameters = parameterList.Substring(1, parameterList.Length - 2).Trim();
          }
        }
        else {
          method = funcPart;
        }

        // If the method contains a dot, assume the part before the last dot is the Type.
        var lastDot = method.LastIndexOf('.');
        if (lastDot != -1) {
          typePart = method.Substring(0, lastDot).Trim();
          method   = method.Substring(lastDot + 1).Trim();
        }
      }

      yield return new JsStackFrame {
        Frame         = $"#{frameIndex++}",
        Type          = typePart,
        Method        = method,
        ParameterList = parameterList,
        Parameters    = parameters,
        File          = file,
        Line          = lineInfo
      };
    }
  }
}
