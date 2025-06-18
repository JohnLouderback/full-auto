namespace GenericModLauncher.Models;

/// <summary>
///   Represents information about a mod that can be used with a game.
/// </summary>
public interface IModInfo : IItemInfo {
  /// <summary>
  ///   If specified, the source port required to run this mod. This is used to ensure that
  ///   the mod is compatible with the game engine it is intended for. If this is null, the mod
  ///   can presumably run on any source port that the game supports or is constrained to a certain
  ///   other mod.
  /// </summary>
  ISourcePortInfo? RequiredSourcePort { get; }

  /// <summary>
  ///   Indicates if this mod should not inherit mixins from the base game or other mods. This is
  ///   useful if you want to establish that this mod is a standalone mod that should not
  ///   inherit any additional functionality or changes from other mods except those explicitly
  ///   specified in its own mixins. If omitted, the mod will inherit mixins from the
  ///   base game and any other mods that are applied to it.
  /// </summary>
  bool? DontInheritMixins { get; }
}
