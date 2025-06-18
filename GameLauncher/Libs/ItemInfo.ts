import { ModInfo } from "./ModInfo";

// Auto-generated from C# type ItemInfo
export interface ItemInfo {
    /**
     * The display name of the item—the game or mod, used in the UI to identify
     * the game.
     *
     */
    readonly displayName: string;
    /**
     * The unique identifier for the game, used to distinguish it from other items
     * such as other mods.
     *
     */
    readonly id: string;
    /**
     * The path to the screenshot image for the item, used in the UI to visually
     * represent the game or mod.
     *
     */
    readonly screenshotPath?: string;
    /**
     * A brief description of the game or mod, providing additional context or
     * information about it.
     *
     */
    readonly description?: string;
    /**
     * Any custom metadata associated with the item, which can be used to store
     * additional information
     *
     */
    customMetadata?: Object;
    /**
     * A collection of mods that are specifically associated with this item, such
     * as other mods that can be applied to this game or only specifically to this
     * mod. These can be "mixed in" to the item to create a new configuration or
     * gameplay experience.
     *
     */
    mixins?: Array<ModInfo>;
}
