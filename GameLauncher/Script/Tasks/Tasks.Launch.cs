using System.Diagnostics;
using GameLaunchTaskSourceGenerator;

namespace GameLauncher.Script;

[GenerateTasks]
public static partial class Tasks {
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
