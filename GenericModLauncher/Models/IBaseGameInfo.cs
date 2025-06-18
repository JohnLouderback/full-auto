namespace GenericModLauncher.Models;

public interface IBaseGameInfo : IItemInfo {
  /// <summary>
  ///   The path to the base game executable.
  /// </summary>
  string GamePath { get; }

  /// <summary>
  ///   The path to the logo image for the game, used in the UI to visually represent
  ///   the game.
  /// </summary>
  string? LogoPath { get; }

  /// <summary>
  ///   A collection of pre-configured mods that are associated with this game.
  /// </summary>
  IEnumerable<IModInfo>? Mods { get; }
}
