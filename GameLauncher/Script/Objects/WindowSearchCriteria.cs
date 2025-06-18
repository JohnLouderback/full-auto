using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents the criteria used to search for a window. Generally, at least one of the properties
///   should be set to search for a window. If none are set, the search will match any window.
/// </summary>
[TypeScriptExport]
[IsInputType]
public class WindowSearchCriteria : ObjectBase {
  [ScriptMember("title")] public string? Title { get; set; }

  [ScriptMember("className")] public string? ClassName { get; set; }


  public static implicit operator WindowSearchCriteria(ScriptObject obj) {
    if (JSTypeConverter.MatchesShape<WindowSearchCriteria>(obj, out var errors)) {
      return JSTypeConverter.ConvertTo<WindowSearchCriteria>(obj);
    }

    throw new ScriptEngineException(
      "Could not convert to WindowSearchCriteria due to conversion errors:\n  " +
      string.Join("\n  ", errors)
    );
  }
}
