namespace GameLauncher.Utils;

/// <summary>
///   A class that allows throwing exceptions with a custom stack trace. The exception will be
///   printed with the custom stack trace when converted to a string.
/// </summary>
public class ScriptException : Exception {
  /// <summary>
  ///   Initializes a new instance of the <see cref="ScriptException" /> class with a specified error
  ///   message.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  public ScriptException(string message) : base(message) {}


  /// <summary>
  ///   Initializes a new instance of the <see cref="ScriptException" /> class with a specified error
  ///   message and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="innerException">
  ///   The exception that is the cause of the current exception, or a null reference if no inner
  ///   exception is specified.
  /// </param>
  public ScriptException(string message, Exception innerException) :
    base(message, innerException) {}


  /// <summary>
  ///   Initializes a new instance of the <see cref="ScriptException" /> class with a specified error
  ///   message and a custom stack trace.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  /// <param name="stackTrace">The custom stack trace.</param>
  public ScriptException(string message, string stackTrace) : base(message) {
    this.SetStackTrace(stackTrace);
  }
}
