using GameLauncher.Script.Utils.CodeGenAttributes;
using GameLauncherTaskGenerator;
using Microsoft.ClearScript;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a result that can be reversed. This is typically used in tasks that make changes
///   to the system that we need to explicitly reverse after the script has completed. For example,
///   a task that changes the screen resolution might need to restore the original resolution
///   after the script has finished executing.
/// </summary>
[TypeScriptExport]
public abstract class UndoableResult : ObjectBase {
  /// <summary>
  ///   Represents whether the result has already been reversed or not.
  /// </summary>
  [ScriptMember("isReversed")]
  public bool IsReversed { get; private set; }

  /// <summary>
  ///   <para>
  ///     Indicates whether the result should be undone when the script completes. This property
  ///     is used to control whether the result should be reversed automatically or if it should
  ///     be left in place. By default, it is set to true, meaning the result will be undone
  ///     when the script completes. If set to false, the result will not be undone automatically.
  ///     Tasks where the end-user opts to keep the result in place can set this property to false.
  ///   </para>
  ///   <para>
  ///     The end-user may still explicitly call the <see cref="UndoableResult.Undo" /> method to
  ///     reverse the result at any time, regardless of this property. This allows for more
  ///     flexibility in controlling the lifecycle of the result.
  ///   </para>
  /// </summary>
  internal bool ShouldUndo { get; set; } = true;

  /// <summary>
  ///   A LIFO stack of undoable results. This is used to keep track of the results that need to be
  ///   reversed when the script has completed. The stack is used to ensure that the most recent
  ///   result is reversed first, which is important for tasks that make multiple changes to the
  ///   system in a specific sequence.
  /// </summary>
  private static Stack<UndoableResult> UndoStack { get; } = new();


  public UndoableResult() {
    // Push the current result onto the undo stack when it is created.
    UndoStack.Push(this);
  }


  /// <summary>
  ///   Immediately reverses the result if it has not already been reversed. This method allows
  ///   explicit control over when the result should be reversed. Calling this method will prevent
  ///   the result from being reversed automatically when the script completes. If the result has
  ///   already been reversed, this method has no effect.
  /// </summary>
  public async Task Undo() {
    if (!IsReversed) {
      await Reverse();
      IsReversed = true;
    }
  }


  /// <summary>
  ///   Reverses the result. This method should be implemented by subclasses to provide the
  ///   specific logic for reversing the result. The implementation should ensure that any changes
  ///   made by the task are undone, restoring the system to its previous state.
  /// </summary>
  /// <returns> A task that represents the asynchronous operation of reversing the result.</returns>
  protected abstract Task Reverse();


  /// <summary>
  ///   Reverses all results in the undo stack. This method is typically called when the script has
  ///   completed executing to ensure that all changes made by the tasks are undone. It iterates
  ///   over the undo stack and calls the `Reverse` method on each result, marking it as reversed
  ///   after it has been successfully reversed.
  /// </summary>
  internal static async Task ReverseAll() {
    while (UndoStack.Count > 0) {
      var result = UndoStack.Pop();
      if (!result.IsReversed &&
          result.ShouldUndo) {
        await result.Reverse();
        result.IsReversed = true;
      }
    }
  }
}
