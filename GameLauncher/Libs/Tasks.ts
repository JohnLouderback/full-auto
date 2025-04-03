// This file is auto-generated. Do not modify manually.

import { Application } from "./Application";
import { Monitor } from "./Monitor";
import { Window } from "./Window";
import { WindowSearchCriteria } from "./WindowSearchCriteria";

/**
 * Launch the application at the specified path.
 *
 * @param path The path to the application.
 * @returns An {@link Application} object representing the application if it was
 * launched
 * successfully; otherwise, `null`. The {@link Application.ExitSignal}
 * property can be used to await the application's exit.
 * @throws ArgumentException Thrown when {@link path} is `null` or empty.
 */
export function launch(path: string): Application | null {
    // @ts-expect-error - This function is injected into the engine dynamically.
    return __Tasks.Launch(path);
}

/**
 * Synthesizes keystrokes from a SendKeys-style string.
 * For example:
 * SendKeys("^a"); will send Ctrl + A.
 * SendKeys("{Enter}"); will send Enter.
 * SendKeys("{Del 4}"); will send Del 4 times.
 * SendKeys("Hello World!"); will send Hello World!.
 *
 * @param keys
 */
export function sendKeys(keys: string): void {
    // @ts-expect-error - This function is injected into the engine dynamically.
    return __Tasks.SendKeys(keys);
}


export function changeResolution(width: number, height: number, refreshRate: number): void {
    // @ts-expect-error - This function is injected into the engine dynamically.
    return __Tasks.ChangeResolution(width, height, refreshRate);
}

/**
 * Retrieves a list of all monitors on the system.
 *
 * @returns An array of all monitors on the system.
 */
export function getAllMonitors(): Array<Monitor> {
    // @ts-expect-error - This function is injected into the engine dynamically.
    return __Tasks.GetAllMonitors();
}

/**
 * Waits for a window to be spawned with the specified criteria. This only
 * awaits new windows and
 * will not return a window that already exists at the time of calling.
 *
 * @param searchCriteria The criteria to use to search for the window.
 * @param [timeout=0
 * ] The maximum time to wait for the window to be created. If `0`, the method
 * waits
 * indefinitely.
 * @returns The window that was created, or `null` if the timeout elapsed.
 */
export function awaitWindow(searchCriteria: WindowSearchCriteria, timeout?: number): Promise<Window | null>;
/**
 * Waits for a window to be spawned with the specified criteria. This only
 * awaits new windows and
 * will not return a window that already exists at the time of calling.
 *
 * @param searchCriteria A function that takes a {@link Window} and returns
 * `true` if the
 * window matches the criteria.
 * @param [timeout=0
 * ] The maximum time to wait for the window to be created. If `0`, the method
 * waits
 * indefinitely.
 * @returns The window that was created, or `null` if the timeout elapsed.
 */
export function awaitWindow(searchCriteria: WindowCriteriaCallback, timeout?: number): Promise<Window | null>;
export async function awaitWindow(...args: [searchCriteria: WindowSearchCriteria | WindowCriteriaCallback, timeout?: number | undefined]): Promise<Window | null> {
    // @ts-expect-error - This function is injected into the engine dynamically.
    return __Tasks.AwaitWindow(...args);
}


// Auto-generated from C# delegate WindowCriteriaCallback
export type WindowCriteriaCallback = (window: Window) => boolean;

