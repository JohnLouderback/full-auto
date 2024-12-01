using System.Diagnostics;
using Downscaler.Activation;
using Downscaler.Contracts.Services;
using Downscaler.Services;
using Downscaler.ViewModels;
using Downscaler.Views;
using DownscalerV3.Contracts.Services;
using Downscaler.Core.Contracts.Models.AppState;
using Downscaler.Core.Contracts.Services;
using Downscaler.Core.Models.AppState;
using Downscaler.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;

namespace Downscaler;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application {
  // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
  // https://docs.microsoft.com/dotnet/core/extensions/generic-host
  // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
  // https://docs.microsoft.com/dotnet/core/extensions/configuration
  // https://docs.microsoft.com/dotnet/core/extensions/logging
  public IHost Host { get; }

  public static WindowEx MainWindow { get; } = new MainWindow();

  public static UIElement? AppTitlebar { get; set; }


  public App() {
    InitializeComponent();

    Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
      .UseContentRoot(AppContext.BaseDirectory)
      .ConfigureServices(
        (context, services) => {
          // Set the process priority to high.
          using (var p = Process.GetCurrentProcess()) {
            p.PriorityClass = ProcessPriorityClass.High;
          }

          // Default Activation Handler
          services
            .AddTransient<ActivationHandler<LaunchActivatedEventArgs>,
              DefaultActivationHandler>();

          // Other Activation Handlers

          // Services //
          services.AddSingleton<IActivationService, ActivationService>();
          services.AddSingleton<IPageService, PageService>();
          services.AddSingleton<INavigationService, NavigationService>();

          // Core Services //
          services.AddSingleton<IAppState, AppState>();
          services.AddSingleton<IYamlParser, YamlParser>();
          services.AddSingleton<IArgsParser, ArgsParser>();
          services.AddSingleton<IFileService, FileService>();
          services.AddSingleton<IWindowEventHandlerService, WindowEventHandlerService>();
          services.AddSingleton<IMouseEventService, MouseEventService>();
          services.AddSingleton<IWindowEventHandlerService, WindowEventHandlerService>();
          services.AddSingleton<ICaptureService, CaptureService>();

          // Views and ViewModels //
          // The MainViewModel is a singleton so that it may be accessed across multiple views.
          services.AddSingleton<MainViewModel>();
          services.AddTransient<MainPage>();

          // Configuration /
        }
      )
      .Build();

    UnhandledException += App_UnhandledException;

    // Parse the command line arguments.
    var argsParser = GetService<IArgsParser>();

    if (!argsParser.ParseArgs(Environment.GetCommandLineArgs())) {
      // If the arguments were not parsed successfully, then exit the application with a non-zero
      // exit code.
      Environment.Exit(1);
    }
  }


  public static T GetService<T>()
    where T : class {
    if ((Current as App)!.Host.Services.GetService(typeof(T)) is not T service) {
      throw new ArgumentException(
        $"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs."
      );
    }

    return service;
  }


  protected override async void OnLaunched(LaunchActivatedEventArgs args) {
    base.OnLaunched(args);

    await GetService<IActivationService>().ActivateAsync(args);
  }


  private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
    // TODO: Log and handle exceptions as appropriate.
    // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
  }
}
