#include "MouseCoordsService.h"

#include <iostream>

#include "AppState.h"

void MouseCoordsService::HandleMouseMoveEvent(int x, int y) {
  const auto appState = AppState::GetInstance();

  // Get the coordinates of the downscaled window (the window for this application).
  const auto downscaledWindowCoords = PixelCoords{
    appState.GetDownscaledWindow().X(),
    appState.GetDownscaledWindow().Y()
  };

  this->AbsoluteCoords = PixelCoords(downscaledWindowCoords.x + x, downscaledWindowCoords.y + y);
  this->RelativeCoordsToSourceWindow = ScaleDownscaledCoordsToSourceCoords(PixelCoords(x, y));
  this->RelativeCoordsToDownscaledWindow = PixelCoords(x, y);

  // Log the current mouse coordinates to the console whenever the mouse moves.
  LogCurrentMouseCoords();
}

PixelCoords MouseCoordsService::ScaleDownscaledCoordsToSourceCoords(PixelCoords downscaledCoords) {
  // Scales the downscaled coordinates back to the source coordinates. As an example, if the source window is 1920x1080
  // and the downscaled window is 960x540, then the coordinates in the downscaled window (10x10) would be (20x20) in the
  // source window.
  auto sourceWindow = AppState::GetInstance().GetWindowToScale();
  auto downscaledWindow = AppState::GetInstance().GetDownscaledWindow();

  // Calculate the scale factor
  auto xScaleFactor = static_cast<float>(sourceWindow.Width()) / downscaledWindow.Width();
  auto yScaleFactor = static_cast<float>(sourceWindow.Height()) / downscaledWindow.Height();

  // Scale the coordinates
  return PixelCoords(
    static_cast<int>(downscaledCoords.X() * xScaleFactor),
    static_cast<int>(downscaledCoords.Y() * yScaleFactor)
  );
}

void MouseCoordsService::LogCurrentMouseCoords() {
  // Takes a "live" region of the console buffer and writes the mouse coordinates to it. The necessary space in the
  // buffer is allocated and overwritten with the mouse coordinates.
  // Get the handle to the standard output (console)
  auto hConsole = GetStdHandle(STD_OUTPUT_HANDLE);

  // Set the cursor position to the beginning of the line where we want to log coordinates
  COORD cursorPosition;
  cursorPosition.X = 0; // Column 0
  cursorPosition.Y = 0; // Row 1
  SetConsoleCursorPosition(hConsole, cursorPosition);

  // Overwrite the line with spaces to ensure old output is cleared
  DWORD written;
  FillConsoleOutputCharacter(hConsole, TEXT(' '), 80, cursorPosition, &written);

  // Reset the cursor position to the beginning of the line
  SetConsoleCursorPosition(hConsole, cursorPosition);

  const auto paddedAbsX =
    std::to_wstring(AbsoluteCoords.X())
    .append(6 - std::to_wstring(AbsoluteCoords.X()).length(), L' ');

  const auto paddedAbsY =
    std::to_wstring(AbsoluteCoords.Y())
    .append(6 - std::to_wstring(AbsoluteCoords.Y()).length(), L' ');

  const auto paddedRelToSourceX =
    std::to_wstring(RelativeCoordsToSourceWindow.X())
    .append(6 - std::to_wstring(RelativeCoordsToSourceWindow.X()).length(), L' ');

  const auto paddedRelToSourceY =
    std::to_wstring(RelativeCoordsToSourceWindow.Y())
    .append(6 - std::to_wstring(RelativeCoordsToSourceWindow.Y()).length(), L' ');

  const auto paddedRelToDownscaledX =
    std::to_wstring(RelativeCoordsToDownscaledWindow.X())
    .append(6 - std::to_wstring(RelativeCoordsToDownscaledWindow.X()).length(), L' ');

  const auto paddedRelToDownscaledY =
    std::to_wstring(RelativeCoordsToDownscaledWindow.Y())
    .append(6 - std::to_wstring(RelativeCoordsToDownscaledWindow.Y()).length(), L' ');

  // Output the current coordinates
  std::wcout
    << L"Absolute:               X=" << paddedAbsX << L"| Y=" << paddedAbsY
    << L"\nRelative to Source:     X=" << paddedRelToSourceX << L"| Y=" << paddedRelToSourceY
    << L"\nRelative to Downscaled: X=" << paddedRelToDownscaledX << L"| Y=" << paddedRelToDownscaledY
    << std::flush;

  // If you want the cursor to move to the next line after logging
  std::wcout << std::endl;
}
