import { DisplayMode } from "./DisplayMode";
import { ChangeDisplayModeResult } from "./ChangeDisplayModeResult";

/**
 * Represents the graphical display of a monitor. For example, the screen
 * resolution, color depth,
 * refresh rate, and other display settings that determined in software.
 *
 */
// Auto-generated from C# class Screen
export interface Screen {
    /**
     * The current display mode of the monitor. The display mode includes the
     * screen resolution,
     * color depth, and refresh rate.
     *
     */
    readonly currentDisplayMode: DisplayMode;
    /**
     * Gets the available display modes for the monitor. The display modes
     * include available
     * combinations of screen resolutions, color depths, and refresh rates
     * supported by the monitor.
     *
     * @returns The available display modes for the monitor.
     */
    listDisplayModes(): Array<DisplayMode>;
    /**
     * Sets the display mode of the monitor to the specified display mode. The
     * display mode includes
     * the screen resolution, color depth, and refresh rate. The display mode
     * can be set temporarily
     * or permanently. Temporary display modes are not saved to the registry and
     * are reset when the
     * script finishes executing. Permanent display modes are saved to the
     * registry and persist
     * after the script finishes executing and across system restarts.
     *
     * @param displayMode The display mode to set the monitor to.
     * @param [shouldPersist=false
     * ] Whether the display mode should remain after the script finishes
     * executing.
     * @returns The result of changing the display mode. If you need to revert
     * the display mode back to the
     * original display mode, you can call the {@link
     * ChangeDisplayModeResult.Undo} method on
     * the result.
     */
    setDisplayMode(displayMode: DisplayMode, shouldPersist?: boolean): ChangeDisplayModeResult;
    /**
     * Sets the display mode of the monitor to the specified display mode. The
     * display mode includes
     * the screen resolution, color depth, and refresh rate. The display mode
     * can be set temporarily
     * or permanently. Temporary display modes are not saved to the registry and
     * are reset when the
     * script finishes executing. Permanent display modes are saved to the
     * registry and persist
     * after the script finishes executing and across system restarts.
     *
     * @param displayMode The display mode to set the monitor to. You must
     * provide a width and height for the display
     * mode. The color depth and refresh rate are optional and will default to
     * the current display
     * mode if not provided. The display mode can also be set to interlaced if
     * desired and the
     * display supports it. Finally, you can specify whether the display mode
     * should persist after
     * the script finishes executing.
     * @returns The result of changing the display mode. If you need to revert
     * the display mode back to the
     * original display mode, you can call the {@link
     * ChangeDisplayModeResult.Undo} method on
     * the result.
     * @throws ArgumentException Thrown if the display mode does not have a
     * width and/or height.
     */
    setDisplayMode(displayMode: { width: number, height: number, /** Whether the display mode should persist after the script finishes executing. */ shouldPersist?: boolean } & Partial<DisplayMode>): ChangeDisplayModeResult;
    /**
     * Sets the display mode of the monitor to the specified display mode. The
     * display mode includes
     * the screen resolution, color depth, and refresh rate. The display mode
     * can be set temporarily
     * or permanently. Temporary display modes are not saved to the registry and
     * are reset when the
     * script finishes executing. Permanent display modes are saved to the
     * registry and persist
     * after the script finishes executing and across system restarts.
     *
     * The values passed to this method may not be arbitrary. The display mode
     * must be supported by
     * the monitor. If the display mode is not supported, the system will not
     * apply the change and
     * an exception will be thrown. You can query the available display modes
     * using the
     * {@link ListDisplayModes} method.
     *
     * @param width The width of the display mode in device pixels.
     * @param height The height of the display mode in device pixels.
     * @param [refreshRate=null] The refresh rate of the display mode in Hz.
     * @param [colorDepth=null] The color depth of the display mode in bits per
     * pixel.
     * @param [shouldPersist=false
     * ] Whether the display mode should remain after the script finishes
     * executing.
     * @returns The result of changing the display mode. If you need to revert
     * the display mode back to the
     * original display mode, you can call the {@link
     * ChangeDisplayModeResult.Undo} method on
     * the result.
     */
    setDisplayMode(width: number, height: number, refreshRate?: number | null, colorDepth?: number | null, shouldPersist?: boolean): ChangeDisplayModeResult;
}
