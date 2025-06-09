using System.Diagnostics;
using GameLauncher.Script.Objects;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;
using Application = GameLauncher.Script.Objects.Application;
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
    return _Launch(path, args: null, options: null);
  }


  /// <summary>
  ///   Launch the application at the specified path.
  /// </summary>
  /// <param name="path"> The path to the application. </param>
  /// <param name="options">
  ///   <see cref="LaunchOptions" />. Optional. The options to use when launching the application.
  /// </param>
  /// <returns>
  ///   An <see cref="Application" /> object representing the application if it was launched
  ///   successfully; otherwise, <see langword="null" />. The <see cref="Application.ExitSignal" />
  ///   property can be used to await the application's exit.
  /// </returns>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="path" /> is <see langword="null" /> or empty.
  /// </exception>
  public static Application? Launch(string path, LaunchOptions options) {
    return _Launch(path, args: null, options);
  }


  /// <summary>
  ///   Launch the application at the specified path.
  /// </summary>
  /// <param name="path"> The path to the application. </param>
  /// <param name="args">
  ///   Optional. The arguments to pass to the application. If <see langword="null" />, no
  ///   arguments are passed.
  /// </param>
  /// <param name="options">
  ///   <see cref="LaunchOptions" />. Optional. The options to use when launching the application.
  /// </param>
  /// <returns>
  ///   An <see cref="Application" /> object representing the application if it was launched
  ///   successfully; otherwise, <see langword="null" />. The <see cref="Application.ExitSignal" />
  ///   property can be used to await the application's exit.
  /// </returns>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="path" /> is <see langword="null" /> or empty.
  /// </exception>
  public static Application? Launch(string path, IList<string> args, LaunchOptions options) {
    return _Launch(path, args, options);
  }


  /// <summary>
  ///   Launch the application at the specified path.
  /// </summary>
  /// <param name="path"> The path to the application. </param>
  /// <param name="args">
  ///   Optional. The arguments to pass to the application. If <see langword="null" />, no
  ///   arguments are passed.
  /// </param>
  /// <returns>
  ///   An <see cref="Application" /> object representing the application if it was launched
  ///   successfully; otherwise, <see langword="null" />. The <see cref="Application.ExitSignal" />
  ///   property can be used to await the application's exit.
  /// </returns>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="path" /> is <see langword="null" /> or empty.
  /// </exception>
  public static Application? Launch(string path, IList<string> args) {
    return _Launch(path, args, options: null);
  }


  [HideFromTypeScript]
  public static Application? Launch(string path, ScriptObject options) {
    if (options is null) {
      return Launch(path);
    }

    return Launch(path, (LaunchOptions)options);
  }


  [HideFromTypeScript]
  public static Application? Launch(string path, IList<string> args, ScriptObject options) {
    if (options is null) {
      return Launch(path, args);
    }

    return _Launch(path, args, (LaunchOptions)options);
  }


  /// <summary>
  ///   Launch the application at the specified path.
  /// </summary>
  /// <param name="path"> The path to the application. </param>
  /// <param name="args">
  ///   Optional. The arguments to pass to the application. If <see langword="null" />, no
  ///   arguments are passed.
  /// </param>
  /// <param name="options">
  ///   <see cref="LaunchOptions" />. Optional. The options to use when launching the application.
  /// </param>
  /// <returns>
  ///   An <see cref="Application" /> object representing the application if it was launched
  ///   successfully; otherwise, <see langword="null" />. The <see cref="Application.ExitSignal" />
  ///   property can be used to await the application's exit.
  /// </returns>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="path" /> is <see langword="null" /> or empty.
  /// </exception>
  private static Application? _Launch(string path, IList<string>? args, LaunchOptions? options) {
    if (string.IsNullOrWhiteSpace(path)) {
      throw new ArgumentException("Path cannot be null or empty.", nameof(path));
    }

    var process = new Process {
      StartInfo = new ProcessStartInfo {
        FileName               = path,
        Arguments              = string.Join(separator: ' ', args ?? []),
        UseShellExecute        = false,
        RedirectStandardOutput = options?.RedirectStdOut ?? false,
        RedirectStandardError  = options?.RedirectStdErr ?? false
      }
    };

    if (process.Start()) {
      if (options?.RedirectStdOut ?? false) {
        process.OutputDataReceived += (_, e) => {
          if (e.Data is not null) {
            Console.WriteLine($"{e.Data}");
          }
        };
        process.BeginOutputReadLine();
      }

      if (options?.RedirectStdErr ?? false) {
        process.ErrorDataReceived += (_, e) => {
          if (e.Data is not null) {
            Console.WriteLine($"{e.Data}");
          }
        };
        process.BeginErrorReadLine();
      }

      return new Application {
        ExitSignal = process.WaitForExitAsync(),
        Process    = new Objects.Process(process)
      };
    }

    return null;
  }
}
