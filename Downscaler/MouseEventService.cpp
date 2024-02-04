#include "MouseEventService.h"

#include "AppState.h"
#include "MouseCoordsService.h"

void MouseEventService::HandleMouseMoveEvent(int x, int y) {
  // First allow the MouseCoordsService to handle the event and update the mouse coordinates.
  auto mouseCoordsService = MouseCoordsService::GetInstance();
  mouseCoordsService.HandleMouseMoveEvent(x, y);

  const auto appState = AppState::GetInstance();
  const auto sourceWindow = appState.GetWindowToScale();
  const auto scaledCoords = mouseCoordsService.GetRelativeCoordsToSourceWindow();
  const auto sourceWindowHandle = sourceWindow.Hwnd();

  // Next, take the scaled coordinates and forward them to the source window.
  // This is done by sending a message to the source window with the coordinates.
  // Convert client coordinates to screen coordinates since WM_MOUSEMOVE expects screen coordinates.
  POINT point = {x, y};
  //ClientToScreen(sourceWindowHandle, &point);

  // WPARAM is set to indicate which, if any, virtual keys are down.
  constexpr WPARAM wParam = 0; //MK_LBUTTON; // Indicate the left mouse button is down.
  // LPARAM is set to the mouse coordinates.
  const LPARAM lParam = MAKELPARAM(point.x, point.y);

  // Send the WM_MOUSEMOVE message
  SendMessage(sourceWindowHandle, WM_MOUSEMOVE, wParam, lParam);
}
