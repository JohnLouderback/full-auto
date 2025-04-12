/**
 * Represents a result that can be reversed. This is typically used in tasks
 * that make changes to the system that we need to explicitly reverse after
 * the script has completed. For example, a task that changes the screen
 * resolution might need to restore the original resolution after the script
 * has finished executing.
 *
 */
// Auto-generated from C# type UndoableResult
export interface UndoableResult {
    /**
     * Represents whether the result has already been reversed or not.
     *
     */
    readonly isReversed: boolean;
    /**
     * Immediately reverses the result if it has not already been reversed. This
     * method allows explicit control over when the result should be reversed.
     * Calling this method will prevent the result from being reversed
     * automatically when the script completes. If the result has already been
     * reversed, this method has no effect.
     *
     */
    undo(): Promise<void>;
}
