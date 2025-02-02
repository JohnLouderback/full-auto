using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
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
        const tryInvoke = (func, ...args): any => {
            try {
                return func(...args);
            } catch (error) {
                throw error;
            }
        };

        export const Tasks = {

        """
      );

      foreach (var method in methods) {
        // Exclusion list:
        if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword))) {
          continue;
        }

        if (method.Identifier.Text == "InjectIntoEngine") {
          continue;
        }

        var jsDoc = GenerateJsDoc(method);
        sb.AppendLine(jsDoc);

        var isAsync    = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
        var returnType = CSharpTypeScriptConverter.Convert(method.ReturnType);

        var parameters = string.Join(
          ", ",
          method.ParameterList.Parameters.Select(
            p => $"{p.Identifier.Text}: {CSharpTypeScriptConverter.Convert(p.Type)}"
          )
        );

        sb.AppendLine(
          $"    {
            method.Identifier.Text
          }: {
            (isAsync ? "async " : "")
          }({
            parameters
          }): {
            returnType
          } => tryInvoke("
        );
        sb.AppendLine("        // @ts-ignore");
        sb.AppendLine($"        __Tasks_{method.Identifier.Text},");
        sb.AppendLine(
          $"        {
            string.Join(", ", method.ParameterList.Parameters.Select(p => p.Identifier.Text))
          }"
        );
        sb.AppendLine("    ),");
        sb.AppendLine();
      }

      sb.AppendLine("};");

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


  private string GenerateJsDoc(MethodDeclarationSyntax method) {
    var xmlDoc = method.GetLeadingTrivia()
      .Select(t => t.GetStructure())
      .OfType<DocumentationCommentTriviaSyntax>()
      .FirstOrDefault();

    if (xmlDoc == null) {
      return "";
    }

    // 🔹 Extract raw XML doc string and clean it
    var rawXml = xmlDoc.ToFullString();

    // 🔹 Remove leading "///" from each line
    var cleanedXml = string.Join(
      "\n",
      rawXml.Split('\n')
        .Select(
          line => line.TrimStart().StartsWith("///")
                    ? line.TrimStart().Substring(3).TrimStart()
                    : line
        )
        .Where(line => !string.IsNullOrWhiteSpace(line)) // Remove empty lines
    );

    try {
      var doc     = XDocument.Parse("<root>" + cleanedXml + "</root>");
      var summary = doc.Descendants("summary").FirstOrDefault()?.Value.Trim() ?? "";
      var paramDocs = doc.Descendants("param")
        .Select(p => $"* @param {p.Attribute("name")?.Value} {p.Value.Trim()}");
      var returnDoc = doc.Descendants("returns").FirstOrDefault()?.Value.Trim();
      var returns   = !string.IsNullOrWhiteSpace(returnDoc) ? $"\n     * @returns {returnDoc}" : "";
      var throws = doc.Descendants("exception")
        .Select(e => $"\n     * @throws {e.Attribute("cref")?.Value} {e.Value.Trim()}");

      return $"""
        
            /**
             * {
               summary
             }
             {
               string.Join("\n", paramDocs)
             }{
               returns
             }{
               string.Join("", throws)
             }
             */
        """;
    }
    catch {
      return "";
    }
  }


  private string GetTypeScriptReturnType(TypeSyntax returnType, bool isAsync) {
    var typeName = returnType.ToString();

    if (isAsync) {
      // If method is async, check for Task<T>
      if (typeName.StartsWith("Task<") &&
          typeName.EndsWith(">")) {
        var innerType = typeName.Substring(5, typeName.Length - 6);
        return $"Promise<{CSharpTypeScriptConverter.Convert(innerType)}>";
      }

      return "Promise<void>";
    }

    // Regular non-async return types
    return CSharpTypeScriptConverter.Convert(typeName);
  }
}
