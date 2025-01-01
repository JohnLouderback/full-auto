using Core.Contracts.Services;
using Core.Services;
using DryIoc;
using IdentifyMonitorsUtil.Contracts.Presenters;
using IdentifyMonitorsUtil.Contracts.Views;
using IdentifyMonitorsUtil.Presenters;
using IdentifyMonitorsUtil.Views;

namespace IdentifyMonitorsUtil;

internal static class Program {
  private static readonly Container container =
    new(rules => rules.WithTrackingDisposableTransients());


  [STAThread]
  private static void Main() {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    container.Register<IMonitorManagerService, MonitorManagerService>(Reuse.Singleton);
    container.Register<IMonitorListPresenter, MonitorListPresenter>(Reuse.ScopedOrSingleton);
    container.Register<IMonitorListView, MonitorList>(Reuse.ScopedOrSingleton);

    // Number to display
    var monitorIndex = 1;

    foreach (var screen in Screen.AllScreens) {
      // Create a new form for each screen
      var numberForm = new NumberForm(
        new MonitorDetails {
          Index      = monitorIndex,
          DeviceName = screen.DeviceName
        }
      );

      // Set form location and size for each screen
      numberForm.Location = screen.Bounds.Location;
      numberForm.Size     = screen.Bounds.Size;

      // Show the form on each screen
      numberForm.Show();
      monitorIndex++;
    }

    var monitorList = container.Resolve<IMonitorListView>();
    monitorList.Show();
    // The monitor list is considered to be the "main window", so when it's closed, the application
    // should exit.
    monitorList.Closed += (sender, args) => Application.Exit();

    // Start the application
    Application.Run();
  }
}
