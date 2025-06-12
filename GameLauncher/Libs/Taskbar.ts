import { AutoHideTaskbarResult } from "./AutoHideTaskbarResult";
import { Window } from "./Window";

/**
 * Represents the Windows taskbar, which is the bar typically located at the
 * bottom of the screen that contains the Start button, open application
 * icons, and system tray.
 *
 */
// Auto-generated from C# type Taskbar
export interface Taskbar extends Window {
    /**
     * Disables auto-hide for the taskbar. When disabled, the taskbar will remain
     * visible at all times, even when not in use.
     *
     * @param [shouldPersist=false] If `true`, the change will persist after the
     * script execution ends. Otherwise, the change will only apply during the
     * script execution and will be reversed when the script finishes.
     */
    disableAutoHide(shouldPersist?: boolean): AutoHideTaskbarResult;
    /**
     * Enables auto-hide for the taskbar. When enabled, the taskbar will
     * automatically hide when not in use, and will reappear when the mouse is
     * moved to the edge of the screen where the taskbar is located.
     *
     * @param [shouldPersist=false] If `true`, the change will persist after the
     * script execution ends. Otherwise, the change will only apply during the
     * script execution and will be reversed when the script finishes.
     */
    enableAutoHide(shouldPersist?: boolean): AutoHideTaskbarResult;
    /**
     * Checks if the taskbar is currently set to auto-hide. If auto-hide is
     * enabled,
     *
     * @returns `true` if auto-hide is enabled; otherwise, `false`.
     */
    isAutoHideEnabled(): boolean;
    /**
     * Checks if the taskbar is currently set to auto-hide. If auto-hide is
     * enabled, this method will toggle the setting to disable it, and vice versa.
     *
     * @param [shouldPersist=false] If `true`, the change will persist after the
     * script execution ends. Otherwise, the change will only apply during the
     * script execution and will be reversed when the script finishes.
     */
    toggleAutoHide(shouldPersist?: boolean): AutoHideTaskbarResult;
}
