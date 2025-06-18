using GameLauncher.Script.Utils.CodeGenAttributes;
using GenericModLauncher.Models;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

[TypeScriptExport]
public abstract class ItemInfo : ObjectBase, IItemInfo {
  /// <inheritdoc />
  [ScriptMember("displayName")]
  public string DisplayName { get; }

  /// <inheritdoc />
  [ScriptMember("id")]
  public string ID { get; }

  /// <inheritdoc />
  [ScriptMember("screenshotPath")]
  public string? ScreenshotPath { get; }

  /// <inheritdoc />
  [ScriptMember("description")]
  public string? Description { get; }

  /// <inheritdoc />
  [ScriptMember("customMetadata")]
  public object? CustomMetadata { get; set; }

  /// <inheritdoc />
  [ScriptMember("mixins")]
  [TsTypeOverride(typeof(IEnumerable<ModInfo>))]
  public IEnumerable<IModInfo>? Mixins { get; set; }
}
