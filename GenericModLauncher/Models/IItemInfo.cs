namespace GenericModLauncher.Models;

public interface IItemInfo {
  /// <summary>
  ///   A callback that is called when the game is launched. It provides information about the base
  ///   games, the mod (if a mod was chosen), and any mixins that were selected. This callback should
  ///   handle the logic for launching the game with the selected mod and mixins.
  /// </summary>
  public delegate Task OnLaunchCallback(
    IBaseGameInfo baseGame,
    IModInfo? mod,
    IEnumerable<IModInfo> mixins
  );

  /// <summary>
  ///   The display name of the item—the game or mod, used in the UI to identify the game.
  /// </summary>
  string DisplayName { get; }

  /// <summary>
  ///   The unique identifier for the game, used to distinguish it from other items such as other
  ///   mods.
  /// </summary>
  string ID { get; }

  /// <summary>
  ///   The path to the screenshot image for the item, used in the UI to visually represent
  ///   the game or mod.
  /// </summary>
  string? ScreenshotPath { get; }

  /// <summary>
  ///   A brief description of the game or mod, providing additional context or information
  ///   about it.
  /// </summary>
  string? Description { get; }

  /// <summary>
  ///   The four digit year when the game or mod was released, used to provide context
  ///   for the item in the UI.
  /// </summary>
  string? ReleaseYear { get; }

  /// <summary>
  ///   Any custom metadata associated with the item, which can be used to store additional
  ///   information
  /// </summary>
  object? CustomMetadata { get; set; }

  /// <summary>
  ///   A collection of mods that are specifically associated with this item, such as
  ///   other mods that can be applied to this game or only specifically to this mod. These can be
  ///   "mixed in" to the item to create a new configuration or gameplay experience.
  /// </summary>
  IEnumerable<IModInfo>? Mixins { get; set; }

  /// <summary>
  ///   A callback that is called when the game or a mod is launched. It provides information about
  ///   the base game, the mod (if a mod was chosen), and any mixins that were selected. This callback
  ///   is used to perform any necessary actions or configurations before the game is started.
  /// </summary>
  OnLaunchCallback? OnLaunch { get; set; }
}
