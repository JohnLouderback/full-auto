using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GameLauncherTaskGenerator;

public static class TsTypeGenerator {
  /// <summary>
  ///   Generates TypeScript code for a given C# type declaration (class or interface) marked with the
  ///   export attribute.
  ///   Returns the generated TS code and an output set of dependency type names referenced by the type.
  /// </summary>
  public static string GenerateTs(
    TypeDeclarationSyntax typeDecl,
    Dictionary<string, TypeDeclarationSyntax> customTypes,
    out HashSet<string> dependencies
  ) {
    dependencies = new HashSet<string>(StringComparer.Ordinal);
    var sb          = new StringBuilder();
    var typeName    = typeDecl.Identifier.Text;
    var isInterface = typeDecl is InterfaceDeclarationSyntax;
    var tsKeyword   = isInterface ? "interface" : "class";

    // Generate the class-level JSDoc from the XML documentation (if any).
    var classDoc = JsDocGenerator.GenerateJsDoc(typeDecl);
    if (!string.IsNullOrWhiteSpace(classDoc)) {
      sb.AppendLine(classDoc);
    }

    sb.AppendLine($"// Auto-generated from C# {(isInterface ? "interface" : "class")} {typeName}");
    sb.AppendLine($"export interface {typeName} {{");

    // Process public members.
    foreach (var member in typeDecl.Members) {
      // Process public properties.
      if (member is PropertyDeclarationSyntax prop) {
        // Only include public properties.
        if (!prop.Modifiers.Any(SyntaxKind.PublicKeyword)) {
          continue;
        }

        var propDoc = JsDocGenerator.GenerateJsDoc(prop, "    ");
        if (!string.IsNullOrWhiteSpace(propDoc)) {
          sb.AppendLine(propDoc);
        }

        // Default to the C# identifier.
        var propName = prop.Identifier.Text;
        // If a ScriptMember attribute is present, use its string value.
        var scriptName = GetScriptMemberName(prop);
        if (!string.IsNullOrWhiteSpace(scriptName)) {
          propName = scriptName;
        }

        var propType = CSharpTypeScriptConverter.Convert(prop.Type);
        // Check if the property type is itself a custom type.
        var baseType = ExtractBaseType(propType);
        if (customTypes.ContainsKey(baseType) &&
            baseType != typeName) {
          dependencies.Add(baseType);
        }

        // Determine if the property should be readonly.
        var isReadOnly = false;
        if (prop.AccessorList != null) {
          // If there's no setter, then the property is readonly.
          if (!prop.AccessorList.Accessors.Any(
                a => a.Kind() == SyntaxKind.SetAccessorDeclaration
              )) {
            isReadOnly = true;
          }
          else {
            // Otherwise, check the setter accessor.
            var setter = prop.AccessorList.Accessors.FirstOrDefault(
              a => a.Kind() == SyntaxKind.SetAccessorDeclaration
            );
            if (setter != null) {
              // If the setter has any modifiers and none of them is public, mark the property readonly.
              if (setter.Modifiers.Any() &&
                  !setter.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) {
                isReadOnly = true;
              }
            }
          }
        }

        sb.AppendLine($"    {(isReadOnly ? "readonly " : "")}{propName}: {propType};");
      }
      // Process public methods.
      else if (member is MethodDeclarationSyntax method) {
        // Skip non-public methods and constructors.
        if (!method.Modifiers.Any(SyntaxKind.PublicKeyword) ||
            method.Identifier.Text == typeName) {
          continue;
        }

        var methodDoc = JsDocGenerator.GenerateJsDoc(method, "    ");
        if (!string.IsNullOrWhiteSpace(methodDoc)) {
          sb.AppendLine(methodDoc);
        }

        var isAsync = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
        // Default to the C# identifier.
        var methodName = method.Identifier.Text;
        // Check for a ScriptMember attribute.
        var methodScriptName = GetScriptMemberName(method);
        if (!string.IsNullOrWhiteSpace(methodScriptName)) {
          methodName = methodScriptName;
        }
        else {
          // Lowercase the first letter for JavaScript conventions.
          methodName = char.ToLower(methodName[0]) + methodName.Substring(1);
        }

        var returnType = CSharpTypeScriptConverter.Convert(method.ReturnType);
        // Add dependency if return type is custom.
        var retBase = ExtractBaseType(returnType);
        if (customTypes.ContainsKey(retBase) &&
            retBase != typeName) {
          dependencies.Add(retBase);
        }

        // Use a local variable to avoid lambda capture issues.
        var localDeps = dependencies;
        var parameters = string.Join(
          ", ",
          method.ParameterList.Parameters.Select(
            p => {
              var paramType = CSharpTypeScriptConverter.Convert(p.Type);
              var paramBase = ExtractBaseType(paramType);
              if (customTypes.ContainsKey(paramBase) &&
                  paramBase != typeName) {
                localDeps.Add(paramBase);
              }

              return $"{p.Identifier.Text}: {paramType}";
            }
          )
        );
        sb.AppendLine($"    {(isAsync ? "async " : "")}{methodName}({parameters}): {returnType};");
      }
    }

    sb.AppendLine("}");
    return sb.ToString();
  }


  /// <summary>
  ///   Extracts the base type name from a TypeScript type string.
  ///   For example, "Array<Application>" yields "Application" and "Application?" becomes "Application".
  /// </summary>
  private static string ExtractBaseType(string typeStr) {
    if (string.IsNullOrWhiteSpace(typeStr)) {
      return typeStr;
    }

    if (typeStr.EndsWith("?")) {
      typeStr = typeStr.Substring(0, typeStr.Length - 1);
    }

    if (typeStr.EndsWith("[]")) {
      typeStr = typeStr.Substring(0, typeStr.Length - 2);
    }

    var genericIndex = typeStr.IndexOf('<');
    if (genericIndex != -1) {
      typeStr = typeStr.Substring(0, genericIndex);
    }

    return typeStr.Trim();
  }


  /// <summary>
  ///   Checks the given member for a ScriptMember attribute and returns its argument value if present.
  /// </summary>
  private static string GetScriptMemberName(MemberDeclarationSyntax member) {
    foreach (var attrList in member.AttributeLists) {
      foreach (var attr in attrList.Attributes) {
        var attrName = attr.Name.ToString();
        if (attrName.EndsWith("ScriptMember") ||
            attrName.EndsWith("ScriptMemberAttribute")) {
          if (attr.ArgumentList != null &&
              attr.ArgumentList.Arguments.Count > 0) {
            // Assume the first argument is a string literal.
            var argExpr = attr.ArgumentList.Arguments[0].Expression;
            if (argExpr is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.StringLiteralExpression)) {
              return literal.Token.ValueText;
            }

            // Fallback: trim any quotes.
            return attr.ArgumentList.Arguments[0].ToString().Trim('"');
          }
        }
      }
    }

    return null;
  }
}
