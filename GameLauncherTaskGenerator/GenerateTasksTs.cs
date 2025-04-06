using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GameLauncherTaskGenerator;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
///   An MSBuild task that generates an entry‐point Tasks.ts file from a set of C# source files.
///   In addition to converting all public methods in the partial class “Tasks”, it also scans
///   for any types (classes/interfaces) marked with a special attribute (default “TypeScriptExport”)
///   and generates separate TS files for them – recursively adding import statements as needed.
/// </summary>
public class GenerateTasksTs : Task {
  /// <summary>
  ///   The directory containing the C# source files.
  /// </summary>
  [Required]
  public string SourceDirectory { get; set; }

  /// <summary>
  ///   The directory where the generated TypeScript files will be written.
  /// </summary>
  [Required]
  public string OutputDir { get; set; }

  /// <summary>
  ///   The name (or partial name) of the attribute that marks a type for export.
  /// </summary>
  public string ExportAttributeName { get; set; } = "TypeScriptExport";

  // New property to mark entry point classes.
  public string EntryPointAttributeName { get; set; } = "EntryPointExport";


  public override bool Execute() {
    // if (!Debugger.IsAttached) {
    //   Debugger.Launch();
    // }

    try {
      var csFiles     = Directory.GetFiles(SourceDirectory, "*.cs", SearchOption.AllDirectories);
      var syntaxTrees = new List<SyntaxTree>();
      // Dictionary: entry point class name => list of its methods
      var entryPointClasses =
        new Dictionary<string,
          List<(MethodDeclarationSyntax method, SemanticModel? semanticModel)>>(
          StringComparer.Ordinal
        );
      // Dictionary: entry point class name => its class declaration SyntaxNode
      var entryPointClassNodes =
        new Dictionary<string, (ClassDeclarationSyntax cls, SemanticModel? semanticModel)>(
          StringComparer.Ordinal
        );
      var entryPointDelegates =
        new Dictionary<string, List<(DelegateDeclarationSyntax del, SemanticModel? semanticModel)>>(
          StringComparer.Ordinal
        );
      // Also use customTypes for non-entry-point types.
      var customTypes =
        new Dictionary<string, List<(TypeDeclarationSyntax type, SemanticModel? semanticModel)>>(
          StringComparer.Ordinal
        );
      // Global delegates from outside any class.
      var globalDelegates =
        new Dictionary<string, (DelegateDeclarationSyntax del, SemanticModel? semanticModel)>(
          StringComparer.Ordinal
        );

      foreach (var file in csFiles) {
        var code = File.ReadAllText(file);
        var tree = CSharpSyntaxTree.ParseText(code);
        syntaxTrees.Add(tree);
      }

      var compilation = CSharpCompilation.Create(
        "DocGenTemp",
        syntaxTrees,
        new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
      );

      foreach (var tree in syntaxTrees) {
        var root          = tree.GetRoot();
        var semanticModel = compilation.GetSemanticModel(tree);

        // Find entry point classes: either the class is named "Tasks" or has the EntryPoint attribute.
        var candidateClasses = root.DescendantNodes()
          .OfType<ClassDeclarationSyntax>()
          .Where(
            cls =>
              cls.Identifier.Text == "Tasks" ||
              cls.AttributeLists.Any(
                al => al.Attributes.Any(a => a.Name.ToString().Contains(EntryPointAttributeName))
              )
          );
        foreach (var cls in candidateClasses) {
          var className = cls.Identifier.Text;
          if (!entryPointClasses.ContainsKey(className)) {
            entryPointClasses[className]    = [];
            entryPointDelegates[className]  = [];
            entryPointClassNodes[className] = (cls, semanticModel); // Save the SyntaxNode.
          }

          entryPointClasses[className]
            .AddRange(
              cls.Members.OfType<MethodDeclarationSyntax>()
                .Select(method => (method, semanticModel))!
            );
          entryPointDelegates[className]
            .AddRange(
              cls.Members.OfType<DelegateDeclarationSyntax>().Select(del => (del, semanticModel))!
            );
        }

        // Also scan for types marked with ExportAttribute (non-entry point custom types).
        foreach (var typeDecl in root.DescendantNodes().OfType<TypeDeclarationSyntax>()) {
          var typeName = typeDecl.Identifier.Text;
          if (HasExportAttribute(typeDecl)) {
            if (!customTypes.ContainsKey(typeName)) {
              customTypes.Add(typeName, [(typeDecl, semanticModel)]);
            }
            else {
              customTypes[typeName].Add((typeDecl, semanticModel));
            }
          }
          // Add unmarked partial declarations if the type already exists.
          else if (typeDecl.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PartialKeyword)) &&
                   customTypes.ContainsKey(typeName)) {
            customTypes[typeName].Add((typeDecl, semanticModel));
          }
        }

        // Global delegates.
        foreach (var del in root.DescendantNodes()
                   .OfType<DelegateDeclarationSyntax>()
                   .Where(d => !(d.Parent is ClassDeclarationSyntax))) {
          var delName = del.Identifier.Text;
          if (!globalDelegates.ContainsKey(delName)) {
            globalDelegates.Add(delName, (del, semanticModel));
          }
        }
      }

