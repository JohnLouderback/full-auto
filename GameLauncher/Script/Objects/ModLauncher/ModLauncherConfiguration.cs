using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using GenericModLauncher.Models;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

[TypeScriptExport]
public class ModLauncherConfiguration : ObjectBase, ILauncherConfiguration {
  /// <inheritdoc />
  [ScriptMember("game")]
  [TsTypeOverride(typeof(BaseGameInfo))]
  public IBaseGameInfo Game { get; set; }

  /// <inheritdoc />
  [ScriptMember("backgroundImagePath")]
  public string? BackgroundImagePath { get; set; }


  public static explicit operator ModLauncherConfiguration(ScriptObject obj) {
    if (JSTypeConverter.MatchesShape<ModLauncherConfiguration>(obj, out var errors)) {
      return JSTypeConverter.ConvertTo<ModLauncherConfiguration>(obj);
    }

    throw new ScriptEngineException(
      "Could not convert to ModLauncherConfiguration due to conversion errors:\n  " +
      string.Join("\n  ", errors)
    );
  }
}
