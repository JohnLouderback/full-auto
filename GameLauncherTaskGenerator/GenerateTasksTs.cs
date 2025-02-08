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


  public override bool Execute() {
    try {
      // Gather all C# files.
      var csFiles      = Directory.GetFiles(SourceDirectory, "*.cs", SearchOption.AllDirectories);
      var tasksMethods = new List<MethodDeclarationSyntax>();
      var tasksDelegates =
        new List<DelegateDeclarationSyntax>();
      // Dictionary: type name => type declaration (class or interface) that is marked for export.
      var customTypes = new Dictionary<string, TypeDeclarationSyntax>(StringComparer.Ordinal);
      // Collection for global delegates.
      var globalDelegates =
        new Dictionary<string, DelegateDeclarationSyntax>(StringComparer.Ordinal);

      // Process every file.
      foreach (var file in csFiles) {
        var code       = File.ReadAllText(file);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root       = syntaxTree.GetRoot();

        // Find all parts of the partial "Tasks" class.
        var tasksClasses = root.DescendantNodes()
          .OfType<ClassDeclarationSyntax>()
          .Where(
            cls => cls.Identifier.Text == "Tasks" &&
                   cls.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
          );
        foreach (var tasksClass in tasksClasses) {
          tasksMethods.AddRange(tasksClass.Members.OfType<MethodDeclarationSyntax>());
          tasksDelegates.AddRange(
            tasksClass.Members.OfType<DelegateDeclarationSyntax>()
          );
        }

        // Find all types (classes or interfaces) marked with the special attribute.
        var typeDeclarations = root.DescendantNodes().OfType<TypeDeclarationSyntax>();
        foreach (var typeDecl in typeDeclarations) {
          if (HasExportAttribute(typeDecl)) {
            var typeName = typeDecl.Identifier.Text;
            if (!customTypes.ContainsKey(typeName)) {
              customTypes.Add(typeName, typeDecl);
            }
          }
        }

        // Collect delegates declared outside of a class.
        var delegateDecls = root.DescendantNodes()
          .OfType<DelegateDeclarationSyntax>()
          .Where(d => !(d.Parent is ClassDeclarationSyntax));
        foreach (var del in delegateDecls) {
          var delName = del.Identifier.Text;
          if (!globalDelegates.ContainsKey(delName)) {
            globalDelegates.Add(delName, del);
          }
        }
      }

      if (!tasksMethods.Any()) {
        Log.LogMessage(MessageImportance.High, "No methods found in partial class Tasks.");
        return true;
      }

      // Build a quick lookup for custom type names.
      var customTypeNames = new HashSet<string>(customTypes.Keys, StringComparer.Ordinal);

      // Determine which custom types are referenced by the Tasks methods.
      var requiredTypes = new HashSet<string>(StringComparer.Ordinal);
      foreach (var method in tasksMethods) {
        // Process the return type.
        var returnType = method.ReturnType.ToString();
        foreach (var type in TsTypeGenerator.ExtractAllBaseTypes(returnType)) {
          if (customTypeNames.Contains(type)) {
            requiredTypes.Add(type);
          }
        }

        // Process each parameter type.
        foreach (var param in method.ParameterList.Parameters) {
          var paramType = param.Type.ToString();
          foreach (var type in TsTypeGenerator.ExtractAllBaseTypes(paramType)) {
            if (customTypeNames.Contains(type)) {
              requiredTypes.Add(type);
            }
          }
        }
      }

      // Build the Tasks.ts entry point.
      var tasksTsBuilder = new StringBuilder();
      tasksTsBuilder.AppendLine("// This file is auto-generated. Do not modify manually.");

      // Insert file-level documentation taken from the Tasks class (if available).
      // (Assumes that the Tasks class documentation is available from one of its parts.)
      var tasksClassDoc = ""; // ...retrieve class-level JsDoc if needed...
      if (!string.IsNullOrWhiteSpace(tasksClassDoc)) {
        tasksTsBuilder.AppendLine(tasksClassDoc);
      }

      tasksTsBuilder.AppendLine();
      // Import all required custom types.
      foreach (var typeName in requiredTypes) {
        tasksTsBuilder.AppendLine($"import {{ {typeName} }} from \"./{typeName}\";");
      }

      tasksTsBuilder.AppendLine();

      // Instead of wrapping methods in a class, generate each as an exported function.
      var methodGroups = tasksMethods.GroupBy(m => m.Identifier.Text);
      foreach (var group in methodGroups) {
        var origName = group.Key;
        var jsName   = char.ToLower(origName[0]) + origName.Substring(1);

        // Filter out overloads that are private, marked with HideFromTypeScript, or named "InjectIntoEngine".
        var validOverloads = group.Where(
            method =>
              !method.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)) &&
              origName != "InjectIntoEngine" &&
              !method.AttributeLists.Any(
                al => al.Attributes.Any(a => a.Name.ToString().EndsWith("HideFromTypeScript"))
              )
          )
          .ToList();

        if (!validOverloads.Any()) {
          continue;
        }

        // For a single valid overload.
        if (validOverloads.Count == 1) {
          var method = validOverloads.First();
          var jsDoc  = JsDocGenerator.GenerateJsDoc(method);
          tasksTsBuilder.AppendLine(jsDoc);

          var isAsync = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
          var retType = CSharpTypeScriptConverter.Convert(method.ReturnType);
          var parameters = string.Join(
            ", ",
            method.ParameterList.Parameters.Select(
              p => {
                // Remove initializer; mark optional if a default value exists.
                return $"{
                  p.Identifier.Text
                }{
                  (p.Default != null ? "?" : "")
                }: {
                  CSharpTypeScriptConverter.Convert(p.Type)
                }";
              }
            )
          );
          tasksTsBuilder.Append("export ");
          if (isAsync) tasksTsBuilder.Append("async ");
          tasksTsBuilder.AppendLine($"function {jsName}({parameters}): {retType} {{");
          tasksTsBuilder.AppendLine(
            "    // @ts-expect-error - This function is injected into the engine dynamically."
          );
          tasksTsBuilder.Append("    return __Tasks." + origName + "(");
          tasksTsBuilder.Append(
            string.Join(", ", method.ParameterList.Parameters.Select(p => p.Identifier.Text))
          );
          tasksTsBuilder.AppendLine(");");
          tasksTsBuilder.AppendLine("}");
          tasksTsBuilder.AppendLine();
        }
        else {
          // Generate overload signatures.
          foreach (var method in validOverloads) {
            var jsDoc = JsDocGenerator.GenerateJsDoc(method);
            tasksTsBuilder.AppendLine(jsDoc);
            var retType = CSharpTypeScriptConverter.Convert(method.ReturnType);
            var parameters = string.Join(
              ", ",
              method.ParameterList.Parameters.Select(
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
            tasksTsBuilder.AppendLine($"export function {jsName}({parameters}): {retType};");
          }

          // Build combined implementation.
          var maxParamCount = validOverloads.Max(m => m.ParameterList.Parameters.Count);
          var unionParams   = new List<string>();
          for (var i = 0; i < maxParamCount; i++) {
            var paramNames = new List<string>();
            var types      = new HashSet<string>();
            var isOptional = false;
            foreach (var method in validOverloads) {
              if (method.ParameterList.Parameters.Count > i) {
                var p = method.ParameterList.Parameters[i];
                paramNames.Add(p.Identifier.Text);
                types.Add(CSharpTypeScriptConverter.Convert(p.Type));
                if (p.Default != null) {
                  isOptional = true;
                }
              }
              else {
                isOptional = true;
              }
            }

            var paramName = paramNames.First();
            var unionType = string.Join(" | ", types);
            if (isOptional && !unionType.Contains("undefined")) {
              unionType += " | undefined";
            }

            // Mark parameter optional if needed.
            unionParams.Add($"{paramName}{(isOptional ? "?" : "")}: {unionType}");
          }

          var retTypes = new HashSet<string>(
            validOverloads.Select(m => CSharpTypeScriptConverter.Convert(m.ReturnType))
          );
          var unionReturn = retTypes.Count == 1 ? retTypes.First() : string.Join(" | ", retTypes);
          // Assume async if any overload is async.
          var anyAsync = validOverloads.Any(
            m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.AsyncKeyword))
          );
          tasksTsBuilder.Append("export ");
          if (anyAsync) tasksTsBuilder.Append("async ");
          tasksTsBuilder.Append($"function {jsName}(...args: [");
          tasksTsBuilder.Append(string.Join(", ", unionParams));
          tasksTsBuilder.AppendLine($"]): {unionReturn} {{");
          tasksTsBuilder.AppendLine(
            "    // @ts-expect-error - This function is injected into the engine dynamically."
          );
          tasksTsBuilder.AppendLine($"    return __Tasks.{origName}(...args);");
          tasksTsBuilder.AppendLine("}");
          tasksTsBuilder.AppendLine();
        }
      }

      // Append delegates declared within Tasks.
      if (tasksDelegates.Any()) {
        tasksTsBuilder.AppendLine();
        foreach (var del in tasksDelegates) {
          // Generate TS code for delegate defined in Tasks.
          var delTs = TsTypeGenerator.GenerateTsForDelegate(del, customTypes, out var _);
          tasksTsBuilder.AppendLine(delTs);
        }
      }

      // Ensure the output directory exists and write Tasks.ts.
      Directory.CreateDirectory(OutputDir);
      var tasksTsPath = Path.Combine(OutputDir, "Tasks.ts");
      File.WriteAllText(tasksTsPath, tasksTsBuilder.ToString());
      Log.LogMessage(MessageImportance.High, $"Generated TypeScript entry point at {tasksTsPath}");

      // Recursively generate TS files for all required custom types.
      var generatedTypes = new HashSet<string>(StringComparer.Ordinal);
      foreach (var typeName in requiredTypes) {
        GenerateCustomTypeTs(typeName, customTypes, customTypeNames, OutputDir, generatedTypes);
      }

      // Process global delegates - generate a separate TS file for each.
      foreach (var kvp in globalDelegates) {
        var del        = kvp.Value;
        var tsCode     = TsTypeGenerator.GenerateTsForDelegate(del, customTypes, out var _);
        var outputPath = Path.Combine(OutputDir, del.Identifier.Text + ".ts");
        File.WriteAllText(outputPath, tsCode);
        Log.LogMessage(
          MessageImportance.High,
          $"Generated TypeScript file for delegate {del.Identifier.Text} at {outputPath}"
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
    Dictionary<string, TypeDeclarationSyntax> customTypes,
    HashSet<string> customTypeNames,
    string outputDir,
    HashSet<string> generatedTypes
  ) {
    if (generatedTypes.Contains(typeName)) {
      return; // Already generated.
    }

    if (!customTypes.TryGetValue(typeName, out var typeDecl)) {
      Log.LogWarning(
        $"Referenced type '{
          typeName
        }' was not marked with the export attribute '{
          ExportAttributeName
        }'."
      );
      return;
    }

    // Use TsTypeGenerator to convert the type.
    var tsCode = TsTypeGenerator.GenerateTs(
      typeDecl,
      customTypes,
      out var dependencies
    );
    // Prepend import statements for any dependencies.
    var importBuilder = new StringBuilder();
    foreach (var dep in dependencies) {
      // Only import if the dependency is also a custom type.
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
