using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GameLaunchTaskSourceGenerator;

[Generator]
public class TasksSourceGenerator : IIncrementalGenerator {
  public void Initialize(IncrementalGeneratorInitializationContext context) {
    // Step 1: Detect all class declarations with [GenerateTasks]
    var classDeclarations =
      context.SyntaxProvider
        .CreateSyntaxProvider(
          (s, _) => IsCandidateClass(s),
          (ctx, _) => (ClassDeclarationSyntax)ctx.Node
        )
        .Where(cls => cls is not null);

    // Step 2: Get project directory from MSBuild properties
    var configOptionsProvider =
      context.AnalyzerConfigOptionsProvider;

    // Step 3: Convert class declarations to semantic model data (INamedTypeSymbol)
    IncrementalValuesProvider<INamedTypeSymbol> classSymbols =
      context.CompilationProvider.Combine(classDeclarations.Collect())
        .SelectMany(
          (pair, _) =>
            pair.Right
              .Select(
                cls => {
                  var model = pair.Left.GetSemanticModel(cls.SyntaxTree);
                  return model.GetDeclaredSymbol(cls);
                }
              )
              .Where(symbol => symbol is not null)
        );

    // Step 4: Register output, now passing the AnalyzerConfigOptionsProvider
    context.RegisterSourceOutput(
      classSymbols.Combine(configOptionsProvider),
      (spc, tuple) => {
        var (classSymbol, configOptions) = tuple;

        // Get the project directory from AnalyzerConfigOptionsProvider
        var options = configOptions.GetOptions(classSymbol.Locations[0].SourceTree);
        var projectDir = options.TryGetValue("build_property.MSBuildProjectDirectory", out var dir)
                           ? dir
                           : Directory.GetCurrentDirectory(); // Fallback

        var tsCode = GenerateTypeScript(classSymbol);

        var outputPath = Path.Combine(projectDir, "Lib", $"{classSymbol.Name}.ts");

        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        // Write the file to disk
        File.WriteAllText(outputPath, tsCode);
      }
    );
  }


  private static string CSharpToTypeScriptType(ITypeSymbol type) {
    return type.ToString() switch {
      "System.String"                                     => "string",
      "System.Int32" or "System.Int64" or "System.Double" => "number",
      "System.Boolean"                                    => "boolean",
      "System.Threading.Tasks.Task"                       => "Promise<void>",
      _                                                   => "any"
    };
  }


  private static string GenerateJsDoc(IMethodSymbol method) {
    var xml = method.GetDocumentationCommentXml();
    if (string.IsNullOrWhiteSpace(xml)) return "";

    var doc     = XDocument.Parse("<root>" + xml + "</root>");
    var summary = doc.Descendants("summary").FirstOrDefault()?.Value.Trim() ?? "";
    var paramDocs = doc.Descendants("param")
      .Select(p => $" * @param {p.Attribute("name")?.Value} {p.Value.Trim()}");
    var returnDoc = doc.Descendants("returns").FirstOrDefault()?.Value.Trim();
    var returns   = returnDoc != null ? $" * @returns {returnDoc}" : "";

    return $@"
/**
 * {
   summary
 }
{
  string.Join("\n", paramDocs)
}
{
  returns
}
 */".Trim();
  }


  private static string GenerateTypeScript(INamedTypeSymbol classSymbol) {
    var sb = new StringBuilder();

    sb.AppendLine("// This file is auto-generated. Do not modify manually.");
    sb.AppendLine("const tryInvoke = async (func, ...args) => {");
    sb.AppendLine("    try {");
    sb.AppendLine("        const returnValue = func(...args);");
    sb.AppendLine("        if (returnValue instanceof Promise) {");
    sb.AppendLine("            await returnValue;");
    sb.AppendLine("        }");
    sb.AppendLine("    } catch (error) {");
    sb.AppendLine("        throw error;");
    sb.AppendLine("    }");
    sb.AppendLine("};");
    sb.AppendLine();
    sb.AppendLine("export const Tasks = {");

    foreach (var method in classSymbol.GetMembers().OfType<IMethodSymbol>()) {
      if (method.DeclaredAccessibility != Accessibility.Public) continue;
      if (method.MethodKind != MethodKind.Ordinary) continue;

      var jsDoc = GenerateJsDoc(method);
      sb.AppendLine(jsDoc);

      var parameters = string.Join(
        ", ",
        method.Parameters.Select(p => $"{p.Name}: {CSharpToTypeScriptType(p.Type)}")
      );
      sb.AppendLine($"    {method.Name}: async ({parameters}) => tryInvoke(");
      sb.AppendLine("        // @ts-ignore");
      sb.AppendLine($"        __Tasks_{method.Name},");
      sb.AppendLine($"        {string.Join(", ", method.Parameters.Select(p => p.Name))}");
      sb.AppendLine("    ),");
    }

    sb.AppendLine("};");
    return sb.ToString();
  }


  private static bool IsCandidateClass(SyntaxNode node) {
    return node is ClassDeclarationSyntax classDecl &&
           classDecl.AttributeLists.Any(
             al => al.Attributes
               .Any(a => a.Name.ToString() == "GenerateTasks")
           );
  }
}
