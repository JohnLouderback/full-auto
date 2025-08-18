using System.Collections;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Utils;

public static class JSInteropUtils {
  private static ScriptEngine? lastScriptEngine;

  private static dynamic? _isPlainObject;

  private static dynamic? _isValueTruthy;

  private static dynamic? _createUInt8Array;

  private static ScriptEngine currentScriptEngine =>
    AppState.ScriptEngine ??
    throw new InvalidOperationException(
      "The script engine is not initialized. Please initialize the script engine before using this method."
    );

  private static dynamic isPlainObject => _isPlainObject ??= currentScriptEngine.Evaluate(
                                            "(value) => Object.prototype.toString.call(value) === '[object Object]' && (Object.getPrototypeOf(value) === Object.prototype || Object.getPrototypeOf(value) === null);"
                                          );

  private static dynamic isValueTruthy => _isValueTruthy ??= currentScriptEngine.Evaluate(
                                            "(value) => !!value;"
                                          );

  private static dynamic createUInt8Array => _createUInt8Array ??= currentScriptEngine.Evaluate(
                                               "(lengthOrData) => new Uint8Array(lengthOrData);"
                                             );


  /// <summary>
  ///   Creates a new Uint8Array with the specified length.
  /// </summary>
  /// <returns>
  ///   A new Uint8Array with the specified length.
  /// </returns>
  public static dynamic CreateUInt8Array(uint length) {
    return createUInt8Array(length);
  }


  /// <summary>
  ///   Creates a new Uint8Array with the specified initial data.
  /// </summary>
  /// <returns>
  ///   A new Uint8Array with the specified initial data.
  /// </returns>
  public static dynamic CreateUInt8Array(IEnumerable initialData) {
    return createUInt8Array(initialData);
  }


  /// <summary>
  ///   Gets the value of a property on the object and casts it to the specified type.
  /// </summary>
  /// <param name="obj"> The object to get the property from. </param>
  /// <param name="name"> The name of the property. </param>
  /// <typeparam name="T"> The type to cast the property to. </typeparam>
  /// <returns> The value of the property, cast to the specified type. </returns>
  public static T? GetProperty<T>(this ScriptObject obj, string name) {
    // We treat undefined as null (implicitly meaning that we treat null and undefined as the same).
    // So, if the value comes back as undefined, we return null.
    var value = obj.GetProperty(name);
    return (T?)(value is null or Undefined ? null : value);
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
    return obj.GetProperty(name) is not Undefined;
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


  /// <summary>
  ///   Determines whether the value is truthy.
  /// </summary>
  /// <param name="value"> The value to check. </param>
  /// <returns>
  ///   <see langword="true" /> if the value is truthy; otherwise, <see langword="false" />.
  /// </returns>
  public static bool IsValueTruthy(this object value) {
    return (bool)isValueTruthy(value);
  }
}
