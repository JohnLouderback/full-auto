using System.ComponentModel;
using DownscalerV3.Contracts.Services;
using DownscalerV3.Core.Contracts.Models;
using DownscalerV3.Core.Contracts.Services;
using DownscalerV3.Core.Utils;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace DownscalerV3.ViewModels;

public partial class MainViewModel : INotifyPropertyChanged {
  private readonly IAppState AppState;

  public string MouseCoordsDetails { get; set; }

  /// <summary>
  ///   The rate of frames per second that are being processed.
  /// </summary>
  public double FrameRate { get; set; }

  /// <summary>
  ///   The rate of frames per second that are being processed as a string in the form of "n" FPS.
  /// </summary>
  public string FrameRateString => $"{FrameRate:0} FPS";

  /// <summary>
  ///   The time in milliseconds that it took to process the last frame.
  /// </summary>
  public double FrameTime { get; set; }

  /// <summary>
  ///   The time in milliseconds that it took to process the last frame as a string in the form of
  ///   "n.####" ms.
  /// </summary>
  public string FrameTimeString => $"{FrameTime:0.0000} ms";

  /// <summary>
  ///   Whether or not to show any debugging UI and information.
  /// </summary>
  public bool ShowDebugInfo { get; set; } = true;

  /// <summary>
  ///   Whether or not the user wishes to show the current mouse coordinates.
  /// </summary>
  public bool ShouldShowMouseCoords { get; set; }

  /// <summary>
  ///   Whether or not the UI is allowed to show the current mouse coordinates. If debug info is
  ///   turned off, then the mouse coordinates should not be shown even if the user has requested to
  ///   see them.
  /// </summary>
  public bool CanShowMouseCoords => ShowDebugInfo && ShouldShowMouseCoords;

  /// <summary>
  ///   Whether or not the user wishes to show the current frames per second.
  /// </summary>
  public bool ShouldShowFPS { get; set; }

  /// <summary>
  ///   Whether or not the UI is allowed to show the current frames per second. If debug info is
  ///   turned off, then the frames per second should not be shown even if the user has requested to
  ///   see them.
  /// </summary>
  public bool CanShowFPS => ShowDebugInfo && ShouldShowFPS;

  /// <summary>
  ///   Whether or not the user wishes to show the current list of window events that have occurred.
  /// </summary>
  public bool ShouldShowEventsList { get; set; }

  /// <summary>
  ///   Whether or not the UI is allowed to show the current list of window events. If debug info is
  ///   turned off, then the list of window events should not be shown even if the user has requested
  ///   to see them.
  /// </summary>
  public bool CanShowEventsList => ShowDebugInfo && ShouldShowEventsList;

  /// <summary>
  ///   Whether or not the user wishes to show the passed in arguments from the command line.
  /// </summary>
  public bool ShouldShowPassedInArgs { get; set; }

  /// <summary>
  ///   Whether or not the UI is allowed to show the passed in arguments from the command line. If
  ///   debug info is turned off, then the passed in arguments should not be shown even if the user
  ///   has requested to see them.
  /// </summary>
  public bool CanShowPassedInArgs => ShowDebugInfo && ShouldShowPassedInArgs;

  /// <summary>
  ///   The width to downscale the source window to.
  /// </summary>
  public int DownscaleWidth =>
    // We need to divide by the DPI scale factor to get the correct width because XAML uses DIPs
    // and not physical pixels.
    (int)(AppState.DownscaleWidth / DpiScaleFactor);

  /// <summary>
  ///   The height to downscale the source window to.
  /// </summary>
  public int DownscaleHeight =>
    // We need to divide by the DPI scale factor to get the correct height because XAML uses DIPs
    // and not physical pixels.
    (int)(AppState.DownscaleHeight / DpiScaleFactor);

