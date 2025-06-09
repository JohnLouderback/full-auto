import { BoundingBox } from "./BoundingBox";
import { ConstrainCursorResult } from "./ConstrainCursorResult";
import { DownscaleOptions } from "./DownscaleOptions";

/**
 * Represents a window of an application or another window.
 *
 */
// Auto-generated from C# type Window
export interface Window {
    /**
     * Constrains the cursor to the bounding box of the window. This means that
     * the cursor will not be able to move outside the window's bounding box until
     * the cursor is released.
     * 
     * Note: The bounding box is not updated automatically if the window is
     * resized, moved, or closed. In those case, you should call this method again
     * to update the cursor constraint or release the cursor.
     *
     * @param shouldPersist If true, the cursor will remain constrained even after
     * the script has finished executing.
     * @returns A {@link ConstrainCursorResult} that can be used to manually
     * reverse the cursor constraint. Alternatively, you may call the {@link
     * Tasks.releaseCursor} task.
     */
    constrainCursor(shouldPersist: boolean): ConstrainCursorResult;
    /**
     * Constrains the cursor to the bounding box of the window. This means that
     * the cursor will not be able to move outside the window's bounding box until
     * the cursor is released.
     * 
     * Note: The bounding box is not updated automatically if the window is
     * resized, moved, or closed. In those case, you should call this method again
     * to update the cursor constraint or release the cursor.
     *
     * @returns A {@link ConstrainCursorResult} that can be used to manually
     * reverse the cursor constraint. Alternatively, you may call the {@link
     * Tasks.releaseCursor} task.
     */
    constrainCursor(): ConstrainCursorResult;
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
    /**
     * Creates a new window that acts as a "mirror" of the current window. The new
     * window will be created using the passed configuration. This is useful for
     * creating a new window that is a scaled version of the current window. Handy
     * for when a window is resistant to being scaled down through other means,
     * such as resizing or cannot be resized beyond a certain size.
     * 
     * A compelling use-case for this is if you wanted to play a contemporary
     * pixel art game at a low resolution on a CRT TV. Imagine that the game
     * utilizes 240p pixel art, but the game is locked to a higher resolution. You
     * could use this to create a new window that is a scaled version of the
     * current window but at an actual 240p resolution.
     *
     * @param downscaleFactor The factor to downscale the window by. For example,
     * a factor of 2 will downscale the window by 2x, meaning that a window of
     * size 1920x1080 will be downscaled to 960x540. To upscale the window, use a
     * factor less than 1. For example, a factor of 0.5 will upscale the window by
     * 2x, meaning that a window of size 1920x1080 will be upscaled to 3840x2160.
     * @returns A reference to the downscaler window.
     */
    downscale(downscaleFactor: number): Promise<Window>;
    /**
     * Creates a new window that acts as a "mirror" of the current window. The new
     * window will be created using the passed configuration. This is useful for
     * creating a new window that is a scaled version of the current window. Handy
     * for when a window is resistant to being scaled down through other means,
     * such as resizing or cannot be resized beyond a certain size.
     * 
     * A compelling use-case for this is if you wanted to play a contemporary
     * pixel art game at a low resolution on a CRT TV. Imagine that the game
     * utilizes 240p pixel art, but the game is locked to a higher resolution. You
     * could use this to create a new window that is a scaled version of the
     * current window but at an actual 240p resolution.
     *
     * @param width The width to scale the window to. The width is in device
     * pixels, meaning that it will not be scaled by the DPI of the monitor.
     * @param height The height to scale the window to. The height is in device
     * pixels, meaning that it will not be scaled by the DPI of the monitor.
     * @returns A reference to the downscaler window.
     */
    downscale(width: number, height: number): Promise<Window>;
    /**
     */
    downscale(box: BoundingBox): Promise<Window>;
    /**
     * Creates a new window that acts as a "mirror" of the current window. The new
     * window will be created using the passed configuration. This is useful for
     * creating a new window that is a scaled version of the current window. Handy
     * for when a window is resistant to being scaled down through other means,
     * such as resizing or cannot be resized beyond a certain size.
     * 
     * A compelling use-case for this is if you wanted to play a contemporary
     * pixel art game at a low resolution on a CRT TV. Imagine that the game
     * utilizes 240p pixel art, but the game is locked to a higher resolution. You
     * could use this to create a new window that is a scaled version of the
     * current window but at an actual 240p resolution.
     *
     * @param options The options to use when creating the downscaler window. This
     * includes the position of the window, the downscale factor, and the width
     * and height to scale to. The downscale factor is the factor to downscale the
     * window by. For example, a factor of 2 will downscale the window by 2x,
     * meaning that a window of size 1920x1080 will be downscaled to 960x540. To
     * upscale the window, use a factor less than 1. For example, a factor of 0.5
     * will upscale the window by 2x, meaning that a window of size 1920x1080 will
     * be upscaled to 3840x2160.
     * @returns A reference to the downscaler window.
     */
    downscale(options: DownscaleOptions): Promise<Window>;
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
     * Moves the window to the specified position.
     *
     * @param x The x-coordinate of the window.
     * @param y The y-coordinate of the window.
     * @returns The same window this method was called on, for chaining.
     */
    move(x: number, y: number): Window;
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
    /**
     * Applies a matte effect to the window, embedding it within a colored
     * backdrop. This is useful for focusing attention on the window's content by
     * surrounding it with a solid color, effectively "blacking out" the rest of
     * the screen around the window.
     *
     * @param [backdropColor=#000000] The color to use for the backdrop matte.
     * This should be a hex color code (e.g., "#000000" for black). Defaults to
     * black if not specified.
     */
    matte(backdropColor?: string): Promise<void>;
}
