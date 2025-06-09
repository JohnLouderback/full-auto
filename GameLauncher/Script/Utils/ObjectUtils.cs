using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.ClearScript;

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
  ///   Internal recursive helper that tracks indentation level.
  /// </summary>
  private static string ToJsonLikeString(object obj, int indentLevel) {
    var indent      = new string(c: ' ', indentLevel * 2);
    var childIndent = new string(c: ' ', (indentLevel + 1) * 2);

    if (obj == null) {
      return "null";
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

      sbArr.Append("\n");
      sbArr.Append(indent).Append("]");
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
