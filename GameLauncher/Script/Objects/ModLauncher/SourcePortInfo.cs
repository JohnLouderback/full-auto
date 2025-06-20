using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using GenericModLauncher.Models;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

[TypeScriptExport]
public class SourcePortInfo : ObjectBase, ISourcePortInfo {
  /// <inheritdoc />
  [ScriptMember("displayName")]
  public string DisplayName { get; set; }

  /// <inheritdoc />
  [ScriptMember("id")]
  public string ID { get; set; }

  /// <inheritdoc />
  [ScriptMember("sourcePortPath")]
  public string? SourcePortPath { get; set; }


  public static explicit operator SourcePortInfo(ScriptObject obj) {
    if (JSTypeConverter.MatchesShape<SourcePortInfo>(obj, out var errors)) {
      return JSTypeConverter.ConvertTo<SourcePortInfo>(obj);
    }

    throw new ScriptEngineException(
      "Could not convert to SourcePortInfo due to conversion errors:\n  " +
      string.Join("\n  ", errors)
    );
  }
}
