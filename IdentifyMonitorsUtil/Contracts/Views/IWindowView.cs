namespace IdentifyMonitorsUtil.Contracts.Views;

/// <summary>
///   Abstractly represents a UI Window as a view.
/// </summary>
public interface IWindowView {
  /// <summary>
  ///   Raised when the window is closed.
  /// </summary>
  public event EventHandler? Closed;


  /// <summary>
  ///   Shows the window so that it is visible to the user.
  /// </summary>
  void Show();
}
