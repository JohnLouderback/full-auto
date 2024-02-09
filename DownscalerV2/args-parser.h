#pragma once
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
 * @brief Ensures that the application has a console to write to. If the application is not attached to a console, it will
 *        allocate a new console and redirect the standard input, output, and error streams to the console.
 */
inline void EnsureConsole() {
  // Try to attach to an existing console
  if (!AttachConsole(ATTACH_PARENT_PROCESS)) {
    // If attaching fails, no console is present, so allocate a new one
    AllocConsole();
  }

  // Redirect unbuffered STDOUT to the console
  freopen_s((FILE**)stdout, "CONOUT$", "w", stdout);

  // Redirect unbuffered STDIN to the console
  freopen_s((FILE**)stdin, "CONIN$", "r", stdin);

  // Redirect unbuffered STDERR to the console
  freopen_s((FILE**)stderr, "CONOUT$", "w", stderr);

  // Clear the error state for each of the C++ standard streams
  std::cout.clear();
  std::cin.clear();
  std::cerr.clear();
  std::clog.clear();

  // Sync C++ standard streams with their C counterparts
  std::ios::sync_with_stdio();
}

/**
 * @brief Provided the arguments passed to the application will store the arguments in the AppState.
 * @param argc The number of arguments passed to the application.
 * @param argv The arguments passed to the application.
 */
inline void ArgParser(int argc, char* argv[]) {
  // The default "fallback" values for the application state.
  constexpr int defaultWindowWidth     = 1920;
  constexpr int defaultWindowHeight    = 1080;
  constexpr int defaultDownscaleFactor = -1; // Reduce the window size in half.
  constexpr int defaultDownscaleWidth  = -1;
  constexpr int defaultDownscaleHeight = -1;
  constexpr auto defaultAspectRatio    = AspectRatio::Maintain;

  // Parse the arguments using the TCLAP library.
  try {
    TCLAP::CmdLine cmd("Downscaler", ' ', "0.1");

    // The first positional argument is the title of the window or name of the process to mirror.
    TCLAP::UnlabeledValueArg<std::string> appArg(
      "app",
      "Title of the window or name of the process to mirror. Titles are case-sensitive, and process names are not.",
      true,
      "",
      "string"
    );

    // The second positional argument is optional and specifies the class name of the window to mirror. This is useful
    // for filtering down to the correct window when a given window title or process would yield multiple windows.
    TCLAP::UnlabeledValueArg<std::string> classArg(
      "class",
      "Class name of the window to mirror. You can use a tool like \"Spy++\", \"Window Detective\", or similar to find the class name of a window. Class names are case-sensitive.",
      false,
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

    // The positional argument must be added to the command line parser first and in order.
    TCLAP::SwitchArg ensureConsoleArg(
      "",
      "ensure-console",
      "Ensure that a console for logging output always exists.",
      false
    );

    // The positional argument must be added to the command line parser first and in order.
    cmd.add(appArg);
    cmd.add(classArg);

    // Named arguments can be added in any order.
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
    const auto parsedClass = classArg.isSet() ? std::make_optional(StringToWString(classArg.getValue())) : std::nullopt;
    const auto parsedEnsureConsole = ensureConsoleArg.getValue();

    // If the ensure console argument was provided, ensure that a console exists.
    if (parsedEnsureConsole) {
      EnsureConsole();
    }

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
        const auto windowByTitle = GetWindowForWindowTitle(StringToWString(appArg.getValue()), parsedClass);
        if (windowByTitle.has_value())
          appState.SetWindowToScale(windowByTitle.value());
        else if (parsedClass.has_value())
          FatalError(
            std::format(
              R"(No window found for the given title and class name. Title: "{}", class name: "{}")",
              appArg.getValue(),
              classArg.getValue()
            )
          );
        else
          FatalError(std::format("No window found for the given title. Title: \"{}\"", appArg.getValue()));
        break;
      }
      case WindowSearchType::ProcessName: {
        const auto windowByProcessName = GetWindowForProcessName(StringToWString(appArg.getValue()), parsedClass);
        if (windowByProcessName.has_value())
          appState.SetWindowToScale(windowByProcessName.value());
        else if (parsedClass.has_value())
          FatalError(
            std::format(
              R"(No window found for the given process name and class name. Process name: "{}", class name: "{}")",
              appArg.getValue(),
              classArg.getValue()
            )
          );
        else
          FatalError(
            std::format("No window found for the given process name. Process name: \"{}\"", appArg.getValue())
          );
        break;
      }
    }

    // Log the selected window to scale for debugging purposes.
    const auto selectedSourceWindow = appState.GetWindowToScale();

    std::wcout << L"Selected window to scale with process name: \"" << selectedSourceWindow.ProcessName()
      << L"\", title: \"" << selectedSourceWindow.Title() << L"\", class name: \"" << selectedSourceWindow.ClassName()
      << L"\", width: \"" << selectedSourceWindow.Width() << L"\", height: \"" << selectedSourceWindow.Height()
      << L"\"" << std::endl;

    // Focus the source window.
    selectedSourceWindow.Focus();

    auto hOwner = GetWindow(selectedSourceWindow.Hwnd(), GW_OWNER);
    if (hOwner != nullptr) {
      SetWindowLongPtr(selectedSourceWindow.Hwnd(), GWLP_HWNDPARENT, NULL);

      // Update the window's position to reflect the change in ownership
      SetWindowPos(
        selectedSourceWindow.Hwnd(),
        nullptr,
        0,
        0,
        0,
        0,
        SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED | SWP_NOACTIVATE
      );
    }
  }
  catch (TCLAP::ArgException& e) {
    std::cerr << "error: " << e.error() << " for arg " << e.argId() << std::endl;
    exit(1);
  }
}
