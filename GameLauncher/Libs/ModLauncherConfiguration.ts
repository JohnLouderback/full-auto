import { BaseGameInfo } from "./BaseGameInfo";

// Auto-generated from C# type ModLauncherConfiguration
export interface ModLauncherConfiguration {
    /**
     * The configuration for the game and its associated mods, which includes the
     * base game information and any mods that can eb applied to it.
     *
     */
    game: BaseGameInfo;
    /**
     * The path to the background image for the mod launcher.
     *
     */
    backgroundImagePath?: string | null;
}
