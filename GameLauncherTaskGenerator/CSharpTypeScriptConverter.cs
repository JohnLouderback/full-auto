using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GameLauncherTaskGenerator;

/// <summary>
///   Converts C# types to TypeScript equivalents.
/// </summary>
public static class CSharpTypeScriptConverter {
  public static string Convert(TypeSyntax type) {
    return Convert(type.ToString());
  }


  /// <summary>
  ///   Converts a C# type (expressed as a string) into its TypeScript equivalent.
  ///   Handles nullable types, arrays, generics (including nested generics), and
  ///   common collection interfaces.
  /// </summary>
  public static string Convert(string csharpType) {
    if (string.IsNullOrWhiteSpace(csharpType)) {
      return "any";
    }

    return ConvertCSharpTypeToTypeScript(csharpType.Trim());
  }


  private static string ConvertCSharpTypeToTypeScript(string input) {
    // Handle nullable types (e.g. "int?" => "number | null")
    if (input.EndsWith("?")) {
      var baseType = input.Substring(0, input.Length - 1).Trim();
      var tsType   = ConvertCSharpTypeToTypeScript(baseType);
      return $"{tsType} | null";
    }

    // Handle array types (e.g. "SomeType[]" => "Array<SomeType>")
    if (input.EndsWith("[]")) {
      var baseType = input.Substring(0, input.Length - 2).Trim();
      var tsType   = ConvertCSharpTypeToTypeScript(baseType);
      return $"Array<{tsType}>";
    }

    // Handle generic types (e.g. "Task<T>", "List<T>", "Dictionary<TKey, TValue>", etc.)
    var genericStart = input.IndexOf('<');
    if (genericStart != -1) {
      var genericEnd = FindMatchingAngleBracket(input, genericStart);
      if (genericEnd == -1) {
        throw new ArgumentException("Invalid generic type syntax: mismatched '<' and '>'");
      }

      var mainType           = input.Substring(0, genericStart).Trim();
      var genericArgsContent = input.Substring(genericStart + 1, genericEnd - genericStart - 1);
      var genericArgs        = SplitGenericArguments(genericArgsContent).ToList();
      var convertedArgs =
        genericArgs.Select(arg => ConvertCSharpTypeToTypeScript(arg.Trim())).ToList();

      // Map known generic types to TypeScript equivalents
      switch (mainType) {
        case "Task":
          if (convertedArgs.Count == 0) {
            return "Promise<void>";
          }

          return $"Promise<{convertedArgs[0]}>";
        case "List":
        case "IEnumerable":
        case "IList":
        case "ICollection":
          return $"Array<{convertedArgs[0]}>";
        case "Dictionary":
          if (convertedArgs.Count >= 2) {
            return $"Record<{convertedArgs[0]}, {convertedArgs[1]}>";
          }

          break;
        default:
          // For any other generic type, keep the original generic structure.
          return $"{mainType}<{string.Join(", ", convertedArgs)}>";
      }
    }

    // Handle non-generic collection interfaces (when the element type isn’t specified)
    if (input == "IEnumerable" ||
        input == "IList" ||
        input == "ICollection") {
      return "Array<any>";
    }

    // Map basic types.
    switch (input) {
      case "void":
        return "void";
      case "string":
        return "string";
      case "int":
      case "long":
      case "float":
      case "double":
      case "decimal":
        return "number";
      case "bool":
        return "boolean";
      case "object":
        return "any";
      case "Task":
        return "Promise<void>";
      default:
        // If the type comes with a namespace (e.g. "System.String"), strip it off.
        if (input.StartsWith("System.", StringComparison.Ordinal)) {
          return ConvertCSharpTypeToTypeScript(input.Substring("System.".Length));
        }

        // Otherwise, assume it’s a custom or unmapped type.
        return input;
    }
  }


  /// <summary>
  ///   Finds the matching '>' for the first '<' in the string.
  /// Returns the index of the matching '>' or
  ///   -1 if not found.
  /// </summary>
  private static int FindMatchingAngleBracket(string s, int startIndex) {
    var count = 0;
    for (var i = startIndex; i < s.Length; i++) {
      var c = s[i];
      if (c == '<') {
        count++;
      }
      else if (c == '>') {
        count--;
      }

      if (count == 0) {
        return i;
      }
    }

    return -1;
  }


  /// <summary>
  ///   Splits a comma-separated list of generic arguments, ignoring commas within nested generics.
  /// </summary>
  private static IEnumerable<string> SplitGenericArguments(string s) {
    var args    = new List<string>();
    var lastPos = 0;
    var depth   = 0;
    for (var i = 0; i < s.Length; i++) {
      // Character "c" of string "s".
      var c = s[i];

      // If we encounter a '<', increase the depth. This is used to understanding nested generics.
      if (c == '<') {
        depth++;
      }
      else if (c == '>') {
        depth--;
      }
      else if (c == ',' &&
               depth == 0) {
        args.Add(s.Substring(lastPos, i - lastPos).Trim());
        lastPos = i + 1;
      }
    }

    if (lastPos < s.Length) {
      args.Add(s.Substring(lastPos).Trim());
    }

    return args;
  }
}
