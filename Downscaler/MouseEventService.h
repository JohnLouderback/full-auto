#pragma once

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
    void HandleMouseMoveEvent(int x, int y);

  private:
    MouseEventService() = default;
};
