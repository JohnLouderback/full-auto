using System.Collections;
using Microsoft.ClearScript;

namespace GameLauncher.TypeScript;

public class CompilerHost {
  private readonly Dictionary<string, string> _inMemoryFiles    = new();
  private readonly string                     _currentDirectory = Directory.GetCurrentDirectory();


  public CompilerHost(Dictionary<string, string>? inMemoryFiles = null) {
    if (inMemoryFiles != null) {
      foreach (var file in inMemoryFiles) {
        _inMemoryFiles[ResolvePath(file.Key)] = file.Value;
      }
    }
  }


  [ScriptMember(Name = "fileExists")]
  public bool FileExists(string fileName) {
    var resolvedPath = ResolvePath(fileName);
    return _inMemoryFiles.ContainsKey(resolvedPath) || File.Exists(resolvedPath);
  }


  [ScriptMember(Name = "getCanonicalFileName")]
  public string GetCanonicalFileName(string fileName) {
    return ResolvePath(fileName);
  }


  [ScriptMember(Name = "getCurrentDirectory")]
  public string GetCurrentDirectory() {
    return _currentDirectory;
  }


  [ScriptMember(Name = "getDefaultLibFileName")]
  public string GetDefaultLibFileName(dynamic options) {
    return ResolvePath("lib.d.ts"); // Adjust as needed
  }


  [ScriptMember(Name = "getNewLine")]
  public string GetNewLine() {
    return Environment.NewLine;
  }


  [ScriptMember(Name = "getSourceFile")]
  public dynamic? GetSourceFile(string fileName, int languageVersion) {
    var content = ReadFile(fileName);
    return content != null ? new { fileName, content, languageVersion } : null;
  }


  [ScriptMember(Name = "readDirectory")]
  public string[] ReadDirectory(
    string rootDir,
    IList extensions,
    IList excludes,
    IList includes
  ) {
    return ReadDirectory(rootDir, extensions, excludes, includes, null);
  }


  [ScriptMember(Name = "readDirectory")]
  public string[] ReadDirectory(
    string rootDir,
    IList extensions,
    IList excludes,
    IList includes,
    dynamic? depth = null
  ) {
    var resolvedPath = ResolvePath(rootDir);
    if (!Directory.Exists(resolvedPath)) {
      return Array.Empty<string>();
    }

    var allFiles = Directory.GetFiles(
      resolvedPath,
      "*.*",
      depth.HasValue ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories
    );
    // Apply extension filtering
    var filteredFiles = allFiles.Where(
      file => {
        var ext = Path.GetExtension(file);
        return true; //extensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
      }
    );

    // Convert IList to string[] for LINQ operations
    var extensionsArr = extensions.Cast<string>().ToArray();
    var excludesArr   = excludes.Cast<string>().ToArray();
    var includesArr   = includes.Cast<string>().ToArray();

    // Apply inclusion patterns (simple name matching)
    if (includesArr.Any()) {
      filteredFiles = filteredFiles.Where(
        file =>
          includesArr.Any(include => file.Contains(include, StringComparison.OrdinalIgnoreCase))
      );
    }

    // Apply exclusion patterns (simple directory name matching)
    if (excludesArr.Any()) {
      filteredFiles = filteredFiles.Where(
        file =>
          !excludesArr.Any(exclude => file.Contains(exclude, StringComparison.OrdinalIgnoreCase))
      );
    }

    return filteredFiles.Select(Path.GetFullPath).ToArray();
  }


  [ScriptMember(Name = "readFile")]
  public string? ReadFile(string fileName) {
    var resolvedPath = ResolvePath(fileName);
    if (_inMemoryFiles.TryGetValue(resolvedPath, out var content)) {
      return content;
    }

    return File.Exists(resolvedPath) ? File.ReadAllText(resolvedPath) : null;
  }


  [ScriptMember(Name = "useCaseSensitiveFileNames")]
  public bool UseCaseSensitiveFileNames() {
    return !Path.DirectorySeparatorChar.Equals('\\'); // Case-sensitive except on Windows
  }


  [ScriptMember(Name = "writeFile")]
  public void WriteFile(string fileName, string content) {
    var resolvedPath = ResolvePath(fileName);
    File.WriteAllText(resolvedPath, content);
  }


  private string ResolvePath(string fileName) {
    if (Path.IsPathFullyQualified(fileName)) {
      return Path.GetFullPath(fileName);
    }

    return Path.GetFullPath(Path.Combine(_currentDirectory, fileName));
  }
}
