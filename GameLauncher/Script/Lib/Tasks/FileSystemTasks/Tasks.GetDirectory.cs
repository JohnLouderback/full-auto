using Directory = GameLauncher.Script.Objects.Directory;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   Gets a file object for the specified path. If the file does not exist, returns
  ///   <see langword="null" />.
  /// </summary>
  /// <param name="path"> The path to the file. </param>
  /// <returns>
  ///   A <see cref="Directory" /> object representing the file if it exists; otherwise,
  ///   <see langword="null" />.
  /// </returns>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="path" /> is <see langword="null" /> or empty.
  /// </exception>
  public static Directory? GetDirectory(string path) {
    if (string.IsNullOrEmpty(path)) {
      throw new ArgumentException("Path cannot be null or empty.", nameof(path));
    }

    // Check if the directory exists.
    if (!System.IO.Directory.Exists(path)) {
      return null;
    }

    return new Directory(path);
  }
}