  /// <summary>
  ///   The "scale factor" is used to adjust sizing from device-independent pixels (DIPs) to physical
  ///   pixels. This is necessary because the source window's dimensions are in physical pixels, but
  ///   the downscale window's dimensions are in DIPs.
  /// </summary>
  private float DpiScaleFactor => AppState.DownscaleWindow.GetMonitor().GetDpi() / 96f;


  public MainViewModel(
    IMouseEventService mouseEventService,
    ICaptureService captureService,
    IAppState appState
  ) {
    AppState          = appState;
    MouseEventService = mouseEventService;
    CaptureService    = captureService;
    UpdateMouseCoordsDetails();
    MouseEventService.MouseMoved += (sender, coords) => {
      // Update the mouse coordinates if the user has requested to see them.
      if (ShouldShowMouseCoords) {
        UpdateMouseCoordsDetails();
      }
    };
  }


  /// <summary>
  ///   Given a <see cref="SwapChainPanel" />, starts capturing the contents of the source window in
  ///   the <see cref="AppState" /> and uses the provided <see cref="SwapChainPanel" /> to display
  ///   the captured contents.
  /// </summary>
  /// <param name="swapChainPanel">
  ///   The <see cref="SwapChainPanel" /> to display the captured contents on.
  /// </param>
  /// <param name="dispatcherQueue">
  ///   The dispatcher queue to use for the capturer. Captured frames will be rendered on this
  ///   dispatcher queue. If omitted, the renderer will use a free-threaded approach entirely
  ///   independent of the UI thread. The trade-off between the two approaches is that the
  ///   free-threaded approach is faster but may cause issues with UI synchronization, while the
  ///   dispatcher queue approach is slower but is guaranteed to be thread-safe and synchronized with
  ///   the UI - possibly preventing lag spikes and other issues.
  /// </param>
  public void StartCapture(SwapChainPanel swapChainPanel, DispatcherQueue? dispatcherQueue = null) {
    // Start the capture service.
    // CaptureService.PickAndCaptureWindow(swapChainPanel);
    CaptureService.StartCapture(swapChainPanel, dispatcherQueue);
    CaptureService.FrameRateChanged += (_, args) => {
      swapChainPanel.DispatcherQueue.TryEnqueue(
        () => {
          FrameRate = args.newFrameRate;
          FrameTime = args.newFrameTime;
        }
      );
    };
  }


  private void UpdateMouseCoordsDetails() {
    MouseCoordsDetails = $"""
      Absolute Position: (X:{
        MouseEventService.CurrentMouseCoords.Absolute.X.ToString(),
        5}px, Y:{
        MouseEventService.CurrentMouseCoords.Absolute.Y.ToString(),
        5}px)
      Relative to downscaled window: (X:{
        MouseEventService.CurrentMouseCoords.RelativeToDownscaledWindow.X.ToString(),
        5
      }px, Y:{
        MouseEventService.CurrentMouseCoords.RelativeToDownscaledWindow.Y.ToString(),
        5
      }px)
      (X:{
        MouseEventService.CurrentMouseCoords.RelativeToDownscaledWindowPercent.X,
        7:P2}, Y:{
        MouseEventService.CurrentMouseCoords.RelativeToDownscaledWindowPercent.Y,
        7:P2})
      Relative to source window: (X:{
        MouseEventService.CurrentMouseCoords.RelativeToSourceWindow.X.ToString(),
        5
      }px, Y:{
        MouseEventService.CurrentMouseCoords.RelativeToSourceWindow.Y.ToString(),
        5
      }px)
      (X:{
        MouseEventService.CurrentMouseCoords.RelativeToSourceWindowPercent.X,
        7:P2}, Y:{
        MouseEventService.CurrentMouseCoords.RelativeToSourceWindowPercent.Y,
        7:P2})
      """;
  }


  // ReSharper disable InconsistentNaming
  private readonly IMouseEventService MouseEventService;

  private readonly ICaptureService CaptureService;
  // ReSharper restore InconsistentNaming
}
