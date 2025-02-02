using Microsoft.ClearScript.V8;

namespace GameLauncher.Script;

public static partial class Tasks {
  private static V8ScriptEngine? engine;


  public static void InjectIntoEngine(V8ScriptEngine engine) {
    Tasks.engine = engine;
    engine.AddHostType("__Tasks", typeof(Tasks));
  }
}
