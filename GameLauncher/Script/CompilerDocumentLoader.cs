using Microsoft.ClearScript;
using Microsoft.ClearScript.JavaScript;
using CSDocumentLoader = Microsoft.ClearScript.DocumentLoader;

namespace GameLauncher.Script;

public class CompilerDocumentLoader : CSDocumentLoader {
  public override async Task<Document> LoadDocumentAsync(
    DocumentSettings settings,
    DocumentInfo? sourceInfo,
    string specifier,
    DocumentCategory category,
    DocumentContextCallback contextCallback
  ) {
    var path = "";
    if (sourceInfo is { Uri: not null } info) {
      var uri = info.Uri;
      if (uri.IsFile &&
          File.Exists(uri.LocalPath)) {
        path = uri.LocalPath;
      }
    }
    else if (specifier is not "") {
      path = Path.Combine(AppContext.BaseDirectory, specifier);
    }

    // If we were provided a path, attempt to load the file.
    if (path is not "") {
      var content = await File.ReadAllTextAsync(path).ConfigureAwait(false);

      if (Path.GetExtension(path) == ".json") {
        content = $"module.exports = {content};";
      }

      return new StringDocument(
        new DocumentInfo(new Uri(path)) {
          Category = ModuleCategory.CommonJS
        },
        content
      );
    }

    // If all else fails, use the default loader.
    return await Default
             .LoadDocumentAsync(settings, sourceInfo, specifier, category, contextCallback)
             .ConfigureAwait(false);
  }
}
