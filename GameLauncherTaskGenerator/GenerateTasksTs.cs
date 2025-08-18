using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static GameLauncherTaskGenerator.Utils;

public class GenerateTasksTs : Task {
  [Required] public string SourceDirectory         { get; set; }
  [Required] public string OutputDir               { get; set; }
  public            string ExportAttributeName     { get; set; } = "TypeScriptExport";
  public            string EntryPointAttributeName { get; set; } = "EntryPointExport";


  public override bool Execute() {
    // if (!Debugger.IsAttached) {
    //   Debugger.Launch();
    // }

    try {
      var syntaxTrees = Directory
        .EnumerateFiles(SourceDirectory, "*.cs", SearchOption.AllDirectories)
        .Concat(
          Directory
            .EnumerateFiles(
              Path.GetFullPath(SourceDirectory + "../GenericModLauncher/"),
              "*.cs",
              SearchOption.AllDirectories
            )
        )
        .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path))
        .ToList();

      var references = new[] {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Task).Assembly.Location)
      };

      var compilation = CSharpCompilation.Create(
        "TemporaryAssembly",
        syntaxTrees,
        references,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
      );

      var processor = new SymbolProcessor(
        compilation,
        ExportAttributeName,
        EntryPointAttributeName,
        OutputDir,
        Log
      );

      processor.Process();

      return true;
    }
    catch (Exception ex) {
      Log.LogErrorFromException(ex, showStackTrace: true);
      return false;
    }
  }
}

file class SymbolProcessor {
  private readonly CSharpCompilation                    _compilation;
  private readonly string                               _exportAttrName;
  private readonly string                               _entryAttrName;
  private readonly string                               _outputDir;
  private readonly TaskLoggingHelper                    _log;
  private readonly Dictionary<string, INamedTypeSymbol> _entryPoints = new(StringComparer.Ordinal);

  private readonly Dictionary<string, INamedTypeSymbol>
    _exportedTypes = new(StringComparer.Ordinal);

  private readonly List<INamedTypeSymbol> _globalDelegates = new();


  public SymbolProcessor(
    CSharpCompilation compilation,
    string exportAttrName,
    string entryAttrName,
    string outputDir,
    TaskLoggingHelper log
  ) {
    _compilation    = compilation;
    _exportAttrName = exportAttrName;
    _entryAttrName  = entryAttrName;
    _outputDir      = outputDir;
    _log            = log;
  }


  public void Process() {
    var processedSymbols = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

    foreach (var syntaxTree in _compilation.SyntaxTrees) {
      var model = _compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: true);
      var root  = syntaxTree.GetRoot();

      foreach (var decl in root.DescendantNodes().OfType<TypeDeclarationSyntax>()) {
        var symbol = model.GetDeclaredSymbol(decl);
        if (symbol is not INamedTypeSymbol named) continue;

        if (IsMarkedWith(named, _entryAttrName)) {
          _entryPoints[named.Name] = named;
        }

        if (IsMarkedWith(named, _exportAttrName)) {
          _exportedTypes[named.Name] = named;
        }
      }

      foreach (var del in root.DescendantNodes().OfType<DelegateDeclarationSyntax>()) {
        var symbol = model.GetDeclaredSymbol(del);
        if (symbol is { TypeKind: TypeKind.Delegate } delSym &&
            del.Parent is BaseNamespaceDeclarationSyntax or CompilationUnitSyntax) {
          _globalDelegates.Add(delSym);
          _exportedTypes[delSym.Name] = delSym;
        }
      }
    }

    var emitter = new TypeScriptEmitter(_compilation, _exportedTypes, _log);

    foreach (var entry in _entryPoints.OrderBy(e => e.Key, StringComparer.Ordinal)) {
      var ts = emitter.EmitEntryPointModule(entry.Value);
      WriteFile(entry.Key + ".ts", ts);
    }

    foreach (var kvp in _exportedTypes.OrderBy(e => e.Key, StringComparer.Ordinal)) {
      if (processedSymbols.Add(kvp.Value)) {
        var ts = emitter.EmitExportedType(kvp.Value);
        WriteFile(kvp.Key + ".ts", ts);
      }
    }

    foreach (var del in _globalDelegates.OrderBy(d => d.Name, StringComparer.Ordinal)) {
      var ts = emitter.EmitDelegate(del);
      WriteFile(del.Name + ".ts", ts);
    }
  }


  private static bool IsMarkedWith(ISymbol symbol, string attributeName) {
    return symbol.GetAttributes()
      .Any(a => a.AttributeClass?.Name.IndexOf(attributeName, StringComparison.Ordinal) > -1
      );
  }


  private void WriteFile(string fileName, string content) {
    var path = Path.Combine(_outputDir, fileName);
    File.WriteAllText(path, content);
    _log.LogMessage(MessageImportance.High, $"Wrote: {fileName}");
  }
}

