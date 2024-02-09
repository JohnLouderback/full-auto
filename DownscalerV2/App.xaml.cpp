#include "pch.h"
#include "App.xaml.h"
#include "MainWindow.xaml.h"
#include "args-parser.h"

using namespace winrt;
using namespace Microsoft::UI::Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

/**
 * @brief Converts the wide character command line arguments to multi-byte character command line arguments.
 * @param argc The number of arguments in the array.
 * @param wargv The array of wide character arguments to convert.
 * @returns The converted array of multi-byte character arguments.
 */
char** ConvertWideCharToMultiByte(int argc, wchar_t** wargv) {
  if (argc == 0 || wargv == nullptr) {
    return nullptr;
  }

  // Allocate memory for the array of char* (C-style strings)
  auto argv = new char*[argc];

  for (int i = 0; i < argc; ++i) {
    // Calculate the length needed for the converted string
    int length = WideCharToMultiByte(CP_UTF8, 0, wargv[i], -1, nullptr, 0, nullptr, nullptr);

    // Allocate memory for the converted string
    argv[i] = new char[length];

    // Perform the actual conversion
    WideCharToMultiByte(CP_UTF8, 0, wargv[i], -1, argv[i], length, nullptr, nullptr);
  }

  return argv;
}

/**
 * @brief Frees the memory allocated for the converted argv array.
 * @param argc The number of arguments in the array.
 * @param argv The array of arguments to free.
 */
void FreeConvertedArgv(int argc, char** argv) {
  if (argv != nullptr) {
    for (int i = 0; i < argc; ++i) {
      delete[] argv[i]; // Free each string
    }
    delete[] argv; // Free the array
  }
}

namespace winrt::DownscalerV2::implementation {
  /// <summary>
  /// Initializes the singleton application object.  This is the first line of authored code
  /// executed, and as such is the logical equivalent of main() or WinMain().
  /// </summary>
  App::App() {
    // Xaml objects should not call InitializeComponent during construction.
    // See https://github.com/microsoft/cppwinrt/tree/master/nuget#initializecomponent

    #if defined _DEBUG && !defined DISABLE_XAML_GENERATED_BREAK_ON_UNHANDLED_EXCEPTION
    UnhandledException(
      [](const IInspectable&, const UnhandledExceptionEventArgs& e) {
        if (IsDebuggerPresent()) {
          auto errorMessage = e.Message();
          __debugbreak();
        }
      }
    );
    #endif
  }

  /// <summary>
  /// Invoked when the application is launched.
  /// </summary>
  /// <param name="e">Details about the launch request and process.</param>
  void App::OnLaunched([[maybe_unused]] const LaunchActivatedEventArgs& e) {
    window = make<MainWindow>();
    window.Activate();

    int argc   = 0;
    auto wargv = CommandLineToArgvW(GetCommandLineW(), &argc);
    auto argv  = ConvertWideCharToMultiByte(argc, wargv);
    ArgParser(argc, argv);
    FreeConvertedArgv(argc, argv); // Remember to free the memory allocated for argv
    LocalFree(wargv);
  }
}
