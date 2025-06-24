// Auto-generated from C# type DownscaleDebugOptions
export interface DownscaleDebugOptions {
    /**
     * If `true`, debug information will be displayed in the downscaler window.
     *
     */
    enabled?: boolean | null;
    /**
     * The font scaling factor to use for the debug information. A value of `1`
     * means the pixel font has a 1:1 ratio with the screen pixels. A value of `2`
     * means the pixel font has a 2:1 ratio with the screen pixels.
     *
     */
    fontScale?: number | null;
    /**
     * Whether to show the frames per second (FPS) in the debug information. The
     * FPS represents the FPS of the downscaler window, not the source window.
     *
     */
    showFPS?: boolean | null;
    /**
     * Whether to show the mouse coordinates in the debug information. This is
     * useful for debugging that the mouse coordinates are being correctly
     * transformed from the downscaler window to the source window.
     *
     */
    showMouseCoordinates?: boolean | null;
    /**
     * The font family to use for the debug information. The font family can be
     * one of the following: `extra-small`: Useful for small windows like 240p.
     * (font: Pixel Rocks, 3px tall) `small`: Useful for small windows like 240p.
     * (font: Pixel Millennium, 5px tall) `normal`: Useful for most windows.
     * (font: Dogica Pixel, 7px tall) `large`: Useful for larger windows like
     * 1024x768 and up. (font: Fixedsys, 9px tall)
     *
     */
    fontFamily?: "extra-small" | "small" | "normal" | "large" | null | undefined;
}
