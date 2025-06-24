using System.Text;

namespace Core.Utils;

public static class StringUtils {
  /// <summary>
  ///   Replaces any sequence of newline characters (including optionally intervening whitespace)
  ///   with a single Windows-style newline sequence ("\r\n").
  /// </summary>
  /// <param name="input">The input string to normalize.</param>
  /// <param name="includeWhitespaceBetweenNewlines">
  ///   If true, whitespace between newline characters is ignored when collapsing them.
  /// </param>
  /// <returns>The normalized string with collapsed newlines.</returns>
  public static string CollapseConsecutiveNewlines(
    string input,
    bool includeWhitespaceBetweenNewlines
  ) {
    if (string.IsNullOrEmpty(input)) {
      return input;
    }

    var result         = new StringBuilder(input.Length);
    var i              = 0;
    var length         = input.Length;
    var lastWasNewline = false;

    while (i < length) {
      var c = input[i];

      if (c is '\r' or '\n') {
        if (!lastWasNewline) {
          result.Append("\r\n");
          lastWasNewline = true;
        }

        // Skip CRLF as a unit
        if (c == '\r' &&
            i + 1 < length &&
            input[i + 1] == '\n') {
          i++;
        }

        i++;

        if (includeWhitespaceBetweenNewlines) {
          var lookahead = i;
          while (lookahead < length) {
            var look = input[lookahead];

            if (look is '\r' or '\n') {
              // Treat as continuation of newline group
              if (look == '\r' &&
                  lookahead + 1 < length &&
                  input[lookahead + 1] == '\n') {
                lookahead++;
              }

              lookahead++;
            }
            else if (char.IsWhiteSpace(look)) {
              var temp = lookahead;

              // Check if whitespace is followed by newline (collapsible)
              while (temp < length &&
                     char.IsWhiteSpace(input[temp]) &&
                     input[temp] != '\r' &&
                     input[temp] != '\n') {
                temp++;
              }

              if (temp < length &&
                  (input[temp] == '\r' || input[temp] == '\n')) {
                // Skip whitespace as part of inter-newline region
                lookahead = temp;
              }
              else {
                break; // Whitespace followed by non-newline → preserve
              }
            }
            else {
              break; // Reached content → preserve
            }
          }

          i = lookahead;
        }
        else {
          // Skip consecutive \r or \n only
          while (i < length &&
                 (input[i] == '\r' || input[i] == '\n')) {
            if (input[i] == '\r' &&
                i + 1 < length &&
                input[i + 1] == '\n') {
              i++;
            }

            i++;
          }
        }
      }
      else {
        result.Append(c);
        lastWasNewline = false;
        i++;
      }
    }

    return result.ToString();
  }
}
