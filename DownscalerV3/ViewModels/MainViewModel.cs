using System.ComponentModel;
using DownscalerV3.Core.Contracts.Services;

namespace DownscalerV3.ViewModels;

public partial class MainViewModel : INotifyPropertyChanged {
  private readonly IMouseEventService MouseEventService;

  private double frameRate;
  private double frameTime;
  private bool   showDebugInfo = true;
  private bool   showMouseCoords;
  private bool   showFPS;
  private bool   showEventsList;
  private bool   showPassedInArgs;
  private string mouseCoordsDetails;

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


  public MainViewModel(IMouseEventService mouseEventService) {
    MouseEventService = mouseEventService;
    UpdateMouseCoordsDetails();
    MouseEventService.MouseMoved += (sender, coords) => {
      // Update the mouse coordinates if the user has requested to see them.
      if (ShouldShowMouseCoords) {
        UpdateMouseCoordsDetails();
      }
    };
  }


  private void UpdateMouseCoordsDetails() {
    MouseCoordsDetails = $"""
      X: {
        MouseEventService.CurrentMouseCoords.Absolute.X.ToString(),
        5}
      Y: {
        MouseEventService.CurrentMouseCoords.Absolute.Y.ToString(),
        5}
      Relative to downscaled window: {
        MouseEventService.CurrentMouseCoords.RelativeToDownscaledWindow
      }
      Relative to source window: {
        MouseEventService.CurrentMouseCoords.RelativeToSourceWindow
      }
      """;
  }
}
