using GameLauncher.Script.Utils;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents the criteria used to search for a window. Generally, at least one of the properties
///   should be set to search for a window. If none are set, the search will match any window.
/// </summary>
public class WindowSearchCriteria {
  [ScriptMember("title")] public string? Title { get; set; }

  [ScriptMember("className")] public string? ClassName { get; set; }


  public static implicit operator WindowSearchCriteria(ScriptObject obj) {
    if (!obj.IsPlainObject()) {
      throw new ScriptEngineException("Expected a plain object.");
    }

    var criteria = new WindowSearchCriteria();

    if (obj.HasProperty("title")) {
      criteria.Title = obj.GetProperty<string>("title");
    }

    if (obj.HasProperty("className")) {
      criteria.ClassName = obj.GetProperty<string>("className");
    }

    return criteria;
  }
}
