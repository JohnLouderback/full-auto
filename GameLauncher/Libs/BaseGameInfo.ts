import { ItemInfo } from "./ItemInfo";
import { ModInfo } from "./ModInfo";

// Auto-generated from C# type BaseGameInfo
export interface BaseGameInfo extends ItemInfo {
    /**
     * The path to the base game executable.
     *
     */
    gamePath: string;
    /**
     * The path to the logo image for the game, used in the UI to visually
     * represent the game.
     *
     */
    logoPath?: string;
    /**
     * A collection of pre-configured mods that are associated with this game.
     *
     */
    mods?: Array<ModInfo>;
}
