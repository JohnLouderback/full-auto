using System.Diagnostics;
using GameLauncher.Script.Objects;
using Process = System.Diagnostics.Process;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   Launch the application at the specified path.
  /// </summary>
  /// <param name="path"> The path to the application. </param>
  /// <returns>
  ///   An <see cref="Application" /> object representing the application if it was launched
  ///   successfully; otherwise, <see langword="null" />. The <see cref="Application.ExitSignal" />
  ///   property can be used to await the application's exit.
  /// </returns>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="path" /> is <see langword="null" /> or empty.
  /// </exception>
  public static Application? Launch(string path) {
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
      return new Application {
        ExitSignal = process.WaitForExitAsync(),
        Process    = new Objects.Process(process)
      };
    }

    return null;
  }
}
