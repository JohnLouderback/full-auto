#pragma once

#include <dwmapi.h>
#include <vector>

#include "macro-utils.h"
#include "Win32WindowEnumeration.h"

/**
 * @brief The AspectRatio enum represents options for either maintaining or stretching the aspect ratio of the mirrored
 *        window.
 */
enum class AspectRatio {
  Maintain,
  Stretch
};

/**
 * @brief The AppState class represents the state of the application.
 */
class AppState {
  private:
    AppState() {
      // Get all windows currently displayed when the application state is initialized.
      this->AllWindows = EnumerateWindows();
    }

    int WindowWidth = -1;
    int WindowHeight = -1;
    int DownscaleFactor = -1;
    int DownscaleWidth = -1;
    int DownscaleHeight = -1;
    AspectRatio AspectRatio = AspectRatio::Maintain;
    Window WindowToScale = nullptr;
    Window DownscaledWindow = nullptr;
    std::vector<Window> AllWindows;

  public:
    /**
     * @brief Retrieves the singleton instance of the AppState.
     * @returns The singleton instance of the AppState.
     */
    static AppState& GetInstance() {
      static AppState instance;
      return instance;
    }

    ACCESSOR(WindowWidth)
    ACCESSOR(WindowHeight)
    ACCESSOR(DownscaleFactor)
    ACCESSOR(DownscaleWidth)
    ACCESSOR(DownscaleHeight)
    ACCESSOR(AspectRatio)
    ACCESSOR(WindowToScale)
    ACCESSOR(DownscaledWindow)
    GETTER(AllWindows)
};
