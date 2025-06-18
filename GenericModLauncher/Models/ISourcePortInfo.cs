namespace GenericModLauncher.Models;

/// <summary>
///   Represents a source port of a game engine, which is a modified version of the original
///   game engine. Certain mods may only work with specific source ports, and this interface
///   provides a way to identify and manage those source ports.
/// </summary>
public interface ISourcePortInfo {
  /// <summary>
  ///   The display name of the source port, used in the UI to identify the source port.
  /// </summary>
  string DisplayName { get; }

  /// <summary>
  ///   The unique identifier for the source port, used to distinguish it from other
  ///   source ports.
  /// </summary>
  string ID { get; }

  /// <summary>
  ///   The path to the source port executable, which is used to run the game with
  ///   this source port. This is typically the path to the modified game engine executable.
  /// </summary>
  string? SourcePortPath { get; }
}
