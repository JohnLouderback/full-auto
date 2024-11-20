using System.ComponentModel;
using DownscalerV3.Contracts.Services;
using DownscalerV3.Core.Contracts.Models.AppState;
using DownscalerV3.Core.Contracts.Services;
using DownscalerV3.Core.Utils;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DownscalerV3.ViewModels;

/// <summary>
///   The event handler for when the position of the downscaler window changes.
/// </summary>
public delegate void PositionChangedEventHandler(object sender, object? args);

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
  public bool ShowDebugInfo => AppState.DebugState.Enabled;

  /// <summary>
  ///   Whether or not the user wishes to show the current mouse coordinates.
  /// </summary>
  public bool ShouldShowMouseCoords => AppState.DebugState.ShowMouseCoordinates;

  /// <summary>
  ///   Whether or not the UI is allowed to show the current mouse coordinates. If debug info is
  ///   turned off, then the mouse coordinates should not be shown even if the user has requested to
  ///   see them.
  /// </summary>
  public bool CanShowMouseCoords => ShowDebugInfo && ShouldShowMouseCoords;

  /// <summary>
  ///   Whether or not the user wishes to show the current frames per second.
  /// </summary>
  public bool ShouldShowFPS => AppState.DebugState.ShowFps;

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
  ///   The current height of the window as reported by the downscaler window.
  /// </summary>
  public int WindowHeight { get; set; }

  /// <summary>
  ///   The current width of the window as reported by the downscaler window.
  /// </summary>
  public int WindowWidth { get; set; }

  /// <summary>
  ///   To properly align the pixel font with the pixel grid, we need to nudge it by a certain amount
  ///   at certain DPIs. As it stands, I cannot find any rhyme or reason to the nudge amount, so the
  ///   value are hard-coded.
  /// </summary>
  public double PixelFontNudge {
    get {
      // The function calculates a nudge value based on the fractional part of DpiScaleFactor. The
      // function creates a repeating waveform with:
      // - Peaks at 0.25 (nudge = 0.64) and 0.75 (nudge = 0.32)
      // - Zeros at 0.0, 0.5, and 1.0.
      //
      // Nudge Value
      //    ^
      //  0.64|------------*---------------------------------------
      //      |           * *        
      //      |          *   *       
      //      |         *     *      
      //  0.48|--------*-------*-----------------------------------
      //      |       *         *     
      //      |      *           *    
      //      |     *             *                
      //  0.32|----*---------------*-----------------*-------------
      //      |   *                 *             *     *
      //      |  *                   *         *           *
      //      | *                     *     *                 *
      //  0.0 |*                       * *                       *
      //      +----------------------------------------------------
      //       ^           ^            ^            ^           ^   
      //       0        0.25          0.5         0.75         1.0
      //             Fractional Part of DpiScaleFactor

      var frac = DpiScaleFactor - Math.Floor(DpiScaleFactor);

      // Calculate nudge value based on fractional part
      var nudge = 0.64 * Math.Max(0, 1 - 4 * Math.Abs(frac - 0.25)) +
                  0.32 * Math.Max(0, 1 - 4 * Math.Abs(frac - 0.75));

      return nudge;
    }
  }

  /// <summary>
  ///   Determines the pixel font size to use based on the current window height.
  /// </summary>
  public double PixelFontSize {
    get {
      // The 1:1 font size is the base font size that the other font sizes are based on.
      var baseFontSize = (double)Application.Current.Resources["MainPixelFontSizeOneToOne"];

      // If the configuration specifies a font size, use that.
      if (AppState.DebugState.FontSize is not null) {
        // The configuration value with be a positive integer, greater than or equal to 1. 1
        // represents a 1:1 ratio with the screen pixels, 2 represents a 2:1 ratio with the screen
        // pixels, and so on. We scale the font size by the DPI scale factor to ensure that the font
        // size is correct for the current DPI.
        return
          Math.Round(
            AppState.DebugState.FontSize.Value *
            baseFontSize *
            (1 / DpiScaleFactor),
            3,
            MidpointRounding.AwayFromZero
          );
      }

      return WindowHeight switch {
        < 480 => baseFontSize *
                 DpiScaleFactor,
        < 720 => (double)Application.Current.Resources["MainPixelFontSizeTwoToOne"] *
                 DpiScaleFactor,
        >= 720 => (double)Application.Current.Resources["MainPixelFontSizeThreeToOne"] *
                  DpiScaleFactor
      };
    }
  }

  /// <summary>
  ///   The "scale factor" is used to adjust sizing from device-independent pixels (DIPs) to physical
  ///   pixels. This is necessary because the source window's dimensions are in physical pixels, but
  ///   the downscale window's dimensions are in DIPs.
  /// </summary>
  public float DpiScaleFactor => AppState.DownscaleWindow.GetMonitor().GetDpi() / 96f;

  /// <summary>
  ///   Raised when the position of the downscaler window changes.
  /// </summary>
  public event PositionChangedEventHandler PositionChanged;


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

    // Ensure that properties whose values are derived from the position of the downscaler window
    // are updated when the position changes.
    PositionChanged += (_, _) => UpdateAfterPositionChange();
  }


  /// <summary>
  ///   Raises the <see cref="PositionChanged" /> event.
  /// </summary>
  public void RaisePositionChanged() {
    PositionChanged?.Invoke(this, null);
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


  /// <summary>
  ///   Updates view model properties after the position of the downscaler window changes. Some
  ///   properties are derived from the position of the window, so they need to be updated when the
  ///   position changes.
  /// </summary>
  private void UpdateAfterPositionChange() {
    Console.WriteLine("Position changed!");
    OnPropertyChanged(nameof(DpiScaleFactor));
    OnPropertyChanged(nameof(PixelFontSize));
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
