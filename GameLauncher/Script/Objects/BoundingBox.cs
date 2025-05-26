using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a bounding box in 2D space. The bounding box is its X and Y position, as well as its
///   width and height. The X and Y position represent the top-left corner of the bounding box relative
///   to the origin of the space. The origin is context-specific.
/// </summary>
[TypeScriptExport]
public class BoundingBox : ObjectBase {
  [ScriptMember("x")] public int X { get; set; }

  [ScriptMember("y")] public int Y { get; set; }

  [ScriptMember("width")] public int Width { get; set; }

  [ScriptMember("height")] public int Height { get; set; }


  public static explicit operator BoundingBox(ScriptObject obj) {
    if (JSTypeConverter.MatchesShape<BoundingBox>(obj, out var errors)) {
      return JSTypeConverter.ConvertTo<BoundingBox>(obj);
    }

    throw new ScriptEngineException(
      "Could not convert to BoundingBox due to conversion errors:\n  " +
      string.Join("\n  ", errors)
    );
  }


  [ScriptMember("toString")]
  public override string ToString() {
    return $"{{ \"x\": {X}, \"y\": {Y}, \"width\": {Width}, \"height\": {Height} }}";
  }
}
