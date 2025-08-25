using GameLauncher.Script.Utils;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using File = GameLauncher.Script.Objects.File;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   Searches for files matching the specified glob pattern in the given directory (and
  ///   potentially its subdirectories). This allows for finding files based on patterns
  ///   and wildcards.
  /// </summary>
  /// <param name="searchDir"> The directory to search in. </param>
  /// <param name="globPattern"> The glob pattern to match files against. </param>
  /// <returns>
  ///   An array containing the files that match the glob pattern. If no files match, an empty array
  ///   is returned.
  /// </returns>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="searchDir" /> or <paramref name="globPattern" /> is
  ///   <see langword="null" /> or empty.
  /// </exception>
  public static Task<JSArray<File>> Glob(
    string searchDir,
    string globPattern
  ) {
    return Glob(searchDir, globPattern, excludePattern: null);
  }


  /// <summary>
  ///   Searches for files matching the specified glob pattern in the given directory (and
  ///   potentially its subdirectories).
  /// </summary>
  /// <param name="searchDir"> The directory to search in. </param>
  /// <param name="globPattern"> The glob pattern to match files against. </param>
  /// <param name="excludePattern">
  ///   Optional. A glob pattern to exclude files from the results. If a matched file also matches this
  ///   glob pattern, it is omitted from the results.
  /// </param>
  /// <returns>
  ///   An array containing the files that match the glob pattern. If no files match, an empty array
  ///   is returned.
  /// </returns>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="searchDir" /> or <paramref name="globPattern" /> is
  ///   <see langword="null" /> or empty.
  /// </exception>
  public static Task<JSArray<File>> Glob(
    string searchDir,
    string globPattern,
    string? excludePattern = null
  ) {
    if (string.IsNullOrEmpty(searchDir)) {
      throw new ArgumentException(
        "Search directory cannot be null or empty.",
        nameof(globPattern)
      );
    }

    if (string.IsNullOrEmpty(globPattern)) {
      throw new ArgumentException("Glob pattern cannot be null or empty.", nameof(globPattern));
    }

    return Task.Run(() => {
        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        matcher.AddInclude(globPattern);
        if (excludePattern != null) matcher.AddExclude(excludePattern);

        var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(searchDir)));
        return JSArray<File>.FromIEnumerable(
          result.Files.Select(file => new File(Path.Combine(searchDir, file.Path)))
        );
      }
    );
  }
}
