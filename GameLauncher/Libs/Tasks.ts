import { Application } from "./Application";
import { BoundingBox } from "./BoundingBox";
import { ConstrainCursorResult } from "./ConstrainCursorResult";
import { Coordinate } from "./Coordinate";
import { Directory } from "./Directory";
import { File } from "./File";
import { HideCursorResult } from "./HideCursorResult";
import { LaunchOptions } from "./LaunchOptions";
import { ModLauncherConfiguration } from "./ModLauncherConfiguration";
import { Monitor } from "./Monitor";
import { Process } from "./Process";
import { Taskbar } from "./Taskbar";
import { Window } from "./Window";
import { WindowSearchCriteria } from "./WindowSearchCriteria";

// Auto-generated. Do not edit manually.

    /**
     * Waits for a window to be spawned with the specified criteria. This only
     * awaits new windows and will not return a window that already exists at the
     * time of calling.
     *
     * @param searchCriteria The criteria to use to search for the window.
     * @param [timeout=0] The maximum time to wait for the window to be created.
     * If `0`, the method waits indefinitely.
     * @returns The window that was created, or `null` if the timeout elapsed.
     */
export function awaitWindow(searchCriteria: WindowSearchCriteria, timeout?: number): Promise<Window | null>;
    /**
     * Waits for a window to be spawned with the specified criteria. This only
     * awaits new windows and will not return a window that already exists at the
     * time of calling.
     *
     * @param searchCriteria A function that takes a {@link Window} and returns
     * `true` if the window matches the criteria.
     * @param [timeout=0] The maximum time to wait for the window to be created.
     * If `0`, the method waits indefinitely.
     * @returns The window that was created, or `null` if the timeout elapsed.
     */
export function awaitWindow(searchCriteria: WindowCriteriaCallback, timeout?: number): Promise<Window | null>;
export function awaitWindow(...args: any[]): any {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.AwaitWindow(...args);
}

export function changeResolution(width: number, height: number, refreshRate: number): void {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.ChangeResolution(width, height, refreshRate);
}

    /**
     * Constrains the cursor to a specified bounding box on the screen. The cursor
     * will be unable to move outside the specified bounding box until the cursor
     * is released.
     *
     * @param boundingBox The bounding box to constrain the cursor to. The
     * bounding box must have a positive width and height greater than zero.
     * @param [shouldPersist=false] Whether the cursor constraint should persist
     * after the script has finished executing.
     * @returns A {@link ConstrainCursorResult} that can be used to manually
     * reverse the cursor constraint. Alternatively, you may call the {@link
     * Tasks.releaseCursor} task.
     */
export function constrainCursor(boundingBox: BoundingBox, shouldPersist?: boolean): ConstrainCursorResult {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.ConstrainCursor(boundingBox, shouldPersist);
}

    /**
     * Searches for any windows that match the specified criteria. If no windows
     * are found, it waits for a new window to be created that matches the
     * criteria. If a window is found, it is returned immediately. If no window is
     * found within the specified timeout, an empty array is returned.
     *
     * @param searchCriteria The criteria to use to search for the window.
     * @param [timeout=0] The maximum time to wait for the window to be created,
     * if none were found initially. If `0`, the method waits indefinitely.
     * @returns An array of windows that match the criteria if any exist at the
     * time of calling. If no windows are found, it will return an array
     * containing the first newly created window that matches the criteria. If no
     * window is found within the specified timeout, an empty array is returned.
     */
export function findOrAwaitWindow(searchCriteria: WindowSearchCriteria, timeout?: number): Promise<Array<Window>>;
    /**
     * Waits for a window to be spawned with the specified criteria. This only
     * awaits new windows and will not return a window that already exists at the
     * time of calling.
     *
     * @param searchCriteria A function that takes a {@link Window} and returns
     * `true` if the window matches the criteria.
     * @param [timeout=0] The maximum time to wait for the window to be created,
     * if none were found initially. If `0`, the method waits indefinitely.
     * @returns An array of windows that match the criteria if any exist at the
     * time of calling. If no windows are found, it will return an array
     * containing the first newly created window that matches the criteria. If no
     * window is found within the specified timeout, an empty array is returned.
     */
export function findOrAwaitWindow(searchCriteria: WindowCriteriaCallback, timeout?: number): Promise<Array<Window>>;
export function findOrAwaitWindow(...args: any[]): any {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.FindOrAwaitWindow(...args);
}

    /**
     * Searches for any windows that match the specified criteria. If no windows
     * are found, it returns an empty array.
     *
     * @param searchCriteria The criteria to use to search for the windows.
     * @returns The window that was created, or `null` if the timeout elapsed.
     */
