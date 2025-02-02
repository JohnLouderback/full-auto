using System.Text.RegularExpressions;

namespace GameLauncher.Utils;

public readonly struct JsStackFrame {
  public string Frame         { get; init; }
  public string Type          { get; init; }
  public string Method        { get; init; }
  public string ParameterList { get; init; }
  public string Parameters    { get; init; }
  public string File          { get; init; }
  public string Line          { get; init; }
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

    // Split the trace into lines (ignoring empty lines).
    var lines      = stackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    var frameIndex = 0;

    foreach (var line in lines) {
      var trimmed = line.Trim();
      // Typically, frame lines start with "at ".
      if (!trimmed.StartsWith("at ")) {
        // Skip any lines that are not frame lines (for example, the error message)
        continue;
      }

      // Remove the "at " prefix.
      var content      = trimmed.Substring(3).Trim();
      var funcPart     = string.Empty;
      var locationPart = string.Empty;

      // If the line contains a function part, it will be followed by a space and a parenthesized location.
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

      // Extract file, line and optional column from the location.
      // Matches patterns like "file.js:10:15" or "file.js:10".
      var file     = string.Empty;
      var lineInfo = string.Empty;
      var locationMatch = Regex.Match(
        locationPart,
        @"^(?<file>.*?):(?<line>\d+)(?::(?<col>\d+))?$"
      );
      if (locationMatch.Success) {
        file = locationMatch.Groups["file"].Value;
        var lineNumber = locationMatch.Groups["line"].Value;
        var col = locationMatch.Groups["col"].Success ? locationMatch.Groups["col"].Value : "";
        lineInfo = string.IsNullOrEmpty(col) ? lineNumber : $"{lineNumber}:{col}";
      }
      else {
        // If the location doesn’t match the expected pattern, assign it entirely to the file.
        file = locationPart;
      }

      // Process the function part to extract method name and (if present) a parameter list.
      var typePart      = string.Empty;
      var method        = string.Empty;
      var parameterList = string.Empty;
      var parameters    = string.Empty;

      if (!string.IsNullOrEmpty(funcPart)) {
        // Sometimes the function name may itself include a parameter list, e.g. "myFunc(arg1, arg2)".
        var paramStart = funcPart.IndexOf('(');
        if (paramStart != -1) {
          method        = funcPart.Substring(0, paramStart).Trim();
          parameterList = funcPart.Substring(paramStart).Trim();
          // Strip the surrounding parentheses for the Parameters property.
          if (parameterList.StartsWith("(") &&
              parameterList.EndsWith(")")) {
            parameters = parameterList.Substring(1, parameterList.Length - 2).Trim();
          }
        }
        else {
          method = funcPart;
        }

        // If the method name contains a dot, assume that the part before the last dot is the Type.
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
