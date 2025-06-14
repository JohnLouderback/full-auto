using GameLauncher.Core.CodeGenAttributes;
using GameLauncher.Script.Utils;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents an X,Y coordinate in 2D space. The coordinate is the  X and Y position. The origin
///   is context-specific.
/// </summary>
[TypeScriptExport]
public class Coordinate : ObjectBase {
  [ScriptMember("x")] public int X { get; set; }

  [ScriptMember("y")] public int Y { get; set; }


  public static explicit operator Coordinate(ScriptObject obj) {
    if (JSTypeConverter.MatchesShape<Coordinate>(obj, out var errors)) {
      return JSTypeConverter.ConvertTo<Coordinate>(obj);
    }

    throw new ScriptEngineException(
      "Could not convert to Coordinate due to conversion errors:\n  " +
      string.Join("\n  ", errors)
    );
  }


  [ScriptMember("toString")]
  public override string ToString() {
    return $"{{ \"x\": {X}, \"y\": {Y} }}";
  }
}
