using GameLauncher.Script.Utils.CodeGenAttributes;
using GenericModLauncher.Models;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   A callback that is called when the game is launched. It provides information about the base
///   games, the mod (if a mod was chosen), and any mixins that were selected. This callback should
///   handle the logic for launching the game with the selected mod and mixins.
/// </summary>
public delegate Task OnLaunchCallback(
  BaseGameInfo baseGame,
  ModInfo? mod,
  List<ModInfo>? mixins
);

[TypeScriptExport]
public abstract class ItemInfo : ObjectBase, IItemInfo {
  private IItemInfo.OnLaunchCallback? onLaunch;

  /// <inheritdoc />
  [ScriptMember("displayName")]
  public string DisplayName { get; set; }

  /// <inheritdoc />
  [ScriptMember("id")]
  public string ID { get; set; }

  /// <inheritdoc />
  [ScriptMember("screenshotPath")]
  public string? ScreenshotPath { get; set; }

  /// <inheritdoc />
  [ScriptMember("description")]
  public string? Description { get; set; }

  /// <inheritdoc />
  [ScriptMember("releaseYear")]
  public string? ReleaseYear { get; set; }

  /// <inheritdoc />
  [ScriptMember("customMetadata")]
  public object? CustomMetadata { get; set; }

  /// <inheritdoc />
  [ScriptMember("mixins")]
  [TsTypeOverride(typeof(List<ModInfo>))]
  public IEnumerable<IModInfo>? Mixins { get; set; }

  /// <inheritdoc cref="IItemInfo.OnLaunch" />
  [ScriptMember("onLaunch")]
  public new OnLaunchCallback? OnLaunch { get; set; }

  /// <inheritdoc />
  [HideFromTypeScript]
  IItemInfo.OnLaunchCallback? IItemInfo.OnLaunch {
    get => onLaunch ??=
             OnLaunch is not null
               ? (baseGame, mod, mixins) => OnLaunch.Invoke(
                 baseGame as BaseGameInfo ??
                 throw new InvalidCastException(
                   $"{nameof(baseGame)} is not a {nameof(BaseGameInfo)} instance."
                 ),
                 mod is not null
                   ? mod as ModInfo ??
                     throw new InvalidCastException(
                       $"{nameof(mod)} is not a {nameof(ModInfo)} instance."
                     )
                   : null,
                 mixins is not null
                   ? mixins as List<ModInfo> ??
                     throw new InvalidCastException(
                       $"{nameof(mixins)} is not a {nameof(List<ModInfo>)} instance."
                     )
                   : null
               )
               : null;
    set {}
  }
}
