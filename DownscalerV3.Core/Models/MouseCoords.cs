using System.Drawing;
using DownscalerV3.Core.Contracts.Models;
using DownscalerV3.Core.Contracts.Models.AppState;
using DownscalerV3.Core.Utils;

namespace DownscalerV3.Core.Models;

public struct MouseCoords : IMouseCoords {
  private Point?              relativeToDownscaledWindow;
  private Point?              relativeToSourceWindow;
  private (float X, float Y)? relativeToDownscaledWindowPercent;
  private (float X, float Y)? relativeToSourceWindowPercent;
  private bool?               isWithinDownscaledWindow;
  private bool?               isWithinSourceWindow;

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
  public (float X, float Y) RelativeToDownscaledWindowPercent {
    get {
      // If we already have the relative position, return it.
      if (this.relativeToDownscaledWindowPercent is not null) {
        return ((float X, float Y))this.relativeToDownscaledWindowPercent;
      }

      // If AppState is null, throw an exception. AppState should be set, indirectly, by the
      // application's DI container.
      if (AppState is null) {
        throw new InvalidOperationException(
          "AppState is was not set. Unable to obtain the state of the application."
        );
      }

      // Get the coordinates of the downscaled window.
      var downscaledWindow = AppState.DownscaleWindow;

      // Calculate the relative position of the mouse to the downscaled window.
      var relativeToDownscaledWindow = RelativeToDownscaledWindow;
      var relativeToDownscaledWindowPercent =
        (
          (float)relativeToDownscaledWindow.X / downscaledWindow.GetClientWidth(),
          (float)relativeToDownscaledWindow.Y / downscaledWindow.GetClientHeight()
        );
      return this.relativeToDownscaledWindowPercent ??= relativeToDownscaledWindowPercent;
    }
  }

  /// <inheritdoc />
  public (float X, float Y) RelativeToSourceWindowPercent {
    get {
      // If we already have the relative position, return it.
      if (this.relativeToSourceWindowPercent is not null) {
        return ((float X, float Y))this.relativeToSourceWindowPercent;
      }

      // If AppState is null, throw an exception. AppState should be set, indirectly, by the
      // application's DI container.
      if (AppState is null) {
        throw new InvalidOperationException(
          "AppState is was not set. Unable to obtain the state of the application."
        );
      }

      // Get the coordinates of the downscaled window.
      var sourceWindow = AppState.WindowToScale;

      // Calculate the relative position of the mouse to the downscaled window.
      var relativeToSourceWindow = RelativeToSourceWindow;
      var relativeToSourceWindowPercent =
        (
          (float)relativeToSourceWindow.X / sourceWindow.GetClientWidth(),
          (float)relativeToSourceWindow.Y / sourceWindow.GetClientHeight()
        );
      return this.relativeToSourceWindowPercent ??= relativeToSourceWindowPercent;
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

  /// <inheritdoc />
  public bool IsWithinSourceWindow {
    get {
      // If we've already calculated whether the mouse is within the source window, return the
      // result.
      if (isWithinSourceWindow is not null) {
        return (bool)isWithinSourceWindow;
      }

      // If AppState is null, throw an exception. AppState should be set, indirectly, by the
      // application's DI container.
      if (AppState is null) {
        throw new InvalidOperationException(
          "AppState is was not set. Unable to obtain the state of the application."
        );
      }

      // Get the coordinates of the source window.
      var sourceWindow       = AppState.WindowToScale;
      var absoluteClientRect = sourceWindow.GetAbsoluteClientRect();
      return isWithinSourceWindow ??= absoluteClientRect.Contains(Absolute);
    }
  }
}
