using Microsoft.ClearScript.V8;

namespace GameLauncher.Script.Utils;

public static class JSUtils {
  /// <summary>
  ///   Converts a list to a JavaScript array.
  /// </summary>
  /// <param name="list"> The list to convert. </param>
  /// <param name="engine"> The ClearScript engine. </param>
  /// <typeparam name="T"> The type of the list. </typeparam>
  /// <returns></returns>
  public static JSArray<T> ListToJSArray<T>(this IEnumerable<T> list, V8ScriptEngine engine) {
    dynamic isInstanceOf = engine.Evaluate("(a, b) => a instanceof b");
    Console.WriteLine(
      $"Is instance of Array? {isInstanceOf(engine.Script.Array.from(list), engine.Script.Array)}"
    );
    return new JSArray<T>(engine.Script.Array.from(list));
  }
}
