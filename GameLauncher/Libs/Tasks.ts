import { Application } from "./Application";
import { LaunchOptions } from "./LaunchOptions";
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
export function awaitWindow(searchCriteria: WindowSearchCriteria, timeout?: number): Promise<Window>;
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
export function awaitWindow(searchCriteria: WindowCriteriaCallback, timeout?: number): Promise<Window>;
export function awaitWindow(...args: any[]): any {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.AwaitWindow(...args);
}

export function changeResolution(width: number, height: number, refreshRate: number): void {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.ChangeResolution(width, height, refreshRate);
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
     * Gets the taskbar object, which represents the Windows taskbar.
     *
     * @returns A {@link Taskbar} object representing the taskbar.
     */
export function getTaskbar(): Taskbar {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.GetTaskbar();
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
export function launch(path: string): Application;
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
export function launch(path: string, options: LaunchOptions): Application;
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
export function launch(path: string, args: Array<string>, options: LaunchOptions): Application;
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
export function launch(path: string, args: Array<string>): Application;
export function launch(...args: any[]): any {
    // @ts-expect-error - Function is injected by the engine
    return __Tasks.Launch(...args);
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
 * A function whose purpose is to determine whether a given window matches the
 * criteria. If the window matches the criteria, the function should return
 * `true`; otherwise, `false`.
 *
 */
// Auto-generated from delegate WindowCriteriaCallback
export type WindowCriteriaCallback = (window: Window, process: Process) => boolean;
