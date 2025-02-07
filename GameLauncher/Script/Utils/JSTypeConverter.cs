using Microsoft.ClearScript;

namespace GameLauncher.Script.Utils;

public class JSTypeConverter {
  private static dynamic? _getJSType;

  private static dynamic getJSType => _getJSType ??= ScriptEngine.Current.Evaluate(
                                 """
                                 (() => {
                                   function getTypeName(value) {
                                     if (value === null) return 'null';
                                     if (value === undefined) return 'undefined';
                                   
                                     const type = typeof value;
                                   
                                     // Handle primitives
                                     if (type !== 'object' && type !== 'function') return type;
                                   
                                     // Handle functions with signature extraction
                                     if (type === 'function') {
                                       return value.name
                                         ? `function ${value.name}(${getFunctionParams(value)})`
                                         : 'Anonymous Function';
                                     }
                                   
                                     // Handle standard objects
                                     const typeString = Object.prototype.toString.call(value);
                                     const constructorName = value.constructor?.name;
                                   
                                     if (typeString === '[object Object]') {
                                       return Object.getPrototypeOf(value) === Object.prototype || Object.getPrototypeOf(value) === null
                                         ? 'Plain Object'
                                         : `class ${constructorName}`;
                                     }
                                   
                                     if (typeString === '[object Array]') {
                                       return `Array<${getIterableType(value)}>`;
                                     }
                                   
                                     if (typeString === '[object Set]') {
                                       return `Set<${getIterableType(Array.from(value))}>`;
                                     }
                                   
                                     if (typeString === '[object Map]') {
                                       return `Map<${getIterableType(Array.from(value.keys()))}, ${getIterableType(Array.from(value.values()))}>`;
                                     }
                                   
                                     if (typeString.startsWith('[object ')) {
                                       return typeString.slice(8, -1); // Extracts "Date", "Promise", "WeakMap", etc.
                                     }
                                   
                                     // Handle user-defined classes
                                     return constructorName ? `class ${constructorName}` : 'Unknown Object';
                                   }
                                   
                                   function getFunctionParams(fn) {
                                     const match = fn.toString().match(/\(([^)]*)\)/);
                                     return match ? match[1] : '...';
                                   }
                                   
                                   function getIterableType(iterable) {
                                     if (iterable.length === 0) return 'empty';
                                   
                                     const uniqueTypes = new Set(iterable.map(getTypeName));
                                     return [...uniqueTypes].join(' | ');
                                   }
                                   
                                   return getTypeName;
                                 })();
                                 """
                               );
  
  /// <summary>
  ///  Get the JavaScript type of the specified value.
  ///
  ///  Example return values:
  ///  <ul>
  ///   <li><c>undefined</c></li>
  ///   <li><c>null</c></li>
  ///   <li><c>boolean</c></li>
  ///   <li><c>number</c></li>
  ///   <li><c>string</c></li>
  ///   <li><c>symbol</c></li>
  ///   <li><c>function name(...)</c></li>
  ///   <li><c>class ClassName</c></li>
  ///   <li><c>Plain Object</c></li>
  ///   <li><c>Array&lt;Type&gt;</c></li>
  ///   <li><c>Set&lt;Type&gt;</c></li>
  ///   <li><c>Map&lt;KeyType, ValueType&gt;</c></li>
  ///   <li><c>Date</c></li>
  ///   <li><c>Promise</c></li>
  ///   <li><c>WeakMap</c></li>
  ///   <li><c>WeakSet</c></li>
  ///   <li><c>Unknown Object</c></li>
  ///  </ul>
  /// </summary>
  /// <param name="value"></param>
  /// <returns></returns>
  public static string GetJSType(object value) {
    return getJSType(value);
  }
  
  public static bool IsPlainObject(object value) => GetJSType(value) == "Plain Object";
  public static bool IsArray(object value) => GetJSType(value).StartsWith("Array<");
  public static bool IsSet(object value) => GetJSType(value).StartsWith("Set<");
  public static bool IsMap(object value) => GetJSType(value).StartsWith("Map<");
  public static bool IsDate(object value) => GetJSType(value) == "Date";
  public static bool IsPromise(object value) => GetJSType(value) == "Promise";
  public static bool IsWeakMap(object value) => GetJSType(value) == "WeakMap";
  public static bool IsWeakSet(object value) => GetJSType(value) == "WeakSet";
  public static bool IsUnknownObject(object value) => GetJSType(value) == "Unknown Object";
  
}
