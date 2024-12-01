namespace Downscaler.Core.Contracts.Services;

/// <summary>
///   Service for parsing command line arguments according to the application's needs.
/// </summary>
public interface IArgsParser {
  /// <summary>
  ///   Parses the given command line arguments and updates the application state accordingly.
  /// </summary>
  /// <param name="args"> The command line arguments that are passed to this process. </param>
  /// <returns> Whether or not the arguments were parsed successfully. </returns>
  bool ParseArgs(string[] args);
}
