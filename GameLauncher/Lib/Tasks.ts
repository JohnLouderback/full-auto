// This file is auto-generated. Do not modify manually.
import { Application } from "./Application";

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

}
