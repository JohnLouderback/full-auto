using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a directory entry in the file system. A directory entry either is a file or a folder
///   within a directory.
/// </summary>
[TypeScriptExport]
public class DirectoryEntry : ObjectBase {
  public string Path { get; private set; }


  internal DirectoryEntry(string path) {
    // Normalize the path to ensure it is in a consistent format.
    Path = System.IO.Path.GetFullPath(path);
  }


  /// <summary>
  ///   Checks if this directory entry is a directory.
  /// </summary>
  /// <returns>
  ///   Returns <see langword="true" /> if this entry is a directory; otherwise, <see langword="false" />
  ///   .
  /// </returns>
  [TsReturnTypeOverride("this is Directory", typeof(Directory))]
  [ScriptMember("isDirectory")]
  public virtual bool IsDirectory() {
    return this is Directory;
  }


  /// <summary>
  ///   Checks if this directory entry is a file.
  /// </summary>
  /// <returns>
  ///   Returns <see langword="true" /> if this entry is a file; otherwise, <see langword="false" />.
  /// </returns>
  [TsReturnTypeOverride("this is File", typeof(File))]
  [ScriptMember("isFile")]
  public virtual bool IsFile() {
    return this is File;
  }
}
