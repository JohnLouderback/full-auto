// This file is auto-generated. Do not modify manually.
import { Application } from "./Application";
import { Window } from "./Window";

export class Tasks {
    /**
     * Launch the application at the specified path.
     *
     * @param path The path to the application.
     * @returns An {@link Application} object representing the application if it
     * was launched
     * successfully; otherwise, `null`. The {@link Application.ExitSignal}
     * property can be used to await the application's exit.
     * @throws ArgumentException Thrown when {@link path} is `null` or empty.
     */
    public static launch(path: string): Application | null {
        // @ts-expect-error - This function is injected into the engine dynamically.
        return __Tasks.Launch(path);
    }

    /**
     * Waits for a window to be spawned with the specified title. This only
     * awaits new windows and
     * will not return a window that already exists at the time of calling.
     *
     * @param title The title of the window to wait for.
     * @param [processID=0] The process ID of the window to wait for. If `0`,
     * the window is allowed to be from
     * any process.
     * @param [timeout=0] The maximum time to wait for the window to be created.
     * If `0`, the method waits
     * indefinitely.
     * @returns The window that was created, or `null` if the timeout elapsed.
     */
    public static async awaitWindow(title: string, processID: number = 0, timeout: number = 0): Promise<Window | null> {
        // @ts-expect-error - This function is injected into the engine dynamically.
        return __Tasks.AwaitWindow(title, processID, timeout);
    }

}
