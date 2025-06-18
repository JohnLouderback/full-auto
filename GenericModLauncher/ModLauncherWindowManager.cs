using System.Windows;

namespace GenericModLauncher;

public static class ModLauncherWindowManager {
  /// <summary>
  ///   Initializes the WPF application for the mod launcher. All window instances will belong to
  ///   this application.
  /// </summary>
  private static void InitializeWpfApplication() {
    // Ensure the WPF application is initialized only once.
    if (Application.Current == null) {
      var app = new Application();
      app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
      app.StartupUri   = new Uri("ModLauncher.xaml", UriKind.Relative);
    }
  }
}
