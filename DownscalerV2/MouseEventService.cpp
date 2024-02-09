#include "MouseEventService.h"

#include <windowsx.h>

#include "AppState.h"
#include "MouseCoordsService.h"

void MouseEventService::HandleMouseMoveEvent(int x, int y, WPARAM wparam) {
  // First allow the MouseCoordsService to handle the event and update the mouse coordinates.
  mouseCoordsService.HandleMouseMoveEvent(x, y);

  const auto sourceWindow = appState.GetWindowToScale();
  const auto scaledCoords = mouseCoordsService.GetRelativeCoordsToSourceWindow();
  const auto sourceWindowHandle = sourceWindow.Hwnd();

  // Next, take the scaled coordinates and forward them to the source window.
  // This is done by sending a message to the source window with the coordinates.
  // Convert client coordinates to screen coordinates since WM_MOUSEMOVE expects screen coordinates.

  // WPARAM is set to indicate which, if any, virtual keys are down.
  const WPARAM wParam = wparam; // Forward from the source window.
  // LPARAM is set to the mouse coordinates.
  const LPARAM lParam = MAKELPARAM(scaledCoords.x, scaledCoords.y);

  // Send the WM_MOUSEMOVE message
  SendMessage(sourceWindowHandle, WM_MOUSEMOVE, wParam, lParam);
}

void MouseEventService::ForwardScaledMouseEvent(UINT eventName, LPARAM lparam, WPARAM wparam) {
  const auto sourceWindow = appState.GetWindowToScale();
  const auto scaledCoords = mouseCoordsService.GetRelativeCoordsToSourceWindow();
  const auto sourceWindowHandle = sourceWindow.Hwnd();

  // LPARAM is set to the scaled mouse coordinates.
  const LPARAM lParam = MAKELPARAM(scaledCoords.x, scaledCoords.y);

  // If the source window does not have focus, focus it.
  if (!sourceWindow.HasFocus())
    sourceWindow.Focus();

  // Send the same event message back to the source window.
  SendMessage(sourceWindowHandle, eventName, wparam, lParam);

  // Attempt to forward the event to any children of the source window that are at the scaled mouse coordinates.
  ForwardScaledMouseEventForChildrenAtMouseCoords(sourceWindow, eventName, lparam, wparam);
}

void MouseEventService::ForwardScaledMouseEventForChildrenAtMouseCoords(
  Window parentWindow,
  UINT eventName,
  LPARAM lparam,
  WPARAM wparam
) {
  const auto scaledCoords = mouseCoordsService.GetRelativeCoordsToSourceWindow();
  const auto sourceWindow = appState.GetWindowToScale();
  const auto sourceWindowClientAbsCoords = sourceWindow.GetAbsoluteClientRect();

  for (auto child : parentWindow.Children()) {
    // Convert the mouse coordinates to the child window's client coordinates.
    const auto childWindowHandle = child.Hwnd();
    const auto childAbsClientRect = child.GetAbsoluteClientRect();
    const auto childClientRect = child.GetClientRectRelativeToWindow();

    // Get the offset of the child window's client area relative to the parent window's client area.
    const auto childXOffset = childAbsClientRect.left - sourceWindowClientAbsCoords.left;
    const auto childYOffset = childAbsClientRect.top - sourceWindowClientAbsCoords.top;

    // Offset the mouse event coordinates by the child window's offset from its top-level parent.
    PixelCoords coordsOffsetForChild = {
      scaledCoords.x - childXOffset,
      scaledCoords.y - childYOffset
    };

    // LPARAM is set to the scaled mouse coordinates.
    const LPARAM lParam = MAKELPARAM(coordsOffsetForChild.x, coordsOffsetForChild.y);


    // If the mouse coordinates are within the child window's client area, forward the event to the child window.
    if (PtInRect(&childClientRect, {scaledCoords.x, scaledCoords.y})) {
      // Send the event message to the child window.
      SendMessage(childWindowHandle, eventName, wparam, lParam);
    }

    // Forward the event to any children of the child window.
    ForwardScaledMouseEventForChildrenAtMouseCoords(child, eventName, lparam, wparam);
  }
}
