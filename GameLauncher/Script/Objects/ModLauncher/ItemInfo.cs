using GameLauncher.Script.Utils.CodeGenAttributes;
using GenericModLauncher.Models;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

[TypeScriptExport]
public abstract class ItemInfo : ObjectBase, IItemInfo {
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
}
