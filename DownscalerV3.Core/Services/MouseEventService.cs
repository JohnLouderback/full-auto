using System.Diagnostics;
using System.Drawing;
using Windows.Win32.System.SystemServices;
using DownscalerV3.Core.Contracts.Models;
using DownscalerV3.Core.Contracts.Services;
using DownscalerV3.Core.Models;
using DownscalerV3.Core.Utils;
using static DownscalerV3.Core.Utils.Macros;
using static Windows.Win32.PInvoke;

namespace DownscalerV3.Core.Services;

public class MouseEventService : IMouseEventService {
  private readonly IAppState AppState;

  /// <summary>
  ///   The stopwatch monitoring the time since the last mouse event. Used to throttle the mouse
  ///   events that are forwarded to the source window.
  /// </summary>
  private readonly Stopwatch lastEventTimer = new();

  /// <summary>
  ///   Represents the current known state of the mouse buttons along with certain other modifier
  ///   keys such as shift and control.
  /// </summary>
  private MODIFIERKEYS_FLAGS mouseButtonState = 0;

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
    lastEventTimer.Start();
  }


  /// <inheritdoc />
  public void UpdateMousePosition(int x, int y) {
    CurrentMouseCoords = new MouseCoords(new Point(x, y));
    ForwardMouseEventToSourceWindow();
    MouseMoved?.Invoke(this, CurrentMouseCoords);
  }


  private void ForwardMouseEventToSourceWindow() {
    // Throttle the mouse events that are forwarded to the source window to 60 events per second.
    if (lastEventTimer.ElapsedMilliseconds < 16) {
      return;
    }

    lastEventTimer.Restart();

    // If the mouse is not within the downscaled window, don't forward the event.
    if (!CurrentMouseCoords.IsWithinDownscaledWindow) {
      return;
    }

    var sourceWindow       = AppState.WindowToScale;
    var sourceWindowAbsPos = sourceWindow.GetAbsoluteClientRect();
    var mouseInset         = sourceWindow.GetClientRectRelativeToWindow();
    var x                  = CurrentMouseCoords.RelativeToSourceWindow.X + mouseInset.left;
    var y                  = CurrentMouseCoords.RelativeToSourceWindow.Y + mouseInset.top;
    var lparam             = MAKELPARAM(x, y);

    PostMessage(sourceWindow.Hwnd, (uint)Msg.WM_MOUSEMOVE, 0, lparam);

    // Forward the event to any child window that the mouse is within.
    foreach (var child in sourceWindow.Children()) {
      var childMouseInset = child.GetAbsoluteClientRect();

      var childOffsetFromParent = new Point {
        X = childMouseInset.left - sourceWindowAbsPos.left,
        Y = childMouseInset.top - sourceWindowAbsPos.top
      };

      // The mouse position needs to be in coordinates relation to the top-left corner of the child
      // window.
      var childX = CurrentMouseCoords.RelativeToSourceWindow.X +
                   childOffsetFromParent.X;
      var childY = CurrentMouseCoords.RelativeToSourceWindow.Y +
                   childOffsetFromParent.Y;

      // If the mouse is not within the child window, don't forward the event.
      if (childX < 0 ||
          childY < 0 ||
          childX > childMouseInset.right ||
          childY > childMouseInset.bottom) {
        continue;
      }

      var childLparam = MAKELPARAM(childX, childY);

      PostMessage(child.Hwnd, (uint)Msg.WM_MOUSEMOVE, 0, childLparam);
    }
  }
}
