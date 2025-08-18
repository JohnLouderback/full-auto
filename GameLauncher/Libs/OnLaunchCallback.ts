import { BaseGameInfo } from "./BaseGameInfo";
import { ModInfo } from "./ModInfo";

/**
 * A callback that is called when the game is launched. It provides
 * information about the base games, the mod (if a mod was chosen), and any
 * mixins that were selected. This callback should handle the logic for
 * launching the game with the selected mod and mixins.
 *
 */
// Auto-generated from delegate OnLaunchCallback
export type OnLaunchCallback = (baseGame: BaseGameInfo, mod?: ModInfo | null, mixins?: Array<ModInfo> | null) => Promise<void>;
