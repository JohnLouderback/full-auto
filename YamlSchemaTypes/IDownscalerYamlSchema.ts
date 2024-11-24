interface IDebugSchema {
    /**
     * If true, debug information will be displayed in the downscaler window.
     * @default false
     */
    enabled?: boolean;

    /**
     * The font scaling factor to use for the debug information. A value of `1` means
     * the pixel font has a 1:1 ratio with the screen pixels. A value of `2`
     * means the pixel font has a 2:1 ratio with the screen pixels.
     *
     * By default, the font size is chosen dynamically based on the size of
     * the downscaler window.
     * @type integer
     */
    'font-scale'?: number;

    /**
     * Whether to show the frames per second (FPS) in the debug information.
     * The FPS represents the FPS of the downscaler window, not the source
     * window.
     */
    'show-fps'?: boolean;

    /**
     * Whether to show the mouse coordinates in the debug information. This
     * is useful for debugging that the mouse coordinates are being correctly
     * transformed from the downscaler window to the source window.
     */
    'show-mouse-coordinates'?: boolean;
}

export interface IDownscalerYamlSchema {
    /**
     * The X position of the top-left corner of the downscaler window. This will be relative
     * to the monitor that the window is on. If this is not specified, the window will be
     * positioned automatically.
     */
    x?: number;

    /**
     * The Y position of the top-left corner of the downscaler window. This will be relative\
     * to the monitor that the window is on. If this is not specified, the window will be
     * positioned automatically.
     */
    y?: number;

    /**
     * The window title to use to search for the window to downscale. This is exclusive with
     * "process-name".
     */
    'window-title'?: string;

    /**
     * The name of a running process to search for to use as the window to downscale. This is
     * exclusive with "window-title". Process names are case-insensitive and should always end
     * with the ".exe" extension.
     */
    'process-name'?: string;

    /**
     * Class name of the window to mirror. You can use a tool like "Spy++", "Window Detective",
     * or similar to find the class name of a window. Class names are case-sensitive. This is
     * useful for finding windows that have the same title, but different class names, particularly
     * child windows of a parent window.
     */
    'class-name'?: string;

    /**
     * The factor to downscale the window by. For example, a factor of 2 will downscale the window
     * by 2x, meaning that a window of size 1920x1080 will be downscaled to 960x540. To upscale the
     * window, use a factor less than 1. For example, a factor of 0.5 will upscale the window by 2x,
     * meaning that a window of size 1920x1080 will be upscaled to 3840x2160.
     */
    'downscale-factor'?: number;

    /**
     * The width to scale the window to. This is exclusive with "downscale-factor". If this is
     * specified, but height is not, the height will be scaled proportionally to maintain the
     * aspect ratio of the window.
     */
    'scale-width'?: number;

    /**
     * The height to scale the window to. This is exclusive with "downscale-factor". If this is
     * specified, but width is not, the width will be scaled proportionally to maintain the
     * aspect ratio of the window.
     */
    'scale-height'?: number;

    /**
     * A namespace where debug configurations can be specified.
     */
    debug?: IDebugSchema;
}