import { ModInfo } from "./ModInfo";
import { OnLaunchCallback } from "./OnLaunchCallback";

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
    screenshotPath?: string | null;
    /**
     * A brief description of the game or mod, providing additional context or
     * information about it.
     *
     */
    description?: string | null;
    /**
     * The four digit year when the game or mod was released, used to provide
     * context for the item in the UI.
     *
     */
    releaseYear?: string | null;
    /**
     * Any custom metadata associated with the item, which can be used to store
     * additional information
     *
     */
    customMetadata?: Object | null;
    /**
     * A collection of mods that are specifically associated with this item, such
     * as other mods that can be applied to this game or only specifically to this
     * mod. These can be "mixed in" to the item to create a new configuration or
     * gameplay experience.
     *
     */
    mixins?: Array<ModInfo>;
    /**
     * A callback that is called when the game or a mod is launched. It provides
     * information about the base game, the mod (if a mod was chosen), and any
     * mixins that were selected. This callback is used to perform any necessary
     * actions or configurations before the game is started.
     *
     */
    onLaunch?: OnLaunchCallback | null;
}
