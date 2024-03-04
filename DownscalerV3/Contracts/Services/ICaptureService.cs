using Windows.Win32.Foundation;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace DownscalerV3.Contracts.Services;

/// <summary>
///   A service that captures the contents of a window and displays them on a
///   <see cref="SwapChainPanel" />.
/// </summary>
public interface ICaptureService {
  /// <summary>
  ///   Occurs when the frame rate of the capture session changes. Provides the new frame rate and
  ///   the time it took to process the last frame.
  /// </summary>
  event EventHandler<(double newFrameRate, double newFrameTime)> FrameRateChanged;


  /// <summary>
  ///   Ends the current capture session. If nothing is being captured, then this method does
  ///   nothing.
  /// </summary>
  void EndCapture();


  /// <summary>
  ///   Invokes a window picker and captures the contents of the selected window. The captured
  ///   contents are then displayed on the provided <see cref="SwapChainPanel" />. If there is a
  ///   current capture session, then this method will end the current session and start a new one.
  /// </summary>
  /// <param name="swapChainPanel"> </param>
  /// <param name="dispatcherQueue">
  ///   The dispatcher queue to use for the capturer. Captured frames will be rendered on this
  ///   dispatcher queue. If omitted, the renderer will use a free-threaded approach entirely
  ///   independent of the UI thread. The trade-off between the two approaches is that the
  ///   free-threaded approach is faster but may cause issues with UI synchronization, while the
  ///   dispatcher queue approach is slower but is guaranteed to be thread-safe and synchronized with
  ///   the UI - possibly preventing lag spikes and other issues.
  /// </param>
  Task PickAndCaptureWindow(SwapChainPanel swapChainPanel, DispatcherQueue? dispatcherQueue = null);


  /// <summary>
  ///   Starts capturing the contents of a window and uses the provided <see cref="SwapChainPanel" />
  ///   to display the captured contents. If there is a current capture session, then this method
  ///   will end the current session and start a new one.
  /// </summary>
  /// <param name="window"> </param>
  /// <param name="swapChainPanel"> </param>
  /// <param name="dispatcherQueue">
  ///   The dispatcher queue to use for the capturer. Captured frames will be rendered on this
  ///   dispatcher queue. If omitted, the renderer will use a free-threaded approach entirely
  ///   independent of the UI thread. The trade-off between the two approaches is that the
  ///   free-threaded approach is faster but may cause issues with UI synchronization, while the
  ///   dispatcher queue approach is slower but is guaranteed to be thread-safe and synchronized with
  ///   the UI - possibly preventing lag spikes and other issues.
  /// </param>
  void StartCapture(
    HWND window,
    SwapChainPanel swapChainPanel,
    DispatcherQueue? dispatcherQueue = null
  );


  /// <summary>
  ///   Starts capturing the contents of the source window in the <see cref="AppState" /> and uses
  ///   the provided <see cref="SwapChainPanel" /> to display the captured contents. If there is a
  ///   current capture session, then this method will end the current session and start a new one.
  /// </summary>
  /// <param name="swapChainPanel"> </param>
  /// <param name="dispatcherQueue">
  ///   The dispatcher queue to use for the capturer. Captured frames will be rendered on this
  ///   dispatcher queue. If omitted, the renderer will use a free-threaded approach entirely
  ///   independent of the UI thread. The trade-off between the two approaches is that the
  ///   free-threaded approach is faster but may cause issues with UI synchronization, while the
  ///   dispatcher queue approach is slower but is guaranteed to be thread-safe and synchronized with
  ///   the UI - possibly preventing lag spikes and other issues.
  /// </param>
  void StartCapture(SwapChainPanel swapChainPanel, DispatcherQueue? dispatcherQueue = null);
}
