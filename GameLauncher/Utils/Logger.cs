using System.Text;
using Spectre.Console;

namespace GameLauncher.Utils;

public static class Logger {
  /// <summary>
  ///   Logs an exception to the console.
  /// </summary>
  /// <param name="ex">The exception to log.</param>
  public static void Exception(Exception ex) {
    AnsiConsole.WriteException(ex);
  }


  /// <summary>
  ///   Given an exception string, logs the exception to the console.
  ///   Assumes the exception string represents a JavaScript exception.
  /// </summary>
  /// <param name="ex">The JavaScript exception string.</param>
  public static void Exception(string ex) {
    if (string.IsNullOrWhiteSpace(ex)) {
      return;
    }

    // Split the string into the first line (the error message) and the rest (the stack trace).
    var newLineIndex = ex.IndexOf('\n');
    if (newLineIndex == -1) {
      AnsiConsole.MarkupLineInterpolated($"[red]{ex.Trim().EscapeMarkup()}[/]");
      return;
    }

    var message    = ex.Substring(0, newLineIndex).Trim();
    var stackTrace = ex.Substring(newLineIndex + 1).Trim();

    // Output the error message.
    AnsiConsole.MarkupLineInterpolated($"[red]{message.EscapeMarkup()}[/]");

    // Parse the stack trace and output each frame.
    var parsedStackTrace = JsStackTraceParser.Parse(stackTrace);
    foreach (var frame in parsedStackTrace) {
      var line = FormatFrame(frame);
      AnsiConsole.MarkupLine(line);
    }
  }


  /// <summary>
  ///   Given a string of a text and a URI, returns a string that represents a link to the provided
  ///   URI. If we can reasonably determine that the current terminal does not support links, then
  ///   this method will return the provided text. Because of this, the text should be a reasonable
  ///   fallback for the link.
  /// </summary>
  /// <param name="text"> The text used to interact with this link. </param>
  /// <param name="uri"> The URI to link to. </param>
  /// <returns> </returns>
  public static string Link(string text, string uri) {
    // If the terminal does not support links, return the text.
    if (!AnsiConsole.Profile.Capabilities.Links) {
      return text;
    }

    // Otherwise, return the text as a link.
    return $"[link={uri}]{text}[/]";
  }


  /// <summary>
  ///   Formats a file path string into its components using markup.
  ///   If the file string contains extra information (e.g. trailing "[temp]"),
  ///   that extra text is rendered in grey while the file name is rendered in purple.
  /// </summary>
  private static string FormatFileInfo(string file) {
    if (string.IsNullOrWhiteSpace(file)) {
      return string.Empty;
    }

    var lastSlashIndex = file.LastIndexOf(Path.DirectorySeparatorChar);
    if (lastSlashIndex == -1) {
      // No directory present.
      var extraIndex = file.IndexOf('[');
      if (extraIndex == -1) {
        return $"[blue]{file.EscapeMarkup()}[/]";
      }

      var fileName = file.Substring(0, extraIndex);
      var extra    = file.Substring(extraIndex);
      return $"[blue]{fileName.EscapeMarkup()}[/][maroon]{extra.EscapeMarkup()}[/]";
    }
    else {
      // Extract directory and file name.
      var directory  = file.Substring(0, lastSlashIndex);
      var fileName   = file.Substring(lastSlashIndex + 1);
      var extraIndex = fileName.IndexOf('[');
      if (extraIndex == -1) {
        return $"[maroon]{
          (directory + Path.DirectorySeparatorChar).EscapeMarkup()
        }[/][blue]{Link(fileName, $"file://{Path.Combine(directory, fileName)}")}[/]";
      }

      var filePart = fileName.Substring(0, extraIndex);
      var extra    = fileName.Substring(extraIndex);
      return $"[maroon]{
        (directory + Path.DirectorySeparatorChar).EscapeMarkup()
      }[/][blue]{
        filePart.EscapeMarkup()
      }[/][maroon]{
        extra.EscapeMarkup()
      }[/]";
    }
  }


  /// <summary>
  ///   Formats a parsed JavaScript stack frame into a single markup string.
  /// </summary>
  private static string FormatFrame(JsStackFrame frame) {
    var sb = new StringBuilder();

    // "    at" indicator.
    sb.Append("[maroon]    at[/] ");

    // Type and method.
    sb.Append($"[maroon]{frame.Type.EscapeMarkup()}[/]");
    sb.Append($"[blue]{frame.Method.EscapeMarkup()}[/]");

    // Optionally include parameter list/details if a method name is provided.
    if (!string.IsNullOrWhiteSpace(frame.Method)) {
      sb.Append($"[maroon]{frame.ParameterList.EscapeMarkup()}[/]");
      sb.Append($"[maroon]{frame.Parameters.EscapeMarkup()}[/] ");
    }

    // Begin file/line info.
    sb.Append("[maroon]([/]");
    if (!string.IsNullOrWhiteSpace(frame.File)) {
      sb.Append(FormatFileInfo(frame.File));
    }

    sb.Append($"[maroon]:{frame.Line.EscapeMarkup()})[/]");

    return sb.ToString();
  }
}
