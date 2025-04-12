namespace GameLauncherTaskGenerator;

public static class Utils {
  /// <summary>
  ///   Converts a string to camel case. For example, "HelloWorld" becomes "helloWorld".
  /// </summary>
  /// <param name="name"> The string to convert. </param>
  /// <returns> The camel case string. </returns>
  public static string ToCamel(string name) {
    if (string.IsNullOrEmpty(name)) return name;
    if (name.Length == 1) return name.ToLowerInvariant();
    return char.ToLowerInvariant(name[0]) + name.Substring(1);
  }
}
