using GameLauncher.Script.Utils;
using GameLauncher.Utils;
using Microsoft.ClearScript.V8;

namespace GameLauncher.Script.Globals;

/// <summary>
///   Injects a handler into the V8 engine that will log unhandled fire-and-forget promise
///   rejections.
/// </summary>
public static class PromiseRejectionHandler {
  public static void InjectIntoEngine(V8ScriptEngine engine) {
    engine.AddHostType("__ErrorUtils", typeof(ErrorUtils));
    engine.AddHostType("__Logger", typeof(Logger));
    engine.Execute(
      """
      // noinspection JSUnresolvedReference,JSUnusedLocalSymbols

      Promise.prototype._then = Promise.prototype.then;
      Promise.prototype.then = function then(onFulfilled, onRejected) {
          return this._then(
              onFulfilled,
              function(reason) {
                  if (onRejected) {
                      return onRejected(reason);
                  } else if (reason instanceof Error) {
                      __Logger.exception(__ErrorUtils.cleanStackTrace(reason.stack));
                      throw reason;
                  } else {
                      throw reason;
                  }
              }
          );
      };
      """
    );
  }
}
