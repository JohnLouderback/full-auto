namespace GameLauncher.Utils.SendKeys;

/// <summary>
///   Represents a keystroke event. This class is used to store information about a key being
///   pressed or released.
/// </summary>
public class Keystroke {
  /// <summary>
  ///   The key name (e.g. "a", "Enter", "Ctrl", etc.)
  /// </summary>
  public string Key { get; set; }

  /// <summary>
  ///   True for a key-down event, false for key-up.
  /// </summary>
  public bool IsDown { get; set; }


  public Keystroke(string key, bool isDown) {
    Key    = key;
    IsDown = isDown;
  }


  public override string ToString() {
    return $"{Key} {(IsDown ? "Down" : "Up")}";
  }
}
