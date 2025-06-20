import { ModInfo } from "./ModInfo";

// Auto-generated from C# type ItemInfo
export interface ItemInfo {
    /**
     * The display name of the item—the game or mod, used in the UI to identify
     * the game.
     *
     */
    displayName: string;
    /**
     * The unique identifier for the game, used to distinguish it from other items
     * such as other mods.
     *
     */
    id: string;
    /**
     * The path to the screenshot image for the item, used in the UI to visually
     * represent the game or mod.
     *
     */
    screenshotPath?: string;
    /**
     * A brief description of the game or mod, providing additional context or
     * information about it.
     *
     */
    description?: string;
    /**
     * The four digit year when the game or mod was released, used to provide
     * context for the item in the UI.
     *
     */
    releaseYear?: string;
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
