using System.Diagnostics;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   Launch the application at the specified path.
  /// </summary>
  /// <param name="path"> The path to the application. </param>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="path" /> is <see langword="null" /> or empty.
  /// </exception>
  public static async Task Launch(string path) {
    if (string.IsNullOrWhiteSpace(path)) {
      throw new ArgumentException("Path cannot be null or empty.", nameof(path));
    }

    var process = new Process {
      StartInfo = new ProcessStartInfo {
        FileName        = path,
        UseShellExecute = false
      }
    };

    if (process.Start()) {
      await process.WaitForExitAsync();
    }
  }
}
