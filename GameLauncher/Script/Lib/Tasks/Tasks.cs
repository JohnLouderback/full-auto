using GameLauncher.Core.CodeGenAttributes;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript.V8;

namespace GameLauncher.Script;

[EntryPointExport]
public static partial class Tasks {
  private static V8ScriptEngine? engine;


  [HideFromTypeScript]
  public static void InjectIntoEngine(V8ScriptEngine engine) {
    Tasks.engine = engine;
    engine.AddHostType("__Tasks", typeof(Tasks));
  }
}
