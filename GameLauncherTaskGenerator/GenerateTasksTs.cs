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
///   An MSBuild task that generates the Tasks object in TypeScript from a C# source directory.
///   The generated file contains all public methods from the partial class "Tasks".
/// </summary>
public class GenerateTasksTs : Task {
  [Required]
  public string SourceDirectory { get; set; } // Instead of a single file, process all .cs files

  [Required] public string OutputDir { get; set; }


  public override bool Execute() {
    try {
      // Find all C# files
      var csFiles = Directory.GetFiles(SourceDirectory, "*.cs", SearchOption.AllDirectories);
      var methods = new List<MethodDeclarationSyntax>();

      foreach (var file in csFiles) {
        var code       = File.ReadAllText(file);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root       = syntaxTree.GetRoot();

        // Find all parts of the Tasks partial class
        var partialClasses = root.DescendantNodes()
          .OfType<ClassDeclarationSyntax>()
          .Where(
            cls => cls.Identifier.Text == "Tasks" &&
                   cls.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
          );

        foreach (var partialClass in partialClasses) {
          // Collect methods from each partial class
          methods.AddRange(partialClass.Members.OfType<MethodDeclarationSyntax>());
        }
      }

      if (!methods.Any()) {
        Log.LogMessage(MessageImportance.High, "No methods found in partial class Tasks.");
        return true;
      }

      var sb = new StringBuilder();
      sb.Append(
        """
        // This file is auto-generated. Do not modify manually.

        export class Tasks {

        """
      );

      foreach (var method in methods) {
        // Exclusion list:
        // Exclude private methods.
        if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword))) {
          continue;
        }

        // This method is for internal use and not needed in the generated file.
        if (method.Identifier.Text == "InjectIntoEngine") {
          continue;
        }

        var jsDoc = JsDocGenerator.GenerateJsDoc(method, "    ");
        sb.AppendLine(jsDoc);

        var isAsync    = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
        var returnType = CSharpTypeScriptConverter.Convert(method.ReturnType);

        var parameters = string.Join(
          ", ",
          method.ParameterList.Parameters.Select(
            p => $"{p.Identifier.Text}: {CSharpTypeScriptConverter.Convert(p.Type)}"
          )
        );

        sb.Append(
          $$"""
              public static {{
                (isAsync ? "async " : "")
              }}{{
                // Lowercase the first letter of the method name to better fit JavaScript conventions.
                method.Identifier.Text[0].ToString().ToLower() + method.Identifier.Text.Substring(1)
              }}({{
                parameters
              }}): {{
                returnType
              }} {
                // @ts-expect-error - This function is injected into the engine dynamically.
                return 
          """
        );
        sb.AppendLine($" __Tasks.{method.Identifier.Text}(");
        sb.AppendLine(
          $"          {
            string.Join(", ", method.ParameterList.Parameters.Select(p => p.Identifier.Text))
          }"
        );
        sb.AppendLine("      )\n    }");
        sb.AppendLine();
      }

      sb.AppendLine("}");

      // Write output
      var outputPath = Path.Combine(OutputDir, "Tasks.ts");
      Directory.CreateDirectory(OutputDir);
      File.WriteAllText(outputPath, sb.ToString());

      Log.LogMessage(MessageImportance.High, $"Generated TypeScript file at {outputPath}");
      return true;
    }
    catch (Exception ex) {
      Log.LogError($"Error generating TypeScript file: {ex.Message}");
      return false;
    }
  }
}
