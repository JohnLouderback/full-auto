using Core.Utils;
using GameLauncher.Script.Objects;
using GameLauncher.Script.Utils;
using Microsoft.ClearScript;

namespace GameLauncher.Script;

public partial class Tasks {
  /// <summary>
  ///   Gets all the windows that are currently open on the system. This includes hidden ones.
  /// </summary>
  /// <returns></returns>
  [ScriptMember("getAllWindows")]
  public static IList<Window> GetAllWindows() {
    return JSArray<Window>.FromIEnumerable(
      NativeUtils.EnumerateWindows().Select(win32Window => new Window(win32Window))
    );
  }
}
