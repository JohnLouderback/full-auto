namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents the state of a window, such as whether it is shown, hidden, minimized, or maximized.
/// </summary>
[Flags]
public enum WindowState {
  NONE      = 0,
  SHOWN     = 0b0001, // 1
  HIDDEN    = 0b0010, // 2
  MINIMIZED = 0b0100, // 4
  MAXIMIZED = 0b1000 // 8
}
