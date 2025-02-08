using Microsoft.ClearScript;

namespace GameLauncher.Script.Utils;

public static class JSInteropUtils {
  private static dynamic? _isPlainObject;

  private static dynamic isPlainObject => _isPlainObject ??= ScriptEngine.Current.Evaluate(
                                            "(value) => Object.prototype.toString.call(value) === '[object Object]' && (Object.getPrototypeOf(value) === Object.prototype || Object.getPrototypeOf(value) === null);"
                                          );


  /// <summary>
  ///   Gets the value of a property on the object and casts it to the specified type.
  /// </summary>
  /// <param name="obj"> The object to get the property from. </param>
  /// <param name="name"> The name of the property. </param>
  /// <typeparam name="T"> The type to cast the property to. </typeparam>
  /// <returns> The value of the property, cast to the specified type. </returns>
  public static T GetProperty<T>(this ScriptObject obj, string name) {
    return (T)obj.GetProperty(name);
  }


  /// <summary>
  ///   Determines whether the object has a property with the specified name.
  /// </summary>
  /// <param name="obj"> The object to check. </param>
  /// <param name="name"> The name of the property to check for. </param>
  /// <returns>
  ///   <see langword="true" /> if the object has the property; otherwise, <see langword="false" />.
  /// </returns>
  public static bool HasProperty(this ScriptObject obj, string name) {
    return obj.GetProperty(name) != null;
  }


  /// <summary>
  ///   Determines whether the object is a plain object, like an object literal.
  /// </summary>
  /// <param name="obj"> The object to check. </param>
  /// <returns>
  ///   <see langword="true" /> if the object is a plain object; otherwise, <see langword="false" />.
  /// </returns>
  public static bool IsPlainObject(this ScriptObject obj) {
    return (bool)isPlainObject(obj);
  }
}
