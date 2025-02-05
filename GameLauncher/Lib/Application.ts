import { Process } from "./Process";
import { Window } from "./Window";

/**
 * Abstractly represents an executable application that is or was running on the
 * system.
 *
 */
// Auto-generated from C# class Application
export interface Application {
    /**
     * An awaitable signal that will resolve when the application's process
     * exits.
     *
     */
    readonly exitSignal: Promise<void>;
    /**
     * Represents the process that is running the application.
     *
     */
    readonly process: Process;
    /**
     * Lists the windows of the application. It does not list the windows of
     * child processes or child
     * windows.
     *
     * @returns A list of windows.
     */
    listWindows(): Array<Window>;
}
