namespace Downscaler.Helpers.Graphics;

/// <summary>
///   Represents a class that can capture a window and render it to a swap chain panel.
/// </summary>
public interface ICapturer {
  /// <summary>
  ///   Event that is raised when the frame rate changes. The new frame rate is passed as the
  ///   argument and is a double in the form of "n" frames per second. The frame time is also
  ///   passed as a double in the form of "n" milliseconds spent per frame.
  /// </summary>
  event SimpleCapturer.FrameRateChangedEventHandler? FrameRateChanged;


  /// <summary>
  ///   Ends the capture session and cleans up any resources that were used.
  /// </summary>
  void Close();


  /// <summary>
  ///   Begins capturing the window and rendering it to the swap chain panel.
  /// </summary>
  void StartCapture();
}
