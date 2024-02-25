using CommunityToolkit.Mvvm.ComponentModel;
using DownscalerV3.Core.Contracts.Services;

namespace DownscalerV3.ViewModels;

public class MainViewModel : ObservableRecipient {
  private readonly IMouseEventService MouseEventService;

  private double frameRate;
  private double frameTime;
  private bool   showDebugInfo = true;
  private bool   showMouseCoords;
  private bool   showFPS;
  private bool   showEventsList;
  private bool   showPassedInArgs;
  private string mouseCoordsDetails;

  public string MouseCoordsDetails {
    get => mouseCoordsDetails;
    set => SetProperty(ref mouseCoordsDetails, value);
  }

  /// <summary>
  ///   The rate of frames per second that are being processed.
  /// </summary>
  public double FrameRate {
    get => frameRate;
    set {
      SetProperty(ref frameRate, value, true);
      OnPropertyChanged(nameof(FrameRateString)); // Also update the string representation.
    }
  }

  /// <summary>
  ///   The rate of frames per second that are being processed as a string in the form of "n" FPS.
  /// </summary>
  public string FrameRateString => $"{FrameRate:0} FPS";

  /// <summary>
  ///   The time in milliseconds that it took to process the last frame.
  /// </summary>
  public double FrameTime {
    get => frameTime;
    set {
      SetProperty(ref frameTime, value, true);
      OnPropertyChanged(nameof(FrameTimeString)); // Also update the string representation.
    }
  }

  /// <summary>
  ///   The time in milliseconds that it took to process the last frame as a string in the form of
  ///   "n.####" ms.
  /// </summary>
  public string FrameTimeString => $"{FrameTime:0.0000} ms";

  /// <summary>
  ///   Whether or not to show any debugging UI and information.
  /// </summary>
  public bool ShowDebugInfo {
    get => showDebugInfo;
    set {
      SetProperty(ref showDebugInfo, value);
      OnPropertyChanged(nameof(ShowMouseCoords));
      OnPropertyChanged(nameof(ShowFPS));
      OnPropertyChanged(nameof(ShowEventsList));
      OnPropertyChanged(nameof(ShowPassedInArgs));
    }
  }

  /// <summary>
  ///   Whether or not to show the current mouse coordinates.
  /// </summary>
  public bool ShowMouseCoords {
    get => showDebugInfo && showMouseCoords;
    set => SetProperty(ref showMouseCoords, value);
  }

  /// <summary>
  ///   Whether or not to show the current frames per second.
  /// </summary>
  public bool ShowFPS {
    get => showDebugInfo && showFPS;
    set => SetProperty(ref showFPS, value);
  }

  /// <summary>
  ///   Whether or not to show the current list of window events that have occurred.
  /// </summary>
  public bool ShowEventsList {
    get => showDebugInfo && showEventsList;
    set => SetProperty(ref showEventsList, value);
  }

  /// <summary>
  ///   Whether or not to show the passed in arguments from the command line.
  /// </summary>
  public bool ShowPassedInArgs {
    get => showDebugInfo && showPassedInArgs;
    set => SetProperty(ref showPassedInArgs, value);
  }


  public MainViewModel(IMouseEventService mouseEventService) {
    MouseEventService = mouseEventService;
    MouseEventService.MouseMoved += (sender, coords) => {
      // Update the mouse coordinates if the user has requested to see them.
      if (ShowMouseCoords) {
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
