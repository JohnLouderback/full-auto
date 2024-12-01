using Downscaler.Core.Contracts.Models.AppState;

namespace Downscaler.Core.Contracts.Services;

/// <summary>
///   Responsible for validating and parsing YAML files.
/// </summary>
public interface IYamlParser {
  /// <summary>
  ///   Parses the given YAML file and returns whether the file was parsed successfully. If
  ///   successful, the <see cref="IAppState" /> instance for the
  ///   application will be updated with the parsed values.
  /// </summary>
  /// <param name="path"> The path to the YAML file to parse. </param>
  /// <returns> <c>true</c> if the YAML file was parsed successfully; otherwise, <c>false</c>. </returns>
  bool ParseYaml(string path);
}
