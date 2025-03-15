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
    /**
     * Lists the windows of the application. It lists all windows belonging to
     * the application,
     * including child windows. If {@link includeChildProcesses} is `true`,
     * then it will also list windows of child processes.
     *
     * @param [includeChildProcesses=false] Whether to additionally include
     * windows of child processes.
     * @returns A list of windows.
     */
    listWindowsDeep(includeChildProcesses?: boolean): Array<Window>;
}
