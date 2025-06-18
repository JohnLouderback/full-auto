namespace GenericModLauncher.Models;

public interface ILauncherConfiguration {
  /// <summary>
  ///   The configuration for the game and its associated mods, which includes
  ///   the base game information and any mods that can eb applied to it.
  /// </summary>
  IBaseGameInfo Game { get; }
}
