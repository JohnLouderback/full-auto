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

    // Split the trace into lines, ignoring empty lines.
    var lines      = stackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    var frameIndex = 0;

    foreach (var line in lines) {
      var trimmed = line.Trim();
      // Skip lines that do not begin with "at "
      if (!trimmed.StartsWith("at ")) {
        continue;
      }

      // Remove the "at " prefix.
      var content      = trimmed.Substring(3).Trim();
      var funcPart     = string.Empty;
      var locationPart = string.Empty;

      // Check if the line contains a function part followed by a parenthesized location.
      var parenIndex = content.IndexOf(" (");
      if (parenIndex != -1) {
        funcPart = content.Substring(0, parenIndex).Trim();

        // Expect the location to be enclosed in parentheses.
        var endParen = content.LastIndexOf(')');
        if (endParen > parenIndex) {
          locationPart = content.Substring(parenIndex + 2, endParen - parenIndex - 2).Trim();
        }
      }
      else {
        // No function information; assume the entire content is the location.
        locationPart = content;
      }

      // Updated regex:
      // Matches patterns like "launch-example.ts [temp]:3:13" and also those with trailing extra data like "-> Object.defineProperty(…)"
      // The extra info is captured by a non-capturing group (?:\s*->.*)? and then ignored.
      var locationMatch = Regex.Match(
        locationPart,
        @"^(?<file>.*?):(?<line>\d+)(?::(?<col>\d+))?(?:\s*->.*)?$"
      );

      var file     = string.Empty;
      var lineInfo = string.Empty;
      if (locationMatch.Success) {
        file = locationMatch.Groups["file"].Value.Trim();
        var lineNumber = locationMatch.Groups["line"].Value.Trim();
        var col = locationMatch.Groups["col"].Success
                    ? locationMatch.Groups["col"].Value.Trim()
                    : "";
        lineInfo = string.IsNullOrEmpty(col) ? lineNumber : $"{lineNumber}:{col}";
      }
      else {
        // If the location doesn’t match the expected pattern, assign it entirely to the file.
        file = locationPart;
      }

      // Process the function part to extract method name and an inline parameter list if present.
      var typePart      = string.Empty;
      var method        = string.Empty;
      var parameterList = string.Empty;
      var parameters    = string.Empty;

      if (!string.IsNullOrEmpty(funcPart)) {
        // Look for a parameter list within the function part.
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

        // If the method contains a dot, split the type from the method name.
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
