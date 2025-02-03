import { Process } from "./Process";

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
     * adsfasdf
     *
     */
    readonly process: Process;
}