export function findWindows(searchCriteria: WindowSearchCriteria): Array<Window>;
    /**
     * Searches for any windows that match the specified criteria. If no windows
     * are found, it returns an empty array.
     *
     * @param searchCriteria A function that takes a {@link Window} and returns
     * `true` if the window matches the criteria.
     * @returns The window that was created, or `null` if the timeout elapsed.
     */
export function findWindows(searchCriteria: WindowCriteriaCallback): Array<Window>;
export function findWindows(...args: any[]): any {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.FindWindows(...args);
}

    /**
     * Retrieves a list of all monitors connected to the system.
     *
     * @returns An array of all monitors connected to the system.
     */
export function getAllMonitors(): Array<Monitor> {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.GetAllMonitors();
}

    /**
     * Gets all the windows that are currently open on the system. This includes
     * hidden ones.
     *
     */
export function getAllWindows(): Array<Window> {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.GetAllWindows();
}

    /**
     * Gets the current position of the cursor on the screen.
     *
     * @returns A {@link Coordinate} representing the current position of the
     * cursor on the screen.
     */
export function getCursorPosition(): Coordinate {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.GetCursorPosition();
}

    /**
     * Gets a file object for the specified path. If the file does not exist,
     * returns `null`.
     *
     * @param path The path to the file.
     * @returns A {@link Directory} object representing the file if it exists;
     * otherwise, `null`.
     */
export function getDirectory(path: string): Directory | null {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.GetDirectory(path);
}

    /**
     * Gets a file object for the specified path. If the file does not exist,
     * returns `null`.
     *
     * @param path The path to the file.
     * @returns A {@link File} object representing the file if it exists;
     * otherwise, `null`.
     */
export function getFile(path: string): File | null {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.GetFile(path);
}

    /**
     * Gets the taskbar object, which represents the Windows taskbar.
     *
     * @returns A {@link Taskbar} object representing the taskbar.
     */
export function getTaskbar(): Taskbar {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.GetTaskbar();
}

    /**
     * Searches for files matching the specified glob pattern in the given
     * directory (and potentially its subdirectories).
     *
     * @param searchDir The directory to search in.
     * @param globPattern The glob pattern to match files against.
     * @returns An array containing the files that match the glob pattern. If no
     * files match, an empty array is returned.
     */
export function glob(searchDir: string, globPattern: string): Array<File>;
    /**
     * Searches for files matching the specified glob pattern in the given
     * directory (and potentially its subdirectories).
     *
     * @param searchDir The directory to search in.
     * @param globPattern The glob pattern to match files against.
     * @param [excludePattern=null] Optional. A glob pattern to exclude files from
     * the results. If a matched file also matches this glob pattern, it is
     * omitted from the results.
     * @returns An array containing the files that match the glob pattern. If no
     * files match, an empty array is returned.
     */
export function glob(searchDir: string, globPattern: string, excludePattern?: string | null): Array<File>;
export function glob(...args: any[]): any {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.Glob(...args);
}

    /**
     * Makes the cursor invisible, effectively hiding it from the user.
     * 
     * This method will decrement the display count of the cursor, which means
     * that the cursor will not be visible until the display count reaches zero.
     * If the display count is already zero, the cursor will remain hidden.
     * 
     * If the {@link force} parameter is set to true, the display count will be
     * decremented forcibly, meaning that the cursor will be hidden regardless of
     * its current display count. This is useful if you want to ensure that the
     * cursor is hidden, even if other applications or scripts have made it
     * visible. However, this should be used with caution, as it may interfere
     * with the user's expectations or other applications' cursor visibility.
     *
     * @param [force=false] If true, forcibly decrements the display count until
     * the cursor is hidden.
     * @param [shouldPersist=false] If true, the cursor will remain hidden even
     * after the script has finished executing.
     */
export function hideCursor(force?: boolean, shouldPersist?: boolean): HideCursorResult {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.HideCursor(force, shouldPersist);
}

    /**
     * Launch the application at the specified path.
     *
     * @param path The path to the application.
     * @returns An {@link Application} object representing the application if it
     * was launched successfully; otherwise, `null`. The {@link
     * Application.exitSignal} property can be used to await the application's
     * exit.
     */
