namespace Core.Contracts.Models;

public interface IUserConfiguredMonitor {
  /// <summary>
  ///   Specifies the order of preference for the user's monitor. For example "1" denotes the display
  ///   is intended as the user's primary display, "2" denotes the display is intended as the user's
  ///   secondary display, and so on. The doesn't have any effect on the actual display configuration
  ///   in the system.
  /// </summary>
  public uint UserPreferenceOrder { get; set; }
}
