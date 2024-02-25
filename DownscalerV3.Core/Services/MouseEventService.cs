using System.Drawing;
using DownscalerV3.Core.Contracts.Models;
using DownscalerV3.Core.Contracts.Services;
using DownscalerV3.Core.Models;

namespace DownscalerV3.Core.Services;

public class MouseEventService : IMouseEventService {
  private readonly IAppState AppState;

  /// <inheritdoc />
  public IMouseCoords CurrentMouseCoords { get; private set; }

  /// <inheritdoc />
  public event EventHandler<IMouseCoords>? MouseMoved;


  public MouseEventService(IAppState appState) {
    AppState           = appState;
    CurrentMouseCoords = new MouseCoords(new Point(0, 0));
    // The MouseCoords struct requires the AppState to be set. This is a bit of a hack, but it's
    // necessary because otherwise there is no way for the MouseCoords struct to obtain the
    // application state and, thereby, the source and downscaled windows. Having these calculations
    // happen within the struct, through getters, allows us to only perform the calculations when
    // they're actually needed. This is a performance optimization.
    MouseCoords.AppState = appState;
  }


  /// <inheritdoc />
  public void UpdateMousePosition(int x, int y) {
    CurrentMouseCoords = new MouseCoords(new Point(x, y));
    MouseMoved?.Invoke(this, CurrentMouseCoords);
  }
}