file class TypeScriptEmitter {
  private readonly CSharpCompilation                    _compilation;
  private readonly Dictionary<string, INamedTypeSymbol> _customTypes;
  private readonly TaskLoggingHelper                    _log;


  public TypeScriptEmitter(
    CSharpCompilation compilation,
    Dictionary<string, INamedTypeSymbol> customTypes,
    TaskLoggingHelper log
  ) {
    _compilation = compilation;
    _customTypes = customTypes;
    _log         = log;
  }


  public void EmitDelegate(INamedTypeSymbol del, StringBuilder sb, HashSet<string> dependencies) {
    if (del.TypeKind != TypeKind.Delegate) throw new ArgumentException("Expected delegate symbol");

    var invoke = del.DelegateInvokeMethod!;
    var doc    = DocumentationResolver.GetJsDoc(del, _compilation);

    if (!string.IsNullOrWhiteSpace(doc)) sb.AppendLine(doc);

    sb.AppendLine($"// Auto-generated from delegate {del.Name}");

    var args = string.Join(
      ", ",
      invoke.Parameters.Select(p => {
          var name = p.Name;
          var type = ToTypeScriptType(p.Type, dependencies, p);
          return p.HasExplicitDefaultValue || p.NullableAnnotation == NullableAnnotation.Annotated
                   ? $"{name}?: {type}"
                   : $"{name}: {type}";
        }
      )
    );

    var ret = ToTypeScriptType(invoke.ReturnType, dependencies);
    sb.AppendLine($"export type {del.Name} = ({args}) => {ret};");
  }


  public string EmitDelegate(INamedTypeSymbol del) {
    if (del.TypeKind != TypeKind.Delegate) throw new ArgumentException("Expected delegate symbol");

    var sb   = new StringBuilder();
    var deps = new HashSet<string>();

    EmitDelegate(del, sb, deps);

    var importBlock = EmitImportBlock(deps, [del.Name]);
    if (!string.IsNullOrWhiteSpace(importBlock)) {
      sb.Insert(index: 0, importBlock + "\n\n");
    }

    return sb.ToString();
  }


  public string EmitEntryPointModule(INamedTypeSymbol type) {
    var sb           = new StringBuilder();
    var dependencies = new HashSet<string>(StringComparer.Ordinal);

    sb.AppendLine("// Auto-generated. Do not edit manually.");
    sb.AppendLine();

    var doc = DocumentationResolver.GetJsDoc(type, _compilation);
    if (!string.IsNullOrWhiteSpace(doc)) sb.AppendLine(doc);

    var methods = type
      .GetMembers()
      .OfType<IMethodSymbol>()
      .Where(m =>
        m.MethodKind == MethodKind.Ordinary &&
        m.DeclaredAccessibility == Accessibility.Public &&
        !m.IsImplicitlyDeclared &&
        !IsHidden(m)
      )
      .GroupBy(m => m.Name)
      .OrderBy(g => g.Key, StringComparer.Ordinal);

    foreach (var group in methods) {
      var origName   = group.Key;
      var exportName = ToCamel(origName);
      var overloads  = group.ToList();

      if (overloads.Count == 1) {
        EmitFunction(sb, overloads[0], exportName, dependencies, type.Name);
      }
      else {
        foreach (var method in overloads) {
          var docComment = DocumentationResolver.GetJsDoc(method, _compilation, "    ");
          if (!string.IsNullOrWhiteSpace(docComment)) sb.AppendLine(docComment);
          var overriddenReturnType = GetOverriddenReturnType(dependencies, method);
          var returnType = overriddenReturnType ??
                           ToTypeScriptType(method.ReturnType, dependencies, method);
          sb.AppendLine(
            $"export function {exportName}({FormatParams(method, dependencies)}): {returnType};"
          );
        }

        sb.Append($"export function {exportName}(...args: any[]): any {{\n");
        sb.AppendLine("    // @ts-expect-error - Function is injected by the engine");
        sb.AppendLine($"    return __{type.Name}.{origName}(...args);");
        sb.AppendLine("}\n");
      }
    }

    var nestedDelegates = type
      .GetMembers()
      .OfType<INamedTypeSymbol>()
      .Where(d => d.TypeKind == TypeKind.Delegate)
      .OrderBy(d => d.Name, StringComparer.Ordinal);

    foreach (var del in nestedDelegates) {
      EmitDelegate(del, sb, dependencies);
    }

    var imports = EmitImportBlock(dependencies, [type.Name]);
    if (!string.IsNullOrWhiteSpace(imports)) {
      sb.Insert(index: 0, imports + "\n\n");
    }

    return sb.ToString();
  }


  public string EmitExportedType(INamedTypeSymbol symbol) {
    var sb           = new StringBuilder();
    var dependencies = new HashSet<string>(StringComparer.Ordinal);

    var doc = DocumentationResolver.GetJsDoc(symbol, _compilation);
    if (!string.IsNullOrWhiteSpace(doc)) sb.AppendLine(doc);

    var baseType      = symbol.BaseType;
    var extendsClause = string.Empty;

    if (baseType is { SpecialType: not SpecialType.System_Object } &&
        _customTypes.ContainsKey(baseType.Name)) {
      dependencies.Add(baseType.Name);
      extendsClause = $" extends {baseType.Name}";
    }

    sb.AppendLine($"// Auto-generated from C# type {symbol.Name}");
    sb.AppendLine($"export interface {symbol.Name}{extendsClause} {{");

    foreach (var member in symbol.GetMembers()) {
      if (member is IPropertySymbol prop &&
          prop.DeclaredAccessibility == Accessibility.Public &&
          !IsHidden(prop)) {
        var name   = GetScriptMemberName(prop) ?? ToCamel(prop.Name);
        var docStr = DocumentationResolver.GetJsDoc(prop, _compilation, "    ");
        if (!string.IsNullOrWhiteSpace(docStr)) sb.AppendLine(docStr);

        var isOptional = prop.NullableAnnotation == NullableAnnotation.Annotated;
        var isReadonly = prop.SetMethod is not
                                           { DeclaredAccessibility: Accessibility.Public }
                                           or { IsInitOnly: true };

        var tsType = ToTypeScriptType(prop.Type, dependencies, prop);

        sb.AppendLine(
          $"    {(isReadonly ? "readonly " : "")}{name}{(isOptional ? "?" : "")}: {tsType};"
        );
      }

      if (member is IMethodSymbol method &&
          method.MethodKind == MethodKind.Ordinary &&
          method.DeclaredAccessibility == Accessibility.Public &&
          !IsHidden(method)) {
        var name  = GetScriptMemberName(method) ?? ToCamel(method.Name);
        var jsDoc = DocumentationResolver.GetJsDoc(method, _compilation, "    ");
        if (!string.IsNullOrWhiteSpace(jsDoc)) sb.AppendLine(jsDoc);

        var parameters           = FormatParams(method, dependencies);
        var overriddenReturnType = GetOverriddenReturnType(dependencies, method);
        var returnType = overriddenReturnType ??
                         ToTypeScriptType(method.ReturnType, dependencies, method);
        sb.AppendLine($"    {name}({parameters}): {returnType};");
      }
    }

    sb.AppendLine("}");

    var importBlock = EmitImportBlock(dependencies, [symbol.Name]);
    if (!string.IsNullOrWhiteSpace(importBlock)) {
      sb.Insert(index: 0, importBlock + "\n\n");
    }

    return sb.ToString();
  }


  private static string EmitImportBlock(
    IEnumerable<string> dependencies,
    IEnumerable<string> excluding
  ) {
    var set = new HashSet<string>(dependencies, StringComparer.Ordinal);
    foreach (var e in excluding) {
      set.Remove(e);
    }

    return string.Join(
      "\n",
      set.OrderBy(x => x).Select(x => $"import {{ {x} }} from \"./{x}\";")
    );
  }


  private static string? GetScriptMemberName(ISymbol symbol) {
    var attr = symbol.GetAttributes()
      .FirstOrDefault(a =>
        a.AttributeClass?.Name is "ScriptMember" or "ScriptMemberAttribute"
      );

    if (attr is null) return null;

    if (attr is { ConstructorArguments.Length: 1 } &&
        attr.ConstructorArguments[0].Value is string s) {
      return s;
    }

    // If the attribute is not null, but we couldn't get the arg, try to get the AttributeSyntax
    // instance from it.

    if (attr.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax {
          ArgumentList.Arguments.Count: 1
        } attrSyntax) {
      var arg = attrSyntax.ArgumentList.Arguments[0].Expression;

      if (arg is LiteralExpressionSyntax literal &&
          literal.IsKind(SyntaxKind.StringLiteralExpression)) {
        var text = literal.Token.ValueText;
        return text.Trim();
      }
    }

    // If the ScriptMember is not null, try to get the AttributeSyntax instance from it.

    return null;
  }


  private static bool IsHidden(ISymbol symbol) {
    return symbol.GetAttributes()
      .Any(a => a.AttributeClass?.Name is "HideFromTypeScript" or "HideFromTypeScriptAttribute"
      );
  }


  private void EmitFunction(
    StringBuilder sb,
    IMethodSymbol method,
    string exportName,
    HashSet<string> dependencies,
    string className
  ) {
    var doc = DocumentationResolver.GetJsDoc(method, _compilation, "    ");
    if (!string.IsNullOrWhiteSpace(doc)) sb.AppendLine(doc);

    var parameters           = FormatParams(method, dependencies);
    var overriddenReturnType = GetOverriddenReturnType(dependencies, method);
    var returnType = overriddenReturnType ??
                     ToTypeScriptType(method.ReturnType, dependencies, method);

    sb.Append("export ");
    if (method.IsAsync) sb.Append("async ");
    sb.AppendLine($"function {exportName}({parameters}): {returnType} {{");
    sb.AppendLine("    // @ts-expect-error - Function is injected by the engine");
    sb.AppendLine(
      $"    return __{
        className
      }.{
        method.Name
      }({
        string.Join(", ", method.Parameters.Select(p => p.Name))
      });"
    );
    sb.AppendLine("}\n");
  }


  private string FormatParams(IMethodSymbol method, HashSet<string> dependencies) {
    return string.Join(
      ", ",
      method.Parameters.Select(p => {
          var name = p.Name;
          var type = ToTypeScriptType(p.Type, dependencies, p);
          return p.HasExplicitDefaultValue || p.NullableAnnotation == NullableAnnotation.Annotated
                   ? $"{name}?: {type}"
                   : $"{name}: {type}";
        }
      )
    );
  }


  private string? GetOverriddenReturnType(HashSet<string> deps, ISymbol? context = null) {
    var overrideAttr = context?.GetAttributes()
      .FirstOrDefault(a =>
        a.AttributeClass?.Name is "TsReturnTypeOverride" or "TsReturnTypeOverrideAttribute"
      );

    // If the override attribute is present and was passed a string, use that
    // as the type.
    if (overrideAttr is not null) {
      if (overrideAttr.ConstructorArguments.Length == 1) {
        if (overrideAttr.ConstructorArguments[0].Value is string overrideText) {
          // Check if there were dependencies specified in the attribute.
          if (overrideAttr.ConstructorArguments.Length > 1) {
            for (var i = 1; i < overrideAttr.ConstructorArguments.Length; i++) {
              var dep = overrideAttr.ConstructorArguments[i].Value;
              if (dep is INamedTypeSymbol namedDep) {
                deps.Add(namedDep.Name);
              }
              else if (dep is TypeOfExpressionSyntax typeOf) {
                deps.Add(typeOf.Type.ToString());
              }
            }
          }

          return overrideText.Trim();
        }

        if (overrideAttr.ConstructorArguments[0].Value is ITypeSymbol typeSymbol) {
          return ToTypeScriptType(typeSymbol, deps);
        }

        throw new InvalidOperationException(
          $"Invalid TsTypeOverride attribute on {context?.Name ?? "unknown"}: " +
          $"expected a string or type, got {
            overrideAttr.ConstructorArguments[0].Value?.GetType().Name
          }"
        );
      }

      // If the attribute is not null, but we couldn't get the arg, try to get the AttributeSyntax
      // instance from it.
      if (overrideAttr.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax {
            ArgumentList.Arguments.Count: >= 1
          } overrideAttrSyntax) {
        var arg = overrideAttrSyntax.ArgumentList.Arguments[0].Expression;

        if (arg is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression)) {
          // Check if there were dependencies specified in the attribute.
          if (overrideAttrSyntax.ArgumentList.Arguments.Count > 1) {
            foreach (var argSyntax in overrideAttrSyntax.ArgumentList.Arguments.Skip(1)) {
              if (argSyntax.Expression is TypeOfExpressionSyntax typeOf) {
                if (typeOf.Type is IdentifierNameSyntax identifier) {
                  deps.Add(identifier.Identifier.Text);
                }
                else if (typeOf.Type is QualifiedNameSyntax qualified) {
                  deps.Add(qualified.ToString());
                }
              }
              else if (argSyntax.Expression is IdentifierNameSyntax identifier) {
                deps.Add(identifier.Identifier.Text);
              }
            }
          }

          var text = literal.Token.ValueText;
          return text.Trim();
        }

        throw new InvalidOperationException(
          $"Invalid TsTypeOverride attribute on {context?.Name ?? "unknown"}: " +
          $"expected a string, got {arg.Kind()} ({arg})"
        );
      }
    }

    return null;
  }


  private string ToTypeScriptType(ITypeSymbol type, HashSet<string> deps, ISymbol? context = null) {
    var overrideAttr = context?.GetAttributes()
      .FirstOrDefault(a => a.AttributeClass?.Name is "TsTypeOverride" or "TsTypeOverrideAttribute"
      );

    // If the override attribute is present and was passed a string, use that
    // as the type.
    if (overrideAttr is not null) {
      if (overrideAttr.ConstructorArguments.Length == 1) {
        if (overrideAttr.ConstructorArguments[0].Value is string overrideText) {
          // Check if there were dependencies specified in the attribute.
          if (overrideAttr.ConstructorArguments.Length > 1) {
            for (var i = 1; i < overrideAttr.ConstructorArguments.Length; i++) {
              var dep = overrideAttr.ConstructorArguments[i].Value;
              if (dep is INamedTypeSymbol namedDep) {
                deps.Add(namedDep.Name);
              }
              else if (dep is TypeOfExpressionSyntax typeOf) {
                deps.Add(typeOf.Type.ToString());
              }
            }
          }

          return overrideText.Trim();
        }

        if (overrideAttr.ConstructorArguments[0].Value is ITypeSymbol typeSymbol) {
          return ToTypeScriptType(typeSymbol, deps);
        }

        throw new InvalidOperationException(
          $"Invalid TsTypeOverride attribute on {context?.Name ?? "unknown"}: " +
          $"expected a string or type, got {
            overrideAttr.ConstructorArguments[0].Value?.GetType().Name
          }"
        );
      }

      // If the attribute is not null, but we couldn't get the arg, try to get the AttributeSyntax
      // instance from it.
      if (overrideAttr.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax {
            ArgumentList.Arguments.Count: >= 1
          } overrideAttrSyntax) {
        var arg = overrideAttrSyntax.ArgumentList.Arguments[0].Expression;

        if (arg is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression)) {
          // Check if there were dependencies specified in the attribute.
          if (overrideAttrSyntax.ArgumentList.Arguments.Count > 1) {
            foreach (var argSyntax in overrideAttrSyntax.ArgumentList.Arguments.Skip(1)) {
              if (argSyntax.Expression is TypeOfExpressionSyntax typeOf) {
                if (typeOf.Type is IdentifierNameSyntax identifier) {
                  deps.Add(identifier.Identifier.Text);
                }
                else if (typeOf.Type is QualifiedNameSyntax qualified) {
                  deps.Add(qualified.ToString());
                }
              }
              else if (argSyntax.Expression is IdentifierNameSyntax identifier) {
                deps.Add(identifier.Identifier.Text);
              }
            }
          }

          var text = literal.Token.ValueText;
          return text.Trim();
        }

        throw new InvalidOperationException(
          $"Invalid TsTypeOverride attribute on {context?.Name ?? "unknown"}: " +
          $"expected a string, got {arg.Kind()} ({arg})"
        );
      }
    }

    switch (type) {
      // Handle nullable types
      case INamedTypeSymbol named when
        named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T ||
        named.SpecialType == SpecialType.System_Nullable_T:
        return ToTypeScriptType(named.TypeArguments[0], deps) + " | null";

      case INamedTypeSymbol { NullableAnnotation: NullableAnnotation.Annotated } named:
        return ToTypeScriptType(
                 // If the type is annotated, we use the type without the annotation to avoid
                 // infinite recursion.
                 named.WithNullableAnnotation(NullableAnnotation.NotAnnotated),
                 deps
               ) +
               " | null";

      // Handle arrays
      case IArrayTypeSymbol array:
        return $"{ToTypeScriptType(array.ElementType, deps)}[]";

      // Handle generic types
      case INamedTypeSymbol { IsGenericType: true } named: {
        var name = named.Name;

        switch (name) {
          case "Task" or "ValueTask" when
            named.TypeArguments.Length == 1:
            return $"Promise<{ToTypeScriptType(named.TypeArguments[0], deps)}>";
          case "List" or "IList" or "Array" or "JSArray" or "IEnumerable" when
            named.TypeArguments.Length == 1:
            return $"Array<{ToTypeScriptType(named.TypeArguments[0], deps)}>";
          case "Dictionary" when
            named.TypeArguments.Length == 2: {
            var keyType = ToTypeScriptType(named.TypeArguments[0], deps);
            var valType = ToTypeScriptType(named.TypeArguments[1], deps);
            return keyType == "string"
                     ? $"Record<string, {valType}>"
                     : $"Map<{keyType}, {valType}>";
          }
          case "Nullable" when named.TypeArguments.Length == 1: {
            return ToTypeScriptType(named.TypeArguments[0], deps);
          }
        }

        if (_customTypes.ContainsKey(name)) deps.Add(name);
        var typeArgs = string.Join(
          ", ",
          named.TypeArguments.Select(t => ToTypeScriptType(t, deps))
        );
        return $"{name}<{typeArgs}>";
      }

      // Handle non-generic types
      case INamedTypeSymbol named: {
        var name = named.Name;
        if (name is "Task") {
          return "Promise<void>";
        }

        // If it's not a value we expect to handle, jump to the default case.
        goto default;
      }

      default:
        if (_customTypes.ContainsKey(type.Name)) deps.Add(type.Name);

        // Handle primitive types
        return type.SpecialType switch {
          SpecialType.System_String  => "string",
          SpecialType.System_Boolean => "boolean",
          SpecialType.System_Int32
            or SpecialType.System_Int64
            or SpecialType.System_UInt32
            or SpecialType.System_UInt64
            or SpecialType.System_Double
            or SpecialType.System_Single => "number",
          SpecialType.System_Void => "void",
          _                       => type.Name
        };
    }
  }
}

