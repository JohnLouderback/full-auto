import { Screen } from "./Screen";
import { MakeMonitorPrimaryResult } from "./MakeMonitorPrimaryResult";

/**
 * Represents a monitor (e.g. a display) connected to the system.
 *
 */
/**
 * Represents a monitor (e.g. a display) connected to the system.
 *
 */
// Auto-generated from C# class Monitor
export interface Monitor {
    /**
     * Represents the graphical display of a monitor. For example, the screen
     * resolution, color
     * depth, refresh rate, and other display settings that determined in
     * software.
     *
     */
    readonly screen: Screen;
    /**
     * The handle of the monitor. This is a unique identifier representing the
     * monitor.
     *
     */
    handle: number;
    /**
     * Indicates whether the monitor is the primary monitor. The primary monitor
     * is the one that
     * is used to display the taskbar and the desktop. It is typically the
     * monitor that fullscreen
     * applications are displayed on in the absence of a specified monitor.
     *
     */
    readonly isPrimary: boolean;
    /**
     * The device ID of the monitor. The device ID is a unique identifier for
     * the monitor. This
     * value is typically derived from the monitor's EDID and is not
     * user-editable.
     * Example: `"ACME1234"`.
     *
     */
    readonly deviceID: string;
    /**
     * The raw device ID of the monitor. The raw device ID is a unique
     * identifier for the monitor.
     * This value is typically derived from the monitor's EDID and is not
     * user-editable.
     * Example:
     * `"MONITOR\ACME1234\{4d36e96e-e325-11ce-bfc1-08002be10318}\0001"`.
     *
     */
    deviceIDRaw: string;
    /**
     * The device name for the monitor. This identifies the monitor as a display
     * device in the
     * system. This value is based on the order the monitor was detected by the
     * system and can be
     * influenced by which port the monitor is connected to.
     * Example: `"DISPLAY1"`.
     *
     */
    readonly deviceName: string;
    /**
     * The device name for the monitor. This identifies the monitor as a display
     * device in the
     * system. This value is based on the order the monitor was detected by the
     * system and can be
     * influenced by which port the monitor is connected to.
     * Example: `"\\.\DISPLAY1\Monitor0"`.
     *
     */
    deviceNameRaw: string;
    /**
     * The device string for the monitor. The device name is a human-readable
     * name for the monitor.
     * This value is influenced by monitor's EDID and can be changed by the
     * user.
     * Example: `"Generic PnP Monitor"`.
     *
     */
    readonly deviceString: string;
    /**
     * The device string for the monitor. The device name is a human-readable
     * name for the monitor.
     * This value is influenced by monitor's EDID and can be changed by the
     * user.
     * Example: `"Generic PnP Monitor(HDMI)"`.
     *
     */
    deviceStringRaw: string;
    /**
     * The device key for the monitor. The device key is a unique identifier for
     * the monitor. This
     * value is the registry key for the monitor in the system.
     * Example:
     * `"\REGISTRY\MACHINE\SYSTEM\ControlSet001\Enum\DISPLAY\ACME1234\4&12345678&0&
     * \UID123456"`
     * .
     *
     */
    readonly deviceKey: string;
    /**
     * Sets this monitor as the primary monitor. The primary monitor is the one
     * that is used to display
     * the taskbar and the desktop. It is typically the monitor that fullscreen
     * applications are
     * displayed
     * on by default.
     *
     * @param [shouldPersist=false] Whether to persist the change to the
     * registry. If `false`, the change is temporary and
     * will be reset when the script finishes executing. If `true`, the change
     * is permanent
     * and will persist after the script finishes executing and across system
     * restarts.
     * @returns The result of making the monitor primary. If you need to revert
     * back to the previous primary
     * monitor, you can call the {@link MakeMonitorPrimaryResult.Undo} method on
     * the result.
     */
    makePrimary(shouldPersist?: boolean): MakeMonitorPrimaryResult;
}
