using Downscaler.Core.Contracts.Models;

namespace Downscaler.Core.Contracts.Services;

public interface IMouseEventService {
  /// <summary>
  ///   Represents the last known position of the mouse. This should remain up-to-date with the
  ///   current position of the mouse throughout the lifetime of the application.
  /// </summary>
  IMouseCoords CurrentMouseCoords { get; }

  /// <summary>
  ///   This event is raised when the mouse is moved. Mouse movement events are raised with a very
  ///   high frequency. Listeners in the UI should likely debounce the event to avoid performance
  ///   issues.
  /// </summary>
  event EventHandler<IMouseCoords> MouseMoved;


  /// <summary>
  ///   Updates the mouse position in absolute screen coordinates.
  /// </summary>
  /// <param name="x"> The x-coordinate of the mouse. </param>
  /// <param name="y"> The y-coordinate of the mouse. </param>
  void UpdateMousePosition(int x, int y);
}
