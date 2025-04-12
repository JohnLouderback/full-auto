// Auto-generated from C# type DisplayMode
export interface DisplayMode {
    /**
     * The width of the display mode in device pixels.
     *
     */
    readonly width: number;
    /**
     * The height of the display mode in device pixels.
     *
     */
    readonly height: number;
    /**
     * The number of bits used to store the color of each pixel in this display
     * mode. This includes all color information and may also include extra bits
     * such as transparency (alpha).
     * 
     * For example:
     * - `8` – 256 colors (palette-based).
     * - `16` – 65,536 colors (high color).
     * - `24` – Over 16 million colors (true color).
     * - `32` – Same as 24-bit color, but with extra bits (usually for
     * transparency).
     * 
     * This value reflects how the display mode is configured by the system, not
     * how the physical screen is built or what it is capable of. A display might
     * support high dynamic range (HDR) or more precise colors (like 10 bits per
     * channel), but that information is not captured here.
     *
     */
    readonly colorDepth: 8 | 16 | 24 | 32;
    /**
     * The refresh rate of the display mode in Hertz. The refresh rate is the
     * number of times the display is updated per second. For example, a refresh
     * rate of 60 Hz means the display is updated 60 times per second. Higher
     * refresh rates can reduce motion blur and flicker, and generally make motion
     * appear smoother, but require more processing power and bandwidth.
     *
     */
    readonly refreshRate: number;
    /**
     * Indicates whether the display mode is interlaced. Interlaced display modes
     * display every other line of the image in each frame. For example 480i
     * displays 240 lines in one frame and 240 lines in the next frame.
     * Non-interlaced display modes display all lines in each frame.
     *
     */
    readonly isInterlaced: boolean;
}