      if (!entryPointClasses.Any()) {
        Log.LogMessage(MessageImportance.High, "No entry point classes found.");
        return true;
      }

      // Build a quick lookup for custom type names.
      var customTypeNames = new HashSet<string>(customTypes.Keys, StringComparer.Ordinal);

      foreach (var kvp in entryPointClasses) {
        var className        = kvp.Key;
        var methods          = kvp.Value;
        var delegatesInClass = entryPointDelegates[className];

        // Determine required custom types for this entry point.
        var requiredTypes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var method in methods) {
          foreach (var type in TsTypeGenerator.ExtractAllBaseTypes(
                     method.method.ReturnType.ToString()
                   )) {
            if (customTypeNames.Contains(type)) requiredTypes.Add(type);
          }

          foreach (var param in method.method.ParameterList.Parameters) {
            foreach (var type in TsTypeGenerator.ExtractAllBaseTypes(param.Type.ToString())) {
              if (customTypeNames.Contains(type)) requiredTypes.Add(type);
            }
          }
        }

        // Build file content for the entry point TS file.
        var sb = new StringBuilder();
        sb.AppendLine("// This file is auto-generated. Do not modify manually.");

        // Use the SyntaxNode for the class to generate file-level documentation.
        var classDoc = JsDocGenerator.GenerateJsDoc(
          entryPointClassNodes[className].cls,
          entryPointClassNodes[className].semanticModel
        );
        if (!string.IsNullOrWhiteSpace(classDoc)) {
          sb.AppendLine(classDoc);
        }

        sb.AppendLine();
        foreach (var typeName in requiredTypes) {
          sb.AppendLine($"import {{ {typeName} }} from \"./{typeName}\";");
        }

        sb.AppendLine();

