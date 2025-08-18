import { DownscaleDebugOptions } from "./DownscaleDebugOptions";

// Auto-generated from C# type DownscaleOptions
export interface DownscaleOptions {
    /**
     * The X position of the top-left corner of the downscaler window. This will
     * be relative to the monitor that the window is on. If this is not specified,
     * the window will be positioned automatically.
     *
     */
    x?: number | null;
    /**
     * The Y position of the top-left corner of the downscaler window. This will
     * be relative to the monitor that the window is on. If this is not specified,
     * the window will be positioned automatically.
     *
     */
    y?: number | null;
    /**
     * The factor to downscale the window by. For example, a factor of 2 will
     * downscale the window by 2x, meaning that a window of size 1920x1080 will be
     * downscaled to 960x540. To upscale the window, use a factor less than 1. For
     * example, a factor of 0.5 will upscale the window by 2x, meaning that a
     * window of size 1920x1080 will be upscaled to 3840x2160.
     *
     */
    downscaleFactor?: number | null;
    /**
     * The width to scale the window to. This is exclusive with {@link
     * DownscaleOptions.downscaleFactor}. If this is specified, but height is not,
     * the height will be scaled proportionally to maintain the aspect ratio of
     * the window.
     *
     */
    scaleWidth?: number | null;
    /**
     * The height to scale the window to. This is exclusive with {@link
     * DownscaleOptions.downscaleFactor}. If this is specified, but width is not,
     * the width will be scaled proportionally to maintain the aspect ratio of the
     * window.
     *
     */
    scaleHeight?: number | null;
    /**
     * A namespace where debug configurations can be specified.
     *
     */
    debug?: DownscaleDebugOptions | null;
}
