using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using GenericModLauncher.Models;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

[TypeScriptExport]
public class BaseGameInfo : ItemInfo, IBaseGameInfo {
  /// <inheritdoc />
  [ScriptMember("gamePath")]
  public string GamePath { get; }

  /// <inheritdoc />
  [ScriptMember("logoPath")]
  public string? LogoPath { get; }

  /// <inheritdoc />
  [ScriptMember("mods")]
  [TsTypeOverride(typeof(IEnumerable<ModInfo>))]
  public IEnumerable<IModInfo>? Mods { get; }


  public static explicit operator BaseGameInfo(ScriptObject obj) {
    if (JSTypeConverter.MatchesShape<BaseGameInfo>(obj, out var errors)) {
      return JSTypeConverter.ConvertTo<BaseGameInfo>(obj);
    }

    throw new ScriptEngineException(
      "Could not convert to BaseGameInfo due to conversion errors:\n  " +
      string.Join("\n  ", errors)
    );
  }
}
