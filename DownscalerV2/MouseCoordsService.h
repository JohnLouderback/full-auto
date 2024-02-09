#pragma once
#include "PixelCoords.h"
#include "macro-utils.h"

/**
 * @brief The MouseCoordsService represents the current state of the mouse coordinates and provides methods for
 *        manipulating, recording, and logging them.
 */
class MouseCoordsService {
  private:
    MouseCoordsService() = default;
    /** The absolute coordinates of the mouse. "Absolute" means relative to the entire screen. */
    PixelCoords AbsoluteCoords = PixelCoords(-1, -1);

    /** The relative coordinates of the mouse to the source window we're mirroring. */
    PixelCoords RelativeCoordsToSourceWindow = PixelCoords(-1, -1);

    /** The relative coordinates of the mouse to the downscaled window. */
    PixelCoords RelativeCoordsToDownscaledWindow = PixelCoords(-1, -1);

  public:
    GETTER(AbsoluteCoords)
    GETTER(RelativeCoordsToSourceWindow)
    GETTER(RelativeCoordsToDownscaledWindow)

    /**
     * @brief Retrieves the singleton instance of the MouseCoordsService.
     * @returns The singleton instance of the MouseCoordsService.
     */
    static MouseCoordsService& GetInstance() {
      static MouseCoordsService instance;
      return instance;
    }

    /**
     * @brief Handles a mouse movement event and updates the mouse coordinates.
     */
    void HandleMouseMoveEvent(int x, int y);

    /**
     * @brief Scales the downscaled coordinates back to the source coordinates. As an example, if the source window is
     *        1920x1080 and the downscaled window is 960x540, then the coordinates in the downscaled window (10x10)
     *        would be (20x20) in the source window.
     * @param downscaledCoords The coordinates relative to the downscaled window to convert to the source window.
     * @returns The scaled coordinates.
     */
    PixelCoords ScaleDownscaledCoordsToSourceCoords(PixelCoords downscaledCoords);

    /**
     * @brief Logs the current mouse coordinates to the console. This takes a "live" region of the console buffer and
     *        writes the mouse coordinates to it.
     */
    void LogCurrentMouseCoords();
};
