using Windows.Win32.Foundation;

namespace DownscalerV3.Contracts.Services;

/// <summary>
///   Service for handling the win32 window events for the given window.
/// </summary>
public interface IWindowEventHandlerService {
  /// <summary>
  ///   Initializes the window event handler service for the given window handle. This method
  ///   will listen for win32 window events update the application state accordingly.
  /// </summary>
  /// <param name="hwnd"> </param>
  void InitializeForWindow(HWND hwnd);
}
