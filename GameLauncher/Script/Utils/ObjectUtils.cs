using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.ClearScript;
using static GameLauncher.Script.Utils.JSTypeConverter;

namespace GameLauncher.Script.Utils;

public static class ObjectUtils {
  /// <summary>
  ///   Converts a script object to a JSON-like string. This will use reflection to find all of the
  ///   members of the object with a <c>ScriptMember</c> attribute and convert them to a JS-like
  ///   console output. This function acts recursively on nested objects and collections.
  ///   The output is pretty-printed with two-space indentation.
  /// </summary>
  /// <param name="obj">The object to convert.</param>
  /// <returns>A console.log-like string representation of the object.</returns>
  public static string ToJsonLikeString(object obj) {
    return ToJsonLikeString(obj, indentLevel: 0);
  }


  /// <summary>
  ///   Describe a Task as &lt;pending&gt;, &lt;fulfilled&gt;, &lt;rejected&gt;, etc.
  /// </summary>
  private static string DescribeTaskStatus(Task task) {
    // Simplify or expand as needed
    return task.Status switch {
      TaskStatus.RanToCompletion => "<fulfilled>",
      TaskStatus.Faulted         => "<rejected>",
      TaskStatus.Canceled        => "<canceled>",
      _                          => "<pending>"
    };
  }


  /// <summary>
  ///   Converts a ScriptObject that is known to be a JavaScript array to a JSON-like string.
  /// </summary>
  /// <param name="so"> The ScriptObject representing the JavaScript array. </param>
  /// <param name="indentLevel">
  ///   The current indentation level for pretty-printing. Defaults to 0.
  /// </param>
  /// <returns>
  ///   The JSON-like string representation of the JavaScript array.
  /// </returns>
  private static string JSArrayToString(ScriptObject so, int indentLevel = 0) {
    var indent      = new string(c: ' ', indentLevel * 2);
    var childIndent = new string(c: ' ', (indentLevel + 1) * 2);

    var length = (int)(so.GetProperty("length") ?? 0);
    var sbArr  = new StringBuilder();
    sbArr.Append("[\n");
    for (var i = 0; i < length; i++) {
      if (i > 0) {
        sbArr.Append(",\n");
      }

      sbArr.Append(childIndent);
      var item = so.GetProperty(i.ToString());
      sbArr.Append(ToJsonLikeString(item, indentLevel + 1));
    }

    sbArr.Append("\n");
    sbArr.Append(indent).Append("]");
    return sbArr.ToString();
  }


  /// <summary>
  ///   Excepts a ScriptObject that is known to be a plain object (i.e. not a host object, array,
  ///   function, etc.) and converts it to a JSON-like string. This is a helper for the main
  ///   ToJsonLikeString function to avoid unnecessary reflection on ScriptObjects.
  /// </summary>
  /// <param name="so"> The ScriptObject representing the plain JavaScript object. </param>
  /// <param name="indentLevel">
  ///   The current indentation level for pretty-printing. Defaults to 0.
  /// </param>
  /// <returns>
  ///   The JSON-like string representation of the plain JavaScript object.
  /// </returns>
  private static string JSPlainScriptObjectToString(ScriptObject so, int indentLevel = 0) {
    var indent      = new string(c: ' ', indentLevel * 2);
    var childIndent = new string(c: ' ', (indentLevel + 1) * 2);

    // Check if it's an array
    if (IsArray(so)) {
      return JSArrayToString(so, indentLevel);
    }

    var sbObj = new StringBuilder();
    sbObj.Append("{");
    var members = so.ListKeys();

    var anyMember = false;
    foreach (var member in members) {
      if (anyMember) {
        sbObj.Append(',');
      }

      sbObj.Append('\n');

      anyMember = true;
      var value = so.GetProperty(member);
      sbObj.Append(childIndent);
      sbObj.Append($"{member}: ");
      sbObj.Append(ToJsonLikeString(value, indentLevel + 1));
    }

    if (anyMember) {
      sbObj.Append('\n');
    }
    else {
      sbObj.Append(' ');
    }

    sbObj.Append(indent).Append('}');
    return sbObj.ToString();
  }


