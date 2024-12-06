namespace IdentifyMonitorsUtil;

internal static class Program {
  [STAThread]
  private static void Main() {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

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

    var monitorList = new MonitorList();
    monitorList.Show();

    // Start the application
    Application.Run();
  }
}
