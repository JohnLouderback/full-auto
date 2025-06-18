using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using GenericModLauncher.Models;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

[TypeScriptExport]
public class ModInfo : ItemInfo, IModInfo {
  private bool dontInheritMixins;

  /// <inheritdoc />
  [ScriptMember("requiredSourcePort")]
  [TsTypeOverride(typeof(SourcePortInfo))]
  public ISourcePortInfo? RequiredSourcePort { get; set; }

  /// <inheritdoc />
  [ScriptMember("dontInheritMixins")]
  public bool? DontInheritMixins {
    get => dontInheritMixins;
    set => dontInheritMixins = value ?? false;
  }


  public static explicit operator ModInfo(ScriptObject obj) {
    if (JSTypeConverter.MatchesShape<ModInfo>(obj, out var errors)) {
      return JSTypeConverter.ConvertTo<ModInfo>(obj);
    }

    throw new ScriptEngineException(
      "Could not convert to ModInfo due to conversion errors:\n  " +
      string.Join("\n  ", errors)
    );
  }
}
