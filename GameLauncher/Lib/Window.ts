import { BoundingBox } from "./BoundingBox";

/**
 * Represents a window of an application or another window.
 *
 */
/**
 * Represents a window of an application or another window.
 *
 */
// Auto-generated from C# class Window
export interface Window {
    /**
     * The handle of the window. This is a unique identifier representing the
     * window.
     *
     */
    handle: number;
    /**
     * The text of the window's titlebar. For example: `"Untitled - Notepad"`.
     * It may be used
     * to either get or set the title of the window.
     *
     */
    title: string;
    /**
     * The class name of the window. For example: `"Notepad"`. Class names are
     * generally
     * used to identify the type of window within the application. They are not
     * necessarily unique.
     *
     */
    className: string;
    getBoundingBox(): BoundingBox;
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
    readonly isClosed: boolean;
    /**
     * Resolves each time the window is shown (multiple awaits allowed).
     *
     */
    shown: Promise<void>;
    /**
     * Resolves each time the window is hidden (multiple awaits allowed).
     *
     */
    hidden: Promise<void>;
    Minimized: Promise<void>;
    Maximized: Promise<void>;
    Restored: Promise<void>;
    BoundsChanged: Promise<void>;
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
     * @throws ArgumentException Thrown when the event name is not the name of a
     * known event.
     */
    on(eventName: string, callback: WindowEventCallback): void;
}

// Auto-generated from C# delegate WindowEventCallback
export type WindowEventCallback = (window: Window) => void;

