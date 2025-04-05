using Core.Utils;
using GameLauncher.Script.Utils;
using Monitor = GameLauncher.Script.Objects.Monitor;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   Retrieves a list of all monitors on the system.
  /// </summary>
  /// <returns> An array of all monitors on the system. </returns>
  public static JSArray<Monitor> GetAllMonitors() {
    return JSArray<Monitor>.FromIEnumerable(
      NativeUtils.EnumerateMonitors().Select(monitor => new Monitor(monitor)).ToList()
    );
  }
}
