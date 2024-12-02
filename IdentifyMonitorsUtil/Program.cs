using System.Runtime.InteropServices;

namespace IdentifyMonitorsUtil;

internal static class Program {
  private const int GWL_EXSTYLE       = -20;
  private const int WS_EX_LAYERED     = 0x80000;
  private const int WS_EX_TRANSPARENT = 0x20;


  [DllImport("user32.dll", SetLastError = true)]
  private static extern int GetWindowLong(nint hWnd, int nIndex);


  [STAThread]
  private static void Main() {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    // Number to display
    var monitorIndex = 1;

    foreach (var screen in Screen.AllScreens) {
      // Create a new form for each screen
      var numberForm = new NumberForm();

      // Set form location and size for each screen
      numberForm.Location = screen.Bounds.Location;
      numberForm.Size     = screen.Bounds.Size;

      // Make the form click-through
      var exStyle = GetWindowLong(numberForm.Handle, GWL_EXSTYLE);
      SetWindowLong(numberForm.Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);

      // Create a label to display the number
      var numberLabel = new Label {
        Text      = monitorIndex.ToString() + " " + screen.DeviceName,
        Dock      = DockStyle.Fill,
        Font      = new Font("Arial", 150, FontStyle.Bold), // Adjust font size as needed
        ForeColor = Color.White,
        TextAlign = ContentAlignment.TopLeft,
        BackColor = Color.Transparent, // Transparent label background
        Margin    = new Padding(10)
      };

      // Set the label as the only control on the form
      numberForm.Controls.Add(numberLabel);

      // Show the form on each screen
      numberForm.Show();
      monitorIndex++;
    }

    // Start the application
    Application.Run();
  }


  // Import SetWindowLong from the user32 library to enable click-through
  [DllImport("user32.dll", SetLastError = true)]
  private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);
}
