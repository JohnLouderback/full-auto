namespace GenericModLauncher.Models;

public interface IItemInfo {
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
}