  /// <summary>
  ///   Internal recursive helper that tracks indentation level.
  /// </summary>
  private static string ToJsonLikeString(object obj, int indentLevel) {
    var indent      = new string(c: ' ', indentLevel * 2);
    var childIndent = new string(c: ' ', (indentLevel + 1) * 2);

    if (obj == null) {
      return "null";
    }

    if (obj is Undefined) {
      return "undefined";
    }

    var t = obj.GetType();

    // Handle primitive numeric, bool, etc.
    if (t.IsPrimitive ||
        obj is decimal) {
      // Booleans to lowercase
      if (obj is bool b) {
        return b.ToString().ToLower();
      }

      return obj.ToString();
    }

    // Strings
    if (obj is string s) {
      return $"\"{s}\"";
    }

    // Dates
    if (obj is DateTime dt) {
      return $"{{ {dt:O} }}";
    }

    // Tasks => “Promise { <pending> }” or similar
    if (obj is Task task) {
      return $"Promise {{ {DescribeTaskStatus(task)} }}";
    }

    // Delegates => [Function: methodName]
    if (obj is Delegate del) {
      var fnName = del.Method.Name;
      return $"[Function: {fnName}]";
    }

    // If the object is a JavaScript object (ScriptObject), handle it specially.
    if (obj is ScriptObject so) {
      // If it's a ScriptObject, we can try to treat it as a plain object first.
      return JSPlainScriptObjectToString(so, indentLevel);
    }

    // Enumerables (excluding strings, already handled above)
    if (obj is IEnumerable enumerable) {
      var sbArr = new StringBuilder();
      sbArr.Append("[\n");
      var first = true;
      foreach (var item in enumerable) {
        if (!first) {
          sbArr.Append(",\n");
        }
        else {
          first = false;
        }

        sbArr.Append(childIndent);
        sbArr.Append(ToJsonLikeString(item, indentLevel + 1));
      }

      sbArr.Append('\n');
      sbArr.Append(indent).Append(']');
      return sbArr.ToString();
    }

    // Otherwise, treat it as an object
    // Show only members decorated with ScriptMemberAttribute
    var sbObj = new StringBuilder();
    sbObj.Append("{\n");
    var anyMember = false;

    // (1) Public instance properties with [ScriptMember]
    var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
      .Where(p => p.GetIndexParameters().Length == 0); // skip indexers

    foreach (var prop in props) {
      var attr = prop.GetCustomAttribute<ScriptMemberAttribute>(inherit: false);
      if (attr == null) {
        continue; // Skip properties not marked with [ScriptMember]
      }

      object value;
      try {
        value = prop.GetValue(obj, index: null);
      }
      catch {
        // If a getter throws, skip or display something else
        continue;
      }

      if (anyMember) {
        sbObj.Append(",\n");
      }

      anyMember = true;

      // Use the attribute's Name if provided, otherwise the property name
      var keyName = string.IsNullOrWhiteSpace(attr.Name) ? prop.Name : attr.Name;

      sbObj.Append(childIndent);
      sbObj.Append($"{keyName}: ");
      sbObj.Append(ToJsonLikeString(value, indentLevel + 1));
    }

    // (2) Public instance fields with [ScriptMember]
    var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
    foreach (var field in fields) {
      var attr = field.GetCustomAttribute<ScriptMemberAttribute>(inherit: false);
      if (attr == null) {
        continue; // Skip fields not marked with [ScriptMember]
      }

      var value = field.GetValue(obj);

      if (anyMember) {
        sbObj.Append(",\n");
      }

      anyMember = true;

      var keyName = string.IsNullOrWhiteSpace(attr.Name) ? field.Name : attr.Name;

      sbObj.Append(childIndent);
      sbObj.Append($"{keyName}: ");
      sbObj.Append(ToJsonLikeString(value, indentLevel + 1));
    }

    // (3) Public instance methods with [ScriptMember], skipping base-object methods
    var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
      .Where(m => !m.IsSpecialName && m.DeclaringType != typeof(object));

    foreach (var method in methods) {
      var attr = method.GetCustomAttribute<ScriptMemberAttribute>(inherit: false);
      if (attr == null) {
        continue; // Skip methods not marked with [ScriptMember]
      }

      if (anyMember) {
        sbObj.Append(",\n");
      }

      anyMember = true;

      // Use the attribute's Name if provided, otherwise the method name
      var keyName = string.IsNullOrWhiteSpace(attr.Name) ? method.Name : attr.Name;

      sbObj.Append(childIndent);
      sbObj.Append($"{keyName}()"); // Use ƒ to indicate a function;
    }

    sbObj.Append('\n');
    sbObj.Append(indent).Append('}');
    return sbObj.ToString();
  }
}
