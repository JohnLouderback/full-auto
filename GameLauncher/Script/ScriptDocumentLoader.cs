using GameLauncher.TypeScript;
using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;

namespace GameLauncher.Script;

public class ScriptDocumentLoader : DocumentLoader {
  public override async Task<Document> LoadDocumentAsync(
    DocumentSettings settings,
    DocumentInfo? sourceInfo,
    string specifier,
    DocumentCategory category,
    DocumentContextCallback contextCallback
  ) {
    if (specifier.StartsWith("@library/")) {
      // Get the path for the library file.
      var libraryPath = Path.Combine(AppContext.BaseDirectory, "Libs", specifier[9..]);

      // First, we'll check if there's a compiled JS version of the library.
      var jsLibraryPath = Path.ChangeExtension(libraryPath, ".js");
      if (!File.Exists(jsLibraryPath)) {
        Console.WriteLine($"Compiling {libraryPath}...");
        // If there isn't, we'll compile the TypeScript version.
        var compiler      = new Compiler();
        var tsLibraryPath = Path.ChangeExtension(libraryPath, ".ts");
        var tsContent     = await File.ReadAllTextAsync(tsLibraryPath).ConfigureAwait(false);
        var jsContent     = compiler.Compile(tsLibraryPath);
        // Store the compiled JS version for future use.
        await File.WriteAllTextAsync(jsLibraryPath, jsContent).ConfigureAwait(false);
      }

      if (File.Exists(jsLibraryPath)) {
        var content = await File.ReadAllTextAsync(jsLibraryPath).ConfigureAwait(false);
        return new StringDocument(
          new DocumentInfo(new Uri(jsLibraryPath)) {
            Category = ModuleCategory.Standard
          },
          content
        );
      }
    }

    // If all else fails, use the default loader.
    return await Default
             .LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback)
             .ConfigureAwait(false);
  }
}
