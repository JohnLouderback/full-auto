// Auto-generated from C# type SourcePortInfo
export interface SourcePortInfo {
    /**
     * The display name of the source port, used in the UI to identify the source
     * port.
     *
     */
    readonly displayName: string;
    /**
     * The unique identifier for the source port, used to distinguish it from
     * other source ports.
     *
     */
    readonly id: string;
    /**
     * The path to the source port executable, which is used to run the game with
     * this source port. This is typically the path to the modified game engine
     * executable.
     *
     */
    readonly sourcePortPath?: string;
}
