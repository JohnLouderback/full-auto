using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace GameLauncher.Utils;

internal static class ExceptionExtensions {
  private static readonly Func<Exception, StackTrace, Exception> _SetStackTrace =
    new Func<Func<Exception, StackTrace, Exception>>(
      () => {
        var target = Expression.Parameter(typeof(Exception));
        var stack  = Expression.Parameter(typeof(StackTrace));
        var traceFormatType =
          typeof(StackTrace).GetNestedType("TraceFormat", BindingFlags.NonPublic);
        var toString = typeof(StackTrace).GetMethod(
          "ToString",
          BindingFlags.NonPublic | BindingFlags.Instance,
          null,
          new[] { traceFormatType },
          null
        );
        var normalTraceFormat = Enum.GetValues(traceFormatType).GetValue(0);
        var stackTraceString = Expression.Call(
          stack,
          toString,
          Expression.Constant(normalTraceFormat, traceFormatType)
        );
        var stackTraceStringField = typeof(Exception).GetField(
          "_stackTraceString",
          BindingFlags.NonPublic | BindingFlags.Instance
        );
        var assign = Expression.Assign(
          Expression.Field(target, stackTraceStringField),
          stackTraceString
        );
        return Expression.Lambda<Func<Exception, StackTrace, Exception>>(
            Expression.Block(assign, target),
            target,
            stack
          )
          .Compile();
      }
    )();

  private static readonly Func<Exception, string, Exception> _SetStackTraceString =
    new Func<Func<Exception, string, Exception>>(
      () => {
        var target     = Expression.Parameter(typeof(Exception));
        var stackTrace = Expression.Parameter(typeof(string));

        // The string value to assign to the _stackTraceString field. It's the stack trace string
        // passed to this method.
        var stackTraceString = stackTrace;
        var stackTraceStringField = typeof(Exception).GetField(
          "_stackTraceString",
          BindingFlags.NonPublic | BindingFlags.Instance
        );
        var assign = Expression.Assign(
          Expression.Field(target, stackTraceStringField),
          stackTraceString
        );
        return Expression.Lambda<Func<Exception, string, Exception>>(
            Expression.Block(assign, target),
            target,
            stackTrace
          )
          .Compile();
      }
    )();


  public static Exception SetStackTrace(this Exception target, StackTrace stack) {
    return _SetStackTrace(target, stack);
  }


  public static Exception SetStackTrace(this Exception target, string stackTrace) {
    return _SetStackTraceString(target, stackTrace);
  }
}
