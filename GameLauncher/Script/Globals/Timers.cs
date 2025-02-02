using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace GameLauncher.Script.Globals;

internal class Timers {
  private static readonly Dictionary<int, Timer> timeoutMap  = new();
  private static readonly Dictionary<int, Timer> intervalMap = new();
  private static          int                    timeoutIdCounter;
  private static          int                    intervalIdCounter;


  public static void ClearInterval(int id) {
    if (intervalMap.TryGetValue(id, out var timer)) {
      timer.Dispose();
      intervalMap.Remove(id);
    }
  }


  public static void ClearTimeout(int id) {
    if (timeoutMap.TryGetValue(id, out var timer)) {
      timer.Dispose();
      timeoutMap.Remove(id);
    }
  }


  public static void InjectIntoEngine(V8ScriptEngine engine) {
    engine.Script.__setTimeout    = new Func<ScriptObject, int, int>(SetTimeout);
    engine.Script.__clearTimeout  = new Action<int>(ClearTimeout);
    engine.Script.__setInterval   = new Func<ScriptObject, int, int>(SetInterval);
    engine.Script.__clearInterval = new Action<int>(ClearInterval);

    engine.Execute(
      """
      // noinspection JSUnresolvedReference,JSUnusedLocalSymbols

      function setTimeout(func, delay) {
          let args = Array.prototype.slice.call(arguments, 2);
          return __setTimeout(func.bind(undefined, ...args), delay || 0);
      }

      function clearTimeout(id) {
          __clearTimeout(id);
      }

      function setInterval(func, interval) {
          let args = Array.prototype.slice.call(arguments, 2);
          return __setInterval(func.bind(undefined, ...args), interval || 0);
      }

      function clearInterval(id) {
          __clearInterval(id);
      }
      """
    );
  }


  public static int SetInterval(ScriptObject func, int interval) {
    var id    = Interlocked.Increment(ref intervalIdCounter);
    var timer = new Timer(_ => func.Invoke(false), null, interval, interval);
    intervalMap[id] = timer;
    return id;
  }


  public static int SetTimeout(ScriptObject func, int delay) {
    var id = Interlocked.Increment(ref timeoutIdCounter);
    var timer = new Timer(
      _ => {
        func.Invoke(false);
        timeoutMap.Remove(id);
      },
      null,
      delay,
      Timeout.Infinite
    );
    timeoutMap[id] = timer;
    return id;
  }
}
