using GameLauncher.Script.Objects;
using GameLauncher.Script.Utils.CodeGenAttributes;
using GenericModLauncher.Services;
using Microsoft.ClearScript;

namespace GameLauncher.Script;

public static partial class Tasks {
  /// <summary>
  ///   Shows the mod launcher window. This is a tool for configuring a window to show a list of
  ///   game mods, allowing users to choose, enable, disable, and ultimately launch the game with
  ///   selected mods.
  /// </summary>
  public static async Task ShowModLauncher(ModLauncherConfiguration configuration) {
    await GuiService.Instance.ShowModLauncher(configuration);
  }


  [HideFromTypeScript]
  public static async Task ShowModLauncher(ScriptObject configuration) {
    await ShowModLauncher(
      (ModLauncherConfiguration)configuration
    );
  }
}
