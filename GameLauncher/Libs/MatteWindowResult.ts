import { UndoableResult } from "./UndoableResult";
import { Window } from "./Window";

/**
 * Represents the result of creating a matte window around a specified window.
 * Calling the {@link UndoableResult.undo} method will reverse the matte
 * effect by closing the matte window, allowing the matted window to function
 * normally again.
 *
 */
// Auto-generated from C# type MatteWindowResult
export interface MatteWindowResult extends UndoableResult {
    /**
     * The "matte" window that was created to surround the matted window. This
     * window is used to create a backdrop effect around the matted window,
     * effectively "blacking out" the rest of the screen around it.
     *
     */
    readonly matteWindow: Window;
    /**
     * The window that was matted. This is the original window that was modified
     * to create the matte effect.
     *
     */
    readonly mattedWindow: Window;
}
