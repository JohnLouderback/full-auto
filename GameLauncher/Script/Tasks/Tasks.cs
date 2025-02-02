using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace GameLauncher.Script;

public static partial class Tasks {
  private static V8ScriptEngine? engine;


  public static void InjectIntoEngine(V8ScriptEngine engine) {
    Tasks.engine                 = engine;
    engine.Script.__Tasks_Launch = new Func<string, Task>(Launch);
    engine.Execute(
      new DocumentInfo("Tasks"),
      """
        // noinspection JSUnresolvedReference,JSUnusedLocalSymbols
        
        const tryInvoke = async (func, ...args) => {
          try {
            const returnValue = func(...args);
            if (returnValue instanceof Promise) {
              await returnValue;
            }
          } catch (error) {
            throw error;
          }
        };
      
        const Tasks = {
          launch: async path => tryInvoke(__Tasks_Launch, path)
        };
        
      """
    );
  }
}
