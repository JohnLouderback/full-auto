#pragma once
#include "AppState.h"
#include "MouseCoordsService.h"

/**
 * @brief The MouseEventService class is responsible for handling mouse events and forwarding them to the source window.
 */
class MouseEventService {
  public:
    /**
     * @brief Retrieves the singleton instance of the MouseEventService.
     * @returns The singleton instance of the MouseEventService.
     */
    static MouseEventService& GetInstance() {
      static MouseEventService instance;
      return instance;
    }

    /**
     * @brief Handles a mouse movement event and updates the mouse coordinates.
     */
    void HandleMouseMoveEvent(int x, int y, WPARAM wparam);

    /**
     * @brief Simply accepts a mouse event, scales the coordinates, and forwards them to the source window.
     */
    void ForwardScaledMouseEvent(UINT eventName, LPARAM lparam, WPARAM wparam);

  private:
    MouseEventService() = default;
    AppState appState = AppState::GetInstance();
    MouseCoordsService mouseCoordsService = MouseCoordsService::GetInstance();

    /**
     * @brief Forwards a mouse event to any child of the source window that is at the scaled mouse coordinates.
     */
    void ForwardScaledMouseEventForChildrenAtMouseCoords(
      Window parentWindow,
      UINT eventName,
      LPARAM lparam,
      WPARAM wparam
    );
};
