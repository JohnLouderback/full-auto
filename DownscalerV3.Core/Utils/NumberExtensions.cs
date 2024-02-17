namespace DownscalerV3.Core.Utils;

public static class NumberExtensions {
  /// <summary>
  ///   Formats the number using the provided format.
  /// </summary>
  /// <param name="number"> The number to format. </param>
  /// <param name="format"> The format to use. <see cref="string.Format(string,object)" /> </param>
  /// <returns> The formatted number. </returns>
  /// <example>
  ///   <code language="csharp">
  ///     var number = 123.456;
  ///     var formattedNumber = number.Format("{0:N2} FPS"); // "123.46 FPS"
  ///   </code>
  /// </example>
  public static string Format(this double number, string format) {
    return string.Format(format, number);
  }
}
