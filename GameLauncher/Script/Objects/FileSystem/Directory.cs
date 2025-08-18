using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a directory in the file system. A directory is a container that can hold files
///   and other directories. It can be used to organize files in a hierarchical structure.
/// </summary>
[TypeScriptExport]
public class Directory : DirectoryEntry {
  private IEnumerable<DirectoryEntry>? _allEntries;

  /// <summary>
  ///   Retrieves all entries (files and directories) within this directory.
  /// </summary>
  /// <exception cref="DirectoryNotFoundException">
  ///   Thrown when the directory does not exist or cannot be accessed.
  /// </exception>
  [ScriptMember("entries")]
  public JSArray<DirectoryEntry> entries {
    get {
      if (_allEntries is not null) {
        return JSArray<DirectoryEntry>.FromIEnumerable(_allEntries);
      }

      // If _allEntries is null, we need to load the entries from the file system.
      if (System.IO.Directory.Exists(Path)) {
        // Get all files and directories in the current directory.
        var files       = System.IO.Directory.GetFiles(Path);
        var directories = System.IO.Directory.GetDirectories(Path);

        // Create DirectoryEntry objects for each file and directory.
        var entries = files.Select(file => new File(file))
          .Concat(directories.Select(dir => new Directory(dir)).Cast<DirectoryEntry>())
          .ToList();

        // Store the entries in _allEntries for future access.
        _allEntries = entries;

        // Return the entries as a JSArray.
        return JSArray<DirectoryEntry>.FromIEnumerable(entries);
      }

      throw new DirectoryNotFoundException();
    }
  }

  /// <summary>
  ///   Retrieves a list of all subdirectories within this directory.
  /// </summary>
  [ScriptMember("subDirs")]
  public JSArray<Directory> SubDirs =>
    JSArray<Directory>.FromIEnumerable(entries.Where(e => e is Directory).Cast<Directory>());

  /// <summary>
  ///   Retrieves a list of all files within this directory.
  /// </summary>
  [ScriptMember("files")]
  public JSArray<File> Files =>
    JSArray<File>.FromIEnumerable(entries.Where(e => e is File).Cast<File>());


  /// <inheritdoc />
  public Directory(string path) : base(path) {}
}
