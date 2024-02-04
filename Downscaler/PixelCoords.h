#pragma once

/**
 * @brief The PixelCoords class represents a coordinate somewhere on the screen in pixels.
 */
struct PixelCoords {
  public:
    PixelCoords(int x, int y) : x(x), y(y) {}
    int x;
    int y;
    [[nodiscard]] int X() const { return this->x; }
    [[nodiscard]] int Y() const { return this->y; }
};
