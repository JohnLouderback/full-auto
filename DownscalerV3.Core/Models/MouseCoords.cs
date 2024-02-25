using System.Drawing;
using DownscalerV3.Core.Contracts.Models;
using DownscalerV3.Core.Utils;

namespace DownscalerV3.Core.Models;

public struct MouseCoords : IMouseCoords {
  private Point? relativeToDownscaledWindow;
  private Point? relativeToSourceWindow;
  private bool?  isWithinDownscaledWindow;

  // ReSharper disable once InconsistentNaming
  /// <summary>
  ///   The application state. This is used to obtain the source window and the downscaled window.
  ///   This should be set, indirectly, by the application's DI container - through a service that
  ///   handles mouse events.
  /// </summary>
  internal static IAppState? AppState;


  public MouseCoords(Point absolutePosition) {
    Absolute = absolutePosition;
  }


  /// <inheritdoc />
  public Point Absolute { get; }

  /// <inheritdoc />
  public Point RelativeToDownscaledWindow {
    get {
      // If we already have the relative position, return it.
      if (this.relativeToDownscaledWindow is not null) {
        return (Point)this.relativeToDownscaledWindow;
      }

      // If AppState is null, throw an exception. AppState should be set, indirectly, by the
      // application's DI container.
      if (AppState is null) {
        throw new InvalidOperationException(
          "AppState is was not set. Unable to obtain the state of the application."
        );
      }

      // Get the coordinates of the downscaled window.
      var downscaledWindow   = AppState.DownscaleWindow;
      var absoluteClientRect = downscaledWindow.GetAbsoluteClientRect();

      // Calculate the relative position of the mouse to the downscaled window.
      var relativeToDownscaledWindow = new Point(
        Absolute.X - absoluteClientRect.left,
        Absolute.Y - absoluteClientRect.top
      );
      return this.relativeToDownscaledWindow ??= relativeToDownscaledWindow;
    }
  }

  /// <inheritdoc />
  public Point RelativeToSourceWindow {
    get {
      // If we already have the relative position, return it.
      if (relativeToSourceWindow is not null) {
        return (Point)relativeToSourceWindow;
      }

      // If AppState is null, throw an exception. AppState should be set, indirectly, by the
      // application's DI container.
      if (AppState is null) {
        throw new InvalidOperationException(
          "AppState is was not set. Unable to obtain the state of the application."
        );
      }

      // Get the source window.
      var sourceWindow = AppState.WindowToScale;

      // Calculate the scale factors to scale the coordinates of the mouse to the source window.
      var scaleFactorX = (double)sourceWindow.GetClientWidth() /
                         AppState.DownscaleWindow.GetClientWidth();
      var scaleFactorY = (double)sourceWindow.GetClientHeight() /
                         AppState.DownscaleWindow.GetClientHeight();

      return relativeToSourceWindow ??= new Point(
               (int)(RelativeToDownscaledWindow.X * scaleFactorX),
               (int)(RelativeToDownscaledWindow.Y * scaleFactorY)
             );
    }
  }

  /// <inheritdoc />
  public bool IsWithinDownscaledWindow {
    get {
      // If we've already calculated whether the mouse is within the downscaled window, return the
      // result.
      if (isWithinDownscaledWindow is not null) {
        return (bool)isWithinDownscaledWindow;
      }

      // If AppState is null, throw an exception. AppState should be set, indirectly, by the
      // application's DI container.
      if (AppState is null) {
        throw new InvalidOperationException(
          "AppState is was not set. Unable to obtain the state of the application."
        );
      }

      // Get the coordinates of the downscaled window.
      var downscaledWindow   = AppState.DownscaleWindow;
      var absoluteClientRect = downscaledWindow.GetAbsoluteClientRect();
      return isWithinDownscaledWindow ??= absoluteClientRect.Contains(Absolute);
    }
  }
}
