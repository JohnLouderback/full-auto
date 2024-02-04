#pragma once

#include <tclap/CmdLine.h>
#include <tclap/ValueArg.h>
#include <tclap/SwitchArg.h>
#include <format>

#include "AppState.h"
#include "general-utils.h"
#include "string-utils.h"
#include "window-utils.h"

/**
 * @brief Provided the arguments passed to the application will store the arguments in the AppState.
 * @param argc The number of arguments passed to the application.
 * @param argv The arguments passed to the application.
 */
inline void ArgParser(int argc, char* argv[]) {
  // The default "fallback" values for the application state.
  constexpr int defaultWindowWidth = 1920;
  constexpr int defaultWindowHeight = 1080;
  constexpr int defaultDownscaleFactor = -1; // Reduce the window size in half.
  constexpr int defaultDownscaleWidth = -1;
  constexpr int defaultDownscaleHeight = -1;
  constexpr auto defaultAspectRatio = AspectRatio::Maintain;

  // Parse the arguments using the TCLAP library.
  try {
    TCLAP::CmdLine cmd("Downscaler", ' ', "0.1");

    // The first positional argument is the title of the window or name of the process to mirror.
    TCLAP::UnlabeledValueArg<std::string> appArg(
      "app",
      "Title of the window or name of the process to mirror",
      true,
      "",
      "string"
    );

    TCLAP::ValueArg<int> widthArg("", "width", "Width of the window", false, defaultWindowWidth, "int");
    TCLAP::ValueArg<int> heightArg("", "height", "Height of the window", false, defaultWindowHeight, "int");

    // Scale may be specified as a factor or as a specific width or height, but not any of these combined.
    TCLAP::OneOf scale;
    TCLAP::ValueArg<int> factorArg(
      "f",
      "factor",
      "Downscale factor of the window",
      false,
      defaultDownscaleFactor,
      "int"
    );
    TCLAP::ValueArg<int> downscaleWidthArg(
      "W",
      "scaleWidth",
      "Width of the downscale window",
      false,
      defaultDownscaleWidth,
      "int"
    );
    TCLAP::ValueArg<int> downscaleHeightArg(
      "H",
      "scaleHeight",
      "Height of the downscale window",
      false,
      defaultDownscaleHeight,
      "int"
    );

    // Add the downscale arguments to the scale group.
    scale.add(factorArg);
    scale.add(downscaleWidthArg);
    scale.add(downscaleHeightArg);

    // Either maintain or stretch the aspect ratio of the window. If neither are provided, maintain the aspect ratio.
    TCLAP::EitherOf aspectRatio;
    TCLAP::SwitchArg maintainArg("m", "maintain", "Maintain the aspect ratio of the window", false);
    TCLAP::SwitchArg stretchArg("s", "stretch", "Stretch the aspect ratio of the window", false);

    aspectRatio.add(maintainArg);
    aspectRatio.add(stretchArg);

    // The positional argument must be added to the command line parser first.
    cmd.add(appArg);
    cmd.add(widthArg);
    cmd.add(heightArg);
    cmd.add(scale);
    cmd.add(aspectRatio);

    cmd.parse(argc, argv);

    // Determine the correct values based on the parsed arguments and the default values.
    const auto parsedWidth = widthArg.getValue();
    const auto parsedHeight = heightArg.getValue();
    const auto parsedFactor = factorArg.getValue();
    const auto parsedDownscaleWidth = downscaleWidthArg.getValue();
    const auto parsedDownscaleHeight = downscaleHeightArg.getValue();
    const auto parsedAspectRatio = maintainArg.getValue() ? AspectRatio::Maintain : AspectRatio::Stretch;

    // Set the application state based on the parsed arguments.
    auto& appState = AppState::GetInstance();

    appState.SetWindowWidth(parsedWidth);
    appState.SetWindowHeight(parsedHeight);
    appState.SetDownscaleWidth(parsedDownscaleWidth);
    appState.SetDownscaleHeight(parsedDownscaleHeight);
    appState.SetDownscaleFactor(parsedFactor);
    appState.SetAspectRatio(parsedAspectRatio);

    // Determine the window to scale based on the provided argument. We run a heuristic to determine if the argument
    // is a title or a process name.
    switch (IsStringTitleOrProcessName(StringToWString(appArg.getValue()))) {
      case WindowSearchType::Title: {
        const auto windowByTitle = GetWindowForWindowTitle(StringToWString(appArg.getValue()));
        if (windowByTitle.has_value())
          appState.SetWindowToScale(windowByTitle.value());
        else
          FatalError(std::format("No window found for the given title. Title: \"{}\"", appArg.getValue()));
        break;
      }
      case WindowSearchType::ProcessName: {
        const auto windowByProcessName = GetWindowForProcessName(StringToWString(appArg.getValue()));
        if (windowByProcessName.has_value())
          appState.SetWindowToScale(windowByProcessName.value());
        else
          FatalError(
            std::format("No window found for the given process name. Process name: \"{}\"", appArg.getValue())
          );
        break;
      }
    }
  }
  catch (TCLAP::ArgException& e) {
    std::cerr << "error: " << e.error() << " for arg " << e.argId() << std::endl;
    exit(1);
  }
}