export function launch(path: string): Application | null;
    /**
     * Launch the application at the specified path.
     *
     * @param path The path to the application.
     * @param options {@link LaunchOptions}. Optional. The options to use when
     * launching the application.
     * @returns An {@link Application} object representing the application if it
     * was launched successfully; otherwise, `null`. The {@link
     * Application.exitSignal} property can be used to await the application's
     * exit.
     */
export function launch(path: string, options: LaunchOptions): Application | null;
    /**
     * Launch the application at the specified path.
     *
     * @param path The path to the application.
     * @param args Optional. The arguments to pass to the application. If `null`,
     * no arguments are passed.
     * @param options {@link LaunchOptions}. Optional. The options to use when
     * launching the application.
     * @returns An {@link Application} object representing the application if it
     * was launched successfully; otherwise, `null`. The {@link
     * Application.exitSignal} property can be used to await the application's
     * exit.
     */
export function launch(path: string, args: Array<string>, options: LaunchOptions): Application | null;
    /**
     * Launch the application at the specified path.
     *
     * @param path The path to the application.
     * @param args Optional. The arguments to pass to the application. If `null`,
     * no arguments are passed.
     * @returns An {@link Application} object representing the application if it
     * was launched successfully; otherwise, `null`. The {@link
     * Application.exitSignal} property can be used to await the application's
     * exit.
     */
export function launch(path: string, args: Array<string>): Application | null;
export function launch(...args: any[]): any {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.Launch(...args);
}

    /**
     * Releases the cursor from its current clipping bounds, allowing it to move
     * freely outside the previously constrained area. This works both when the
     * cursor was constrained by the {@link Tasks.constrainCursor} task or by any
     * other means (i.e., any other application or system setting that may have
     * constrained the cursor).
     *
     */
export function releaseCursor(): void {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.ReleaseCursor();
}

    /**
     * Synthesizes keystrokes from a SendKeys-style string. These are either
     * handled by the currently focused window or the system.
     * 
     * Examples of input:
     * - `SendKeys("^a");` will send Ctrl + A.
     * - `SendKeys("{F1}");` will send F1.
     * - `SendKeys("#!^a");` will send Windows + Alt + Ctrl + A.
     * - `SendKeys("abcd");` will send a, b, c, d.
     * - `SendKeys("{Enter}");` will send Enter.
     * - `SendKeys("{Del 4}");` will send Del 4 times.
     * - `SendKeys("Hello World!");` will send Hello World!.
     * 
     * Note that, while this method cannot be awaited, there is an inherent
     * asynchronicity in synthesizing keystrokes. The keystrokes are sent to the
     * system's input queue, and the synthesized keystrokes may not be processed
     * immediately. This is due to the nature of the input queue and the way the
     * operating system handles input events.
     *
     * @param keys
     */
export function sendKeys(keys: string): void {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.SendKeys(keys);
}

    /**
     * Sets the position of the cursor on the screen to the specified coordinates.
     *
     * @param x The X coordinate to set the cursor position to.
     * @param y The Y coordinate to set the cursor position to.
     */
export function setCursorPosition(x: number, y: number): void {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.SetCursorPosition(x, y);
}

    /**
     * Makes the cursor visible, allowing it to be seen by the user.
     * 
     * This method will increment the display count of the cursor, which means
     * that the cursor will be visible until the display count reaches zero. If
     * the display count is already zero, the cursor will remain visible.
     * 
     * If the {@link force} parameter is set to true, the display count will be
     * incremented forcibly, meaning that the cursor will be made visible
     * regardless of its current display count. This is useful if you want to
     * ensure that the cursor is visible, even if other applications or scripts
     * have made it invisible. However, this should be used with caution, as it
     * may interfere with the user's expectations or other applications' cursor
     * visibility.
     *
     * @param [force=false] If true, forcibly increments the display count until
     * the cursor is shown.
     */
export function showCursor(force?: boolean): void {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.ShowCursor(force);
}

    /**
     * Shows the mod launcher window. This is a tool for configuring a window to
     * show a list of game mods, allowing users to choose, enable, disable, and
     * ultimately launch the game with selected mods.
     *
     */
export async function showModLauncher(configuration: ModLauncherConfiguration): Promise<void> {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.ShowModLauncher(configuration);
}

/**
 * A function whose purpose is to determine whether a given window matches the
 * criteria. If the window matches the criteria, the function should return
 * `true`; otherwise, `false`.
 *
 */
// Auto-generated from delegate WindowCriteriaCallback
export type WindowCriteriaCallback = (window: Window, process: Process) => boolean;