file static class DocumentationResolver {
  private const int MaxColumn = 80;

  /// <summary>
  ///   Formats a display string to match the format of crefs in XML documentation comments.
  /// </summary>
  private static readonly SymbolDisplayFormat ExactParameterTypeFormat = new(
    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
    miscellaneousOptions:
    SymbolDisplayMiscellaneousOptions.None // critical: no special type keywords
  );


  public static string GetJsDoc(ISymbol symbol, Compilation compilation, string indent = "") {
    if (symbol == null) return string.Empty;

    var xml = symbol.GetDocumentationCommentXml(expandIncludes: true, cancellationToken: default)
      ?.Replace(oldChar: '\n', newChar: ' ')
      .Replace(oldChar: '\r', newChar: ' ');
    if (string.IsNullOrWhiteSpace(xml)) {
      // If the symbol is a method and has an overridden method, get the doc from the overridden method
      if (symbol is IMethodSymbol m &&
          m.OverriddenMethod is {} overridden) {
        return GetJsDoc(overridden, compilation, indent);
      }

      return string.Empty;
    }

    try {
      var doc = XElement.Parse("<root>" + xml + "</root>");

      // Unwrap the XML documentation from its root element. For instance
      // `<member name="...">...</member>`. We will put members of that element into the root.
      var docContents = doc.Elements().FirstOrDefault();
      if (docContents != null) {
        doc = new XElement("root", docContents.Elements());
      }

      var resolved = MergeInheritedDocIfPresent(doc, symbol, compilation);
      return ConvertToJsDoc(symbol, resolved, compilation, indent);
    }
    catch {
      return string.Empty;
    }
  }


  private static string ConvertToJsDoc(
    ISymbol symbol,
    XElement root,
    Compilation compilation,
    string indent
  ) {
    var lines = new List<string> { $"{indent}/**" };

    void Emit(string content) {
      foreach (var line in Wrap(content)) {
        lines.Add($"{indent} * {line}");
      }
    }

    // summary
    var summary = root.Element("summary")?.Nodes();
    if (summary != null) {
      var str = ProcessInlineNodes(summary, symbol, compilation).Trim();
      if (!string.IsNullOrWhiteSpace(str)) {
        Emit(str);
        lines.Add($"{indent} *");
      }
    }

    // param
    if (symbol is IMethodSymbol method) {
      foreach (var param in root.Elements("param")) {
        var name = param.Attribute("name")?.Value?.Trim();
        if (string.IsNullOrWhiteSpace(name)) continue;

        // Check the method symbol to see if the param has a default value.
        var paramSymbol = method.Parameters.FirstOrDefault(p => p.Name == name);
        if (paramSymbol is { HasExplicitDefaultValue: true }) {
          var defaultValue = (paramSymbol.ExplicitDefaultValue?.ToString() is "True" or "False"
                                ? paramSymbol.ExplicitDefaultValue.ToString().ToLowerInvariant()
                                : paramSymbol.ExplicitDefaultValue?.ToString()) ??
                             "null";

          name = $"[{name}={defaultValue}]";
        }

        var desc = ProcessInlineNodes(param.Nodes(), symbol, compilation).Trim();
        Emit($"@param {name} {desc}");
      }
    }

    // returns
    var returns = root.Element("returns")?.Nodes();
    if (returns != null) {
      var str = ProcessInlineNodes(returns, symbol, compilation).Trim();
      if (!string.IsNullOrWhiteSpace(str)) {
        Emit($"@returns {str}");
      }
    }

    // remarks
    var remarks = root.Element("remarks")?.Nodes();
    if (remarks != null) {
      lines.Add($"{indent} *");
      var str = ProcessInlineNodes(remarks, symbol, compilation).Trim();
      Emit(str);
    }

    lines.Add($"{indent} */");
    return string.Join("\n", lines);
  }


  private static string FormatCrefForJsDoc(ISymbol symbol) {
    return symbol switch {
      IMethodSymbol m when m.ContainingType != null =>
        $"{m.ContainingType.Name}.{ToCamel(m.Name)}",

      IPropertySymbol p when p.ContainingType != null =>
        $"{p.ContainingType.Name}.{ToCamel(p.Name)}",

      INamedTypeSymbol t => t.Name,

      _ => ToCamel(symbol.Name)
    };
  }


  private static string FormatList(XElement list, ISymbol context, Compilation compilation) {
    var type    = list.Attribute("type")?.Value?.ToLowerInvariant() ?? "bullet";
    var items   = list.Elements("item").ToList();
    var builder = new StringBuilder();

    var header = list.Element("listheader");
    if (header != null) {
      var txt = ProcessInlineNodes(header.Nodes(), context, compilation).Trim();
      if (!string.IsNullOrWhiteSpace(txt)) {
        builder.AppendLine(txt);
      }
    }

    for (var i = 0; i < items.Count; i++) {
      var txt = ProcessInlineNodes(items[i].Nodes(), context, compilation).Trim();
      if (type == "number") {
        builder.AppendLine($"{i + 1}. {txt}");
      }
      else {
        builder.AppendLine($"- {txt}");
      }
    }

    return builder.ToString();
  }


  private static ISymbol? GetImplementedInterfaceMember(ISymbol symbol) {
    var containingType = symbol.ContainingType;
    if (containingType == null) return null;

    foreach (var @interface in containingType.AllInterfaces) {
      foreach (var member in @interface.GetMembers()) {
        var implementation = containingType.FindImplementationForInterfaceMember(member);
        if (SymbolEqualityComparer.Default.Equals(implementation, symbol)) {
          return member;
        }
      }
    }

    return null;
  }


  private static XElement MergeInheritedDocIfPresent(
    XElement root,
    ISymbol symbol,
    Compilation compilation
  ) {
    var inheritEl = root.Element("inheritdoc");
    if (inheritEl == null) return root;

    XElement? baseDocXml = null;

    // cref override
    var crefAttr = inheritEl.Attribute("cref")?.Value;
    if (!string.IsNullOrWhiteSpace(crefAttr)) {
      var resolved = ResolveCrefSymbol(crefAttr!, compilation);
      baseDocXml = TryGetXmlElement(resolved);
    }
    else {
      ISymbol? baseSymbol = symbol switch {
        IMethodSymbol m when m.OverriddenMethod != null => m.OverriddenMethod,
        IMethodSymbol m when m.ExplicitInterfaceImplementations.Length > 0 => m
          .ExplicitInterfaceImplementations[0],
        IPropertySymbol p when p.OverriddenProperty != null => p.OverriddenProperty,
        IPropertySymbol p when p.ExplicitInterfaceImplementations.Length > 0 => p
          .ExplicitInterfaceImplementations[0],
        _ => null
      };

      baseDocXml = TryGetXmlElement(baseSymbol);
    }

    if (baseDocXml == null) {
      // If we couldn't resolve the cref, try to find the implemented interface member
      // and get its documentation, if there is one.
      var implementedMember = GetImplementedInterfaceMember(symbol);
      if (implementedMember != null) {
        baseDocXml = TryGetXmlElement(implementedMember);
      }
    }

    if (baseDocXml == null) return root;

    // If the baseDocXml is a <member> element, we need to unwrap it.
    if (baseDocXml.Element("member") is {} member) {
      baseDocXml = new XElement("root", member.Elements());
    }

    var merged = new XElement("root");

    // Copy inherited documentation
    foreach (var el in baseDocXml.Elements()) {
      merged.Add(new XElement(el));
    }

    // Override with elements from the derived doc (except <inheritdoc>)
    foreach (var el in root.Elements().Where(e => e.Name.LocalName != "inheritdoc")) {
      merged.Elements(el.Name).Remove();
      merged.Add(new XElement(el));
    }

    return merged;
  }


  private static string NormalizeCref(string cref, ISymbol context, Compilation compilation) {
    if (string.IsNullOrWhiteSpace(cref)) return cref;

    if (cref.StartsWith("T:") ||
        cref.StartsWith("M:") ||
        cref.StartsWith("P:") ||
        cref.StartsWith("F:") ||
        cref.StartsWith("E:")) {
      cref = cref.Substring(2);
    }

    // Try to find the method name (possibly with arguments) by scanning from the end
    var openParen = cref.LastIndexOf('(');
    var lastDot   = cref.LastIndexOf(value: '.', openParen > 0 ? openParen : cref.Length - 1);

    if (lastDot < 0) return cref;

    var containerName = cref.Substring(startIndex: 0, lastDot);
    var memberName    = cref.Substring(lastDot + 1);

    var containerSymbol = compilation.GetTypeByMetadataName(containerName);
    if (containerSymbol == null) return cref;

    // Method overloads may not match by name alone, but for normalization
    // purposes we only lowercase the name
    var simpleMemberName = memberName.Contains('(')
                             ? memberName.Substring(startIndex: 0, memberName.IndexOf('('))
                             : memberName;

    var memberSymbol = containerSymbol.GetMembers(simpleMemberName).FirstOrDefault();
    if (memberSymbol is IMethodSymbol or IPropertySymbol) {
      var newMemberName =
        char.ToLowerInvariant(simpleMemberName[0]) + simpleMemberName.Substring(1);
      var suffix = memberName.Contains('(')
                     ? memberName.Substring(memberName.IndexOf('('))
                     : "";

      return $"{containerName}.{newMemberName}{suffix}";
    }

    return cref;
  }


  private static string ProcessInlineCodeNode(
    XElement code,
    ISymbol context,
    Compilation compilation
  ) {
    // If the code element has a langword attribute, we use that as the value.
    if (code.Attribute("langword") is {} langword) {
      return $"`{langword.Value.ToLowerInvariant()}`";
    }

    // Otherwise, we use the text content of the element.
    return $"`{code.Value.Trim()}`";
  }


  private static string ProcessInlineNode(XNode node, ISymbol context, Compilation compilation) {
    return node switch {
      XText t => t.Value,
      XElement e => e.Name.LocalName switch {
        "paramref" => $"{{@link {e.Attribute("name")?.Value}}}",
        "see"      => ResolveSeeTag(e, context, compilation),
        "c"        => ProcessInlineCodeNode(e, context, compilation),
        "list"     => "\n" + FormatList(e, context, compilation) + "\n\n",
        "para"     => ProcessParagraphNode(e, context, compilation),
        _          => ProcessInlineNodes(e.Nodes(), context, compilation)
      },
      _ => string.Empty
    };
  }


  private static string ProcessInlineNodes(
    IEnumerable<XNode> nodes,
    ISymbol context,
    Compilation compilation
  ) {
    return string.Concat(nodes.Select(n => ProcessInlineNode(n, context, compilation)));
  }


  private static string ProcessParagraphNode(
    XElement para,
    ISymbol context,
    Compilation compilation
  ) {
    return
      "\n\n" +
      string.Concat(para.Nodes().Select(n => ProcessInlineNode(n, context, compilation))) +
      "\n\n";
  }


  private static ISymbol? ResolveCrefSymbol(
    string cref,
    Compilation compilation,
    ISymbol? context = null
  ) {
    // Specifies if the cref represents a type, like a class.
    var isType = false;

    if (cref.StartsWith("T:")) {
      isType = true;
    }

    if (cref.StartsWith("T:") ||
        cref.StartsWith("M:") ||
        cref.StartsWith("P:") ||
        cref.StartsWith("F:") ||
        cref.StartsWith("E:") ||
        // Handle when the type couldn't be resolved. We have fallback logic to look it up
        // dynamically.
        cref.StartsWith("!:")) {
      cref = cref.Substring(2);
    }

    INamedTypeSymbol? containerSymbol;
    string            containerName;
    string            memberName;
    string?           paramList = null;

    // If the cref is not a type, we need to get the containing type of the provided member.
    if (!isType) {
      // Handle method parameters: "Namespace.Type.Method(Type1,Type2)"
      var paramStart = cref.IndexOf('(');
      paramList = paramStart >= 0 ? cref.Substring(paramStart) : null;
      var fullName = paramStart >= 0 ? cref.Substring(startIndex: 0, paramStart) : cref;

      var lastDot = fullName.LastIndexOf('.');
      if (lastDot == -1) return null;

      containerName = fullName.Substring(startIndex: 0, lastDot);
      memberName    = fullName.Substring(lastDot + 1);
    }
    else {
      containerName = cref;
      memberName    = string.Empty;
    }

    containerSymbol = compilation.GetTypeByMetadataName(containerName);

    // If the container symbol is null, we'll try to look it up dynamically.
    if (containerSymbol == null) {
      if (context is not null) {
        // Get the syntax node for the context symbol.
        var syntaxRef = context.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef != null) {
          var syntaxNode    = syntaxRef.GetSyntax();
          var semanticModel = compilation.GetSemanticModel(syntaxNode.SyntaxTree);

          // Try to find the container symbol in the current context.
          try {
            containerSymbol = semanticModel.LookupSymbols(syntaxNode.SpanStart, name: containerName)
              .OfType<INamedTypeSymbol>()
              .FirstOrDefault();
          }
          catch (Exception ex) {
            return null;
          }
        }
      }

      // If that didn't find the symbol either, return null.
      if (containerSymbol is null) {
        return null;
      }
    }

    // If the cref represented a type or there was no member name, we can just return the container symbol.
    if (isType || memberName.Length == 0) {
      return containerSymbol;
    }

    if (paramList is not null) {
      var methods = containerSymbol.GetMembers(memberName).OfType<IMethodSymbol>();
      foreach (var method in methods) {
        var sig = string.Join(
          ",",
          method.Parameters.Select(p => p.Type.ToDisplayString(ExactParameterTypeFormat))
        );
        if (string.Equals($"({sig})", paramList, StringComparison.Ordinal)) {
          return method;
        }
      }

      // If not found on this type, check base types
      var baseType = containerSymbol.BaseType;
      while (baseType != null) {
        methods = baseType.GetMembers(memberName).OfType<IMethodSymbol>();
        foreach (var method in methods) {
          var sig = string.Join(
            ",",
            method.Parameters.Select(p =>
              p.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)
            )
          );
          if (string.Equals($"({sig})", paramList, StringComparison.Ordinal)) {
            return method;
          }
        }

        baseType = baseType.BaseType;
      }

      return null;
    }

    // Direct match on fields, properties, or parameterless methods
    var member = containerSymbol.GetMembers(memberName).FirstOrDefault();
    if (member is not null) return member;

    // Check base types
    var current = containerSymbol.BaseType;
    while (current is not null) {
      member = current.GetMembers(memberName).FirstOrDefault();
      if (member is not null) return member;
      current = current.BaseType;
    }

    return null;
  }


  private static string ResolveSeeTag(XElement see, ISymbol context, Compilation compilation) {
    var langword = see.Attribute("langword")?.Value;

    if (!string.IsNullOrWhiteSpace(langword)) {
      return ProcessInlineCodeNode(see, context, compilation);
    }

    var cref = see.Attribute("cref")?.Value;
    if (string.IsNullOrWhiteSpace(cref)) return string.Empty;

    var symbol = ResolveCrefSymbol(cref, compilation, context);
    if (symbol == null) return string.Empty;

    var jsDocLink = FormatCrefForJsDoc(symbol);
    return $"{{@link {jsDocLink}}}";
  }


  private static XElement? TryGetXmlElement(ISymbol? symbol) {
    if (symbol == null) return null;
    var xml = symbol.GetDocumentationCommentXml(expandIncludes: true, cancellationToken: default)
      ?.Replace(oldChar: '\n', newChar: ' ')
      .Replace(oldChar: '\r', newChar: ' ');
    if (string.IsNullOrWhiteSpace(xml)) return null;

    try {
      return XElement.Parse("<root>" + xml + "</root>");
    }
    catch {
      return null;
    }
  }


  private static IEnumerable<string> Wrap(string text) {
    // Maximum line width for wrapping, accounting for JSDoc indentation (e.g., ' * ').
    var maxWidth = MaxColumn - 4;

    // Track whether the last line yielded was blank.
    var previousLineWasBlank = false;

    // Split the input text into logical lines to preserve explicit newlines.
    foreach (var line in text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)) {
      if (string.IsNullOrWhiteSpace(line)) {
        // Only yield a blank line if the previous line was not also blank.
        if (!previousLineWasBlank) {
          yield return string.Empty;
          previousLineWasBlank = true;
        }

        continue;
      }

      // Non-blank line: reset blank-line tracking.
      previousLineWasBlank = false;

      // Split the line into words and wrap.
      var words   = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
      var current = new StringBuilder();

      foreach (var word in words) {
        // If appending the word would exceed the max width, flush the line.
        if (current.Length + word.Length + 1 > maxWidth) {
          yield return current.ToString().TrimEnd();
          current.Clear();
        }

        current.Append(word + " ");
      }

      if (current.Length > 0) {
        yield return current.ToString().TrimEnd();
      }
    }
  }
}
