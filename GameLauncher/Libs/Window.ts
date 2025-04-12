import { BoundingBox } from "./BoundingBox";

/**
 * Represents a window of an application or another window.
 *
 */
// Auto-generated from C# type Window
export interface Window {
    /**
     * The handle of the window. This is a unique identifier representing the
     * window.
     *
     */
    readonly handle: number;
    /**
     * The text of the window's titlebar. For example: `"Untitled - Notepad"`. It
     * may be used to either get or set the title of the window.
     *
     */
    title: string;
    /**
     * The class name of the window. For example: `"Notepad"`. Class names are
     * generally used to identify the type of window within the application. They
     * are not necessarily unique.
     *
     */
    readonly className: string;
    readonly isClosed: boolean;
    readonly isMinimized: boolean;
    readonly isMaximized: boolean;
    readonly isShowing: boolean;
    readonly isFocused: boolean;
    /**
     * Resolves the next time the window is shown.
     *
     */
    readonly shown: Promise<void>;
    /**
     * Resolves the next time the window is hidden.
     *
     */
    readonly hidden: Promise<void>;
    /**
     * Resolves the next time the window is minimized.
     *
     */
    readonly minimized: Promise<void>;
    /**
     * Resolves the next time the window is maximized.
     *
     */
    readonly maximized: Promise<void>;
    /**
     * Resolves the next time the window is restored. When the window is
     * "un-minimized."
     *
     */
    readonly restored: Promise<void>;
    readonly focused: Promise<void>;
    /**
     * Resolves the next time the window's bounds change.
     *
     */
    readonly boundsChanged: Promise<void>;
    /**
     * Resolves once when the window is closed.
     *
     */
    readonly closed: Promise<void>;
    /**
     * Binds a callback to an event.
     *
     * @param eventName The name of the event to bind the callback to.
     * @param callback The callback to execute when the event occurs.
     */
    on(eventName: "shown" | "hidden" | "minimized" | "maximized" | "restored" | "focused" | "boundsChanged" | "closed", callback: WindowEventCallback): void;
    /**
     * Requests that the window be closed. This sends a close message to the
     * window, which may or may not result in the window being closed. The window
     * may choose to ignore the request.
     *
     */
    close(): void;
    /**
     * Focuses the window. This brings the window to the front and makes it the
     * active window.
     *
     */
    focus(): void;
    /**
     * Maximizes the window. This expands the window to fill the entire screen.
     *
     */
    maximize(): void;
    /**
     * Minimizes the window. This reduces the window to an icon on the taskbar or
     * otherwise hides it from view.
     *
     */
    minimize(): void;
    /**
     * Restores the window. This restores the window to its previous size and
     * position after being minimized or maximized.
     *
     */
    restore(): void;
    /**
     * Gets the bounding box of the window. This is the rectangle that contains
     * the window's position and size on the screen. The pixels are in screen
     * coordinates, with the origin in the top-left corner of the screen.
     *
     */
    getBoundingBox(): BoundingBox;
    /**
     * Makes the window fullscreen. It does not automatically change the window's
     * style to borderless. This is done by the {@link Window.makeBorderless}
     * method.
     *
     * @param [method=resize] The method to use to make the window fullscreen.
     * Valid values are the following: `"resize"` - Sets the window's width and
     * height to cover the entire screen. This will set the window to cover the
     * entire screen of the monitor the window currently resides on. The window
     * will be resized to the monitor's current resolution. This is the default
     * method. To remove the window's borders, use the {@link
     * Window.makeBorderless} method first, before calling this method. `"alt
     * enter"` - Sends the `Alt + Enter` key combination to the window. This is
     * the same as pressing `Alt + Enter` on the keyboard. This method may not
     * work if the window does not support it.
     */
    makeFullscreen(method?: "resize" | "alt enter"): void;
    /**
     * Sets the position and size of the window.
     *
     * @param boundingBox The bounding box to set the window to.
     * @returns The same window this method was called on, for chaining.
     */
    setBoundingBox(boundingBox: BoundingBox): Window;
    /**
     * Sets the position and size of the window.
     *
     * @param x The x-coordinate of the window.
     * @param y The y-coordinate of the window.
     * @param width The width of the window.
     * @param height The height of the window.
     * @returns The same window this method was called on, for chaining.
     */
    setBoundingBox(x: number, y: number, width: number, height: number): Window;
    /**
     * Sets the window to be borderless. This removes the window's title bar and
     * borders, making it appears as purely rectangular rendering surface.
     *
     */
    makeBorderless(): void;
}