        // Group methods by name.
        var methodGroups = methods.GroupBy(m => m.method.Identifier.Text);
        foreach (var group in methodGroups) {
          var origName = group.Key;
          // Use lower-cased first letter for TS function name.
          var tsName = char.ToLower(origName[0]) + origName.Substring(1);
          var validOverloads = group.Where(
              m =>
                !m.method.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PrivateKeyword)) &&
                origName != "InjectIntoEngine" &&
                !m.method.AttributeLists.Any(
                  al => al.Attributes.Any(a => a.Name.ToString().EndsWith("HideFromTypeScript"))
                )
            )
            .ToList();

          if (!validOverloads.Any()) {
            continue;
          }

          if (validOverloads.Count == 1) {
            var method = validOverloads.First();
            var jsDoc  = JsDocGenerator.GenerateJsDoc(method.method, method.semanticModel);
            sb.AppendLine(jsDoc);

            var isAsync = method.method.Modifiers.Any(mod => mod.IsKind(SyntaxKind.AsyncKeyword));
            var retType = CSharpTypeScriptConverter.Convert(method.method.ReturnType);
            var parameters = string.Join(
              ", ",
              method.method.ParameterList.Parameters.Select(
                p =>
                  $"{
                    p.Identifier.Text
                  }{
                    (p.Default != null ? "?" : "")
                  }: {
                    CSharpTypeScriptConverter.Convert(p.Type)
                  }"
              )
            );
            sb.Append("export ");
            if (isAsync) sb.Append("async ");
            sb.AppendLine($"function {tsName}({parameters}): {retType} {{");
            sb.AppendLine(
              "    // @ts-expect-error - This function is injected into the engine dynamically."
            );
            sb.Append("    return __" + className + "." + origName + "(");
            sb.Append(
              string.Join(
                ", ",
                method.method.ParameterList.Parameters.Select(p => p.Identifier.Text)
              )
            );
            sb.AppendLine(");");
            sb.AppendLine("}");
            sb.AppendLine();
          }
          else {
            foreach (var method in validOverloads) {
              var jsDoc = JsDocGenerator.GenerateJsDoc(method.method, method.semanticModel);
              sb.AppendLine(jsDoc);
              var retType = CSharpTypeScriptConverter.Convert(method.method.ReturnType);
              var parameters = string.Join(
                ", ",
                method.method.ParameterList.Parameters.Select(
                  p =>
                    $"{
                      p.Identifier.Text
                    }{
                      (p.Default != null ? "?" : "")
                    }: {
                      CSharpTypeScriptConverter.Convert(p.Type)
                    }"
                )
              );
              sb.AppendLine($"export function {tsName}({parameters}): {retType};");
            }

            // Combined implementation.
            var maxParamCount = validOverloads.Max(m => m.method.ParameterList.Parameters.Count);
            var unionParams   = new List<string>();
            for (var i = 0; i < maxParamCount; i++) {
              var paramNames = new List<string>();
              var types      = new HashSet<string>();
              var isOptional = false;
              foreach (var method in validOverloads) {
                if (method.method.ParameterList.Parameters.Count > i) {
                  var p = method.method.ParameterList.Parameters[i];
                  paramNames.Add(p.Identifier.Text);
                  types.Add(CSharpTypeScriptConverter.Convert(p.Type));
                  if (p.Default != null) isOptional = true;
                }
                else {
                  isOptional = true;
                }
              }

              var pname     = paramNames.First();
              var unionType = string.Join(" | ", types);
              if (isOptional && !unionType.Contains("undefined")) {
                unionType += " | undefined";
              }

              unionParams.Add($"{pname}{(isOptional ? "?" : "")}: {unionType}");
            }

            var retTypes = new HashSet<string>(
              validOverloads.Select(m => CSharpTypeScriptConverter.Convert(m.method.ReturnType))
            );
            var unionReturn = retTypes.Count == 1 ? retTypes.First() : string.Join(" | ", retTypes);
            var anyAsync = validOverloads.Any(
              m => m.method.Modifiers.Any(mod => mod.IsKind(SyntaxKind.AsyncKeyword))
            );
            sb.Append("export ");
            if (anyAsync) sb.Append("async ");
            sb.Append(
              $"function {tsName}(...args: [{string.Join(", ", unionParams)}]): {unionReturn} {{"
            );
            sb.AppendLine();
            sb.AppendLine(
              "    // @ts-expect-error - This function is injected into the engine dynamically."
            );
            sb.AppendLine($"    return __{className}.{origName}(...args);");
            sb.AppendLine("}");
            sb.AppendLine();
          }
        }

        // Append delegates declared in this entry point.
        if (delegatesInClass.Any()) {
          sb.AppendLine();
          foreach (var del in delegatesInClass) {
            var customTypesForTs = customTypes.ToDictionary(
              kvp => kvp.Key,
              kvp => kvp.Value.First()
            );
            var delTs = TsTypeGenerator.GenerateTsForDelegate(del.del, customTypesForTs, out var _);
            sb.AppendLine(delTs);
          }
        }

        // Write the TS file for this entry point.
        var outputPath = Path.Combine(OutputDir, className + ".ts");
        File.WriteAllText(outputPath, sb.ToString());
        Log.LogMessage(
          MessageImportance.High,
          $"Generated TypeScript entry point for {className} at {outputPath}"
        );
      }

      // Process global delegates.
      foreach (var kvp in globalDelegates) {
        var del = kvp.Value;
        var customTypesForTs = customTypes.ToDictionary(ct => ct.Key, ct => ct.Value.First());
        var tsCode = TsTypeGenerator.GenerateTsForDelegate(del.del, customTypesForTs, out var _);
        var outputPath = Path.Combine(OutputDir, del.del.Identifier.Text + ".ts");
        File.WriteAllText(outputPath, tsCode);
        Log.LogMessage(
          MessageImportance.High,
          $"Generated TypeScript file for delegate {del.del.Identifier.Text} at {outputPath}"
        );
      }

      // Recursively generate TS files for all required custom types.
      foreach (var typeName in customTypes.Keys) {
        GenerateCustomTypeTs(
          typeName,
          customTypes,
          customTypeNames,
          OutputDir,
          new HashSet<string>(StringComparer.Ordinal)
        );
      }

      return true;
    }
    catch (Exception ex) {
      Log.LogError($"Error generating TypeScript files: {ex.Message}");
      return false;
    }
  }


  /// <summary>
  ///   Recursively generates a TypeScript file for the custom type (and its dependencies) if not already
  ///   generated.
  /// </summary>
  private void GenerateCustomTypeTs(
    string typeName,
    Dictionary<string, List<(TypeDeclarationSyntax decl, SemanticModel? semanticModel)>>
      customTypes,
    HashSet<string> customTypeNames,
    string outputDir,
    HashSet<string> generatedTypes
  ) {
    if (generatedTypes.Contains(typeName)) {
      return; // Already generated.
    }

    if (!customTypes.TryGetValue(typeName, out var typeDeclList)) {
      Log.LogWarning(
        $"Referenced type '{
          typeName
        }' was not marked with the export attribute '{
          ExportAttributeName
        }'."
      );
      return;
    }

    // Build a dependency mapping that merges partial declarations.
    var customTypesForTs =
      new Dictionary<string, (TypeDeclarationSyntax, SemanticModel? semanticModel)>(
        StringComparer.Ordinal
      );
    foreach (var kvp in customTypes) {
      if (kvp.Value.Count > 1) {
        // Merge members from all declarations.
        var first         = kvp.Value.First();
        var mergedMembers = new List<MemberDeclarationSyntax>();
        foreach (var decl in kvp.Value) {
          mergedMembers.AddRange(decl.decl.Members);
        }

        var mergedDecl = first.decl.WithMembers(SyntaxFactory.List(mergedMembers));
        customTypesForTs[kvp.Key] = (mergedDecl, first.semanticModel);
      }
      else {
        customTypesForTs[kvp.Key] = kvp.Value.First();
      }
    }

    string          tsCode;
    HashSet<string> dependencies;
    // If any declaration is partial, aggregate all declarations.
    if (typeDeclList.Any(
          decl => decl.decl.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PartialKeyword))
        )) {
      tsCode = TsTypeGenerator.GenerateTsForPartialClass(
        typeDeclList,
        customTypesForTs,
        out dependencies
      );
    }
    else {
      tsCode = TsTypeGenerator.GenerateTs(typeDeclList.First(), customTypesForTs, out dependencies);
    }

    // Prepend import statements for any dependencies.
    var importBuilder = new StringBuilder();
    foreach (var dep in dependencies) {
      if (customTypeNames.Contains(dep) &&
          dep != typeName) {
        importBuilder.AppendLine($"import {{ {dep} }} from \"./{dep}\";");
      }
    }

    if (importBuilder.Length > 0) {
      tsCode = importBuilder + "\n" + tsCode;
    }

    var outputPath = Path.Combine(outputDir, typeName + ".ts");
    File.WriteAllText(outputPath, tsCode);
    Log.LogMessage(
      MessageImportance.High,
      $"Generated TypeScript file for {typeName} at {outputPath}"
    );
    generatedTypes.Add(typeName);

    // Recursively generate files for any dependencies.
    foreach (var dep in dependencies) {
      if (customTypeNames.Contains(dep)) {
        GenerateCustomTypeTs(dep, customTypes, customTypeNames, outputDir, generatedTypes);
      }
    }
  }


  /// <summary>
  ///   Determines whether the type declaration is marked with the export attribute.
  /// </summary>
  private bool HasExportAttribute(TypeDeclarationSyntax typeDecl) {
    foreach (var attrList in typeDecl.AttributeLists) {
      foreach (var attr in attrList.Attributes) {
        if (attr.ToString().Contains(ExportAttributeName)) {
          return true;
        }
      }
    }

    return false;
  }
}
