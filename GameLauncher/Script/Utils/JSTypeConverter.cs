using System.Collections;
using System.Reflection;
using Microsoft.ClearScript;

// or whichever engine you use

namespace GameLauncher.Script.Utils;

public static class JSTypeConverter {
  private static dynamic? _getJSType;

  private static dynamic getJSType => _getJSType ??= AppState.ScriptEngine.Evaluate(
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
                                              return typeString.slice(8, -1); // Extracts "Date", "Promise", etc.
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
  ///   Converts a ScriptObject (assumed to be a plain object) to a target type T.
  ///   If the object’s shape is incompatible, an exception is thrown with detailed
  ///   messages listing every mismatch.
  /// </summary>
  public static T ConvertTo<T>(ScriptObject obj) where T : new() {
    List<string> errors;
    if (!MatchesShape<T>(obj, out errors)) {
      throw new ScriptEngineException(
        "Conversion failed due to shape mismatches: " + string.Join("; ", errors)
      );
    }

    return (T)ConvertObject(obj, typeof(T));
  }


  /// <summary>
  ///   Get the JavaScript type of the specified value.
  ///   Example return values:
  ///   <ul>
  ///     <li>
  ///       <c>undefined</c>
  ///     </li>
  ///     <li>
  ///       <c>null</c>
  ///     </li>
  ///     <li>
  ///       <c>boolean</c>
  ///     </li>
  ///     <li>
  ///       <c>number</c>
  ///     </li>
  ///     <li>
  ///       <c>string</c>
  ///     </li>
  ///     <li>
  ///       <c>symbol</c>
  ///     </li>
  ///     <li>
  ///       <c>function name(...)</c>
  ///     </li>
  ///     <li>
  ///       <c>class ClassName</c>
  ///     </li>
  ///     <li>
  ///       <c>Plain Object</c>
  ///     </li>
  ///     <li>
  ///       <c>Array&lt;Type&gt;</c>
  ///     </li>
  ///     <li>
  ///       <c>Set&lt;Type&gt;</c>
  ///     </li>
  ///     <li>
  ///       <c>Map&lt;KeyType, ValueType&gt;</c>
  ///     </li>
  ///     <li>
  ///       <c>Date</c>
  ///     </li>
  ///     <li>
  ///       <c>Promise</c>
  ///     </li>
  ///     <li>
  ///       <c>WeakMap</c>
  ///     </li>
  ///     <li>
  ///       <c>WeakSet</c>
  ///     </li>
  ///     <li>
  ///       <c>Unknown Object</c>
  ///     </li>
  ///   </ul>
  /// </summary>
  public static string GetJSType(object value) {
    return getJSType(value);
  }


  public static bool IsArray(object value) {
    return GetJSType(value).StartsWith("Array<");
  }


  public static bool IsDate(object value) {
    return GetJSType(value) == "Date";
  }


  public static bool IsFunction(object value) {
    return GetJSType(value).StartsWith("function ") || GetJSType(value) == "Anonymous Function";
  }


  public static bool IsMap(object value) {
    return GetJSType(value).StartsWith("Map<");
  }


  public static bool IsPlainObject(object value) {
    return GetJSType(value) == "Plain Object";
  }


  public static bool IsPromise(object value) {
    return GetJSType(value) == "Promise";
  }


  public static bool IsSet(object value) {
    return GetJSType(value).StartsWith("Set<");
  }


  public static bool IsUnknownObject(object value) {
    return GetJSType(value) == "Unknown Object";
  }


  public static bool IsWeakMap(object value) {
    return GetJSType(value) == "WeakMap";
  }


  public static bool IsWeakSet(object value) {
    return GetJSType(value) == "WeakSet";
  }


  /// <summary>
  ///   Checks whether the given ScriptObject (assumed to be a plain object)
  ///   has a “shape” compatible with the public properties of type T.
  ///   Required (non-nullable) properties must be present and convertible;
  ///   nullable properties are optional.
  ///   Any mismatches are returned in the errors list.
  /// </summary>
  public static bool MatchesShape<T>(ScriptObject obj, out List<string> errors) {
    errors = new List<string>();
    if (!obj.IsPlainObject()) {
      errors.Add($"Expected a plain object, but got {GetJSType(obj)}.");
      return false;
    }

    ValidateShape(obj, typeof(T), "", errors);
    return errors.Count == 0;
  }


  /// <summary>
  ///   Recursively creates an instance of targetType from the given ScriptObject.
  ///   Assumes that the shape has already been validated.
  /// </summary>
  private static object ConvertObject(ScriptObject obj, Type targetType) {
    var result = Activator.CreateInstance(targetType) ??
                 throw new ScriptEngineException(
                   $"Failed to create an instance of {targetType.Name}."
                 );

    foreach (var prop in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
      var jsPropName = GetJSPropertyName(prop);
      // If the JS object has a property by that name, then convert it.
      if (obj.PropertyNames.Contains(jsPropName)) {
        var value          = obj.GetProperty(jsPropName);
        var convertedValue = ConvertValue(value, prop.PropertyType);
        prop.SetValue(result, convertedValue);
      }
    }

    return result;
  }


  /// <summary>
  ///   Converts an individual value (from a ScriptObject) to the specified targetType.
  ///   Handles primitives, arrays, and nested ScriptObjects recursively.
  /// </summary>
  private static object ConvertValue(object? value, Type targetType) {
    if (value == null) {
      return null!;
    }

    // If targetType is Nullable<T>, get its underlying type.
    var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

    if (IsSimpleType(underlyingType)) {
      try {
        return Convert.ChangeType(value, underlyingType);
      }
      catch (Exception ex) {
        throw new ScriptEngineException(
          $"Failed to convert value '{value}' to type {targetType.Name}: {ex.Message}"
        );
      }
    }

    if (underlyingType.IsArray) {
      // Expect value to be a ScriptObject representing an array.
      if (value is ScriptObject so &&
          IsArray(so)) {
        var elementType = underlyingType.GetElementType()!;
        var list        = new List<object?>();
        foreach (var element in (IEnumerable)so) {
          list.Add(ConvertValue(element, elementType));
        }

        var arr = Array.CreateInstance(elementType, list.Count);
        for (var i = 0; i < list.Count; i++) {
          arr.SetValue(list[i], i);
        }

        return arr;
      }

      throw new ScriptEngineException($"Expected an array for conversion to {targetType.Name}.");
    }

    if (value is ScriptObject soValue) {
      // Recursively convert nested objects.
      return ConvertObject(soValue, underlyingType);
    }

    if (underlyingType.IsAssignableFrom(value.GetType())) {
      return value;
    }

    throw new ScriptEngineException(
      $"Value '{value}' of type {value.GetType().Name} is not assignable to {targetType.Name}."
    );
  }


  /// <summary>
  ///   Returns the name that a given .NET property is exposed as in JS.
  ///   If the property is decorated with [ScriptMember("name")], that name is used;
  ///   otherwise the CLR property name is returned.
  /// </summary>
  private static string GetJSPropertyName(PropertyInfo prop) {
    var attr = prop.GetCustomAttribute<ScriptMemberAttribute>();
    if (attr != null &&
        !string.IsNullOrEmpty(attr.Name)) {
      return attr.Name;
    }

    return prop.Name;
  }


  /// <summary>
  ///   Determines whether a property is “required” (i.e. not nullable).
  ///   For value types this means not a Nullable&lt;T&gt;.
  ///   For reference types, this uses the C# 8+ nullability metadata.
  /// </summary>
  private static bool IsPropertyRequired(PropertyInfo prop) {
    if (prop.PropertyType.IsValueType) {
      return Nullable.GetUnderlyingType(prop.PropertyType) == null;
    }

    var nullabilityContext = new NullabilityInfoContext();
    var nullability        = nullabilityContext.Create(prop);
    return nullability.ReadState != NullabilityState.Nullable;
  }


  /// <summary>
  ///   Returns true if the given type is “simple” – that is, a primitive, enum,
  ///   string, decimal, DateTime, etc.
  /// </summary>
  private static bool IsSimpleType(Type type) {
    return type.IsPrimitive ||
           type.IsEnum ||
           type == typeof(string) ||
           type == typeof(decimal) ||
           type == typeof(DateTime) ||
           type == typeof(DateTimeOffset) ||
           type == typeof(Guid);
  }


  /// <summary>
  ///   Recursively validates that the given ScriptObject has properties matching the public
  ///   properties of targetType. Any mismatches (missing or unconvertible properties) are added
  ///   to the errors list.
  /// </summary>
  private static void ValidateShape(
    ScriptObject obj,
    Type targetType,
    string path,
    List<string> errors
  ) {
    foreach (var prop in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
      var jsPropName = GetJSPropertyName(prop);
      var fullPath   = string.IsNullOrEmpty(path) ? jsPropName : $"{path}.{jsPropName}";
      var isRequired = IsPropertyRequired(prop);

      if (!obj.PropertyNames.Contains(jsPropName)) {
        if (isRequired) {
          errors.Add($"Missing required property '{fullPath}'.");
        }

        continue;
      }

      // The property exists.
      var value = obj.GetProperty(jsPropName);
      if (value == null) {
        if (isRequired) {
          errors.Add($"Property '{fullPath}' cannot be null.");
        }

        continue;
      }

      var propType       = prop.PropertyType;
      var underlyingType = Nullable.GetUnderlyingType(propType) ?? propType;

      if (IsSimpleType(underlyingType)) {
        try {
          Convert.ChangeType(value, underlyingType);
        }
        catch (Exception ex) {
          errors.Add(
            $"Property '{
              fullPath
            }' expected type \"{
              underlyingType.Name
            }\" but got type \"{
              GetJSType(value)
            }\". Conversion failed: {
              ex.Message
            }"
          );
        }
      }
      else if (underlyingType.IsArray) {
        if (!(value is ScriptObject arrayObj) ||
            !IsArray(arrayObj)) {
          errors.Add($"Property '{fullPath}' expected an array but got {GetJSType(value)}.");
        }
        else {
          var elementType = underlyingType.GetElementType()!;
          var index       = 0;
          foreach (var element in (IEnumerable)arrayObj) {
            if (element is ScriptObject elementObj) {
              ValidateShape(elementObj, elementType, $"{fullPath}[{index}]", errors);
            }
            else if (IsSimpleType(elementType)) {
              try {
                Convert.ChangeType(element, elementType);
              }
              catch (Exception ex) {
                errors.Add(
                  $"Element '{
                    fullPath
                  }[{
                    index
                  }]' expected type {
                    elementType.Name
                  } but conversion failed: {
                    ex.Message
                  }"
                );
              }
            }
            else {
              if (!elementType.IsAssignableFrom(element.GetType())) {
                errors.Add(
                  $"Element '{
                    fullPath
                  }[{
                    index
                  }]' expected type {
                    elementType.Name
                  } but got {
                    element.GetType().Name
                  }."
                );
              }
            }

            index++;
          }
        }
      }
      else if (value is ScriptObject nestedObj) {
        // Recursively validate nested objects.
        ValidateShape(nestedObj, underlyingType, fullPath, errors);
      }
      else {
        if (!underlyingType.IsAssignableFrom(value.GetType())) {
          errors.Add(
            $"Property '{
              fullPath
            }' expected type {
              underlyingType.Name
            } but got {
              value.GetType().Name
            }."
          );
        }
      }
    }
  }
}
