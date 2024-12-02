using System.Runtime.InteropServices;

namespace IdentifyMonitorsUtil;

public partial class NumberForm : Form {
  private const int GWL_EXSTYLE = -20;
  private const int WS_EX_LAYERED = 0x80000;
  private const int WS_EX_TRANSPARENT = 0x20;


  public NumberForm(MonitorDetails monitorDetails) {
    InitializeComponent();
    SetStyle(ControlStyles.SupportsTransparentBackColor, true);
    BackColor       = Color.Black;
    TransparencyKey = Color.Black; // I had to add this to get it to work.
    FormBorderStyle = FormBorderStyle.None; // No borders
    WindowState     = FormWindowState.Maximized; // Fullscreen on each monitor
    StartPosition   = FormStartPosition.Manual;
    Opacity         = 0.8; // Nearly fully transparent
    ShowInTaskbar   = false;
    TopMost         = true;

    Shown += (sender, args) => {
      monitorIndex.Text = monitorDetails.Index.ToString();
      deviceID.Text     = "Device ID: " + monitorDetails.DeviceName;
      // Make the form click-through
      var exStyle = GetWindowLong(Handle, GWL_EXSTYLE);
      SetWindowLong(Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT);
    };
  }


  [DllImport("user32.dll", SetLastError = true)]
  private static extern int GetWindowLong(nint hWnd, int nIndex);


  // Import SetWindowLong from the user32 library to enable click-through
  [DllImport("user32.dll", SetLastError = true)]
  private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);


  // protected override void OnPaintBackground(PaintEventArgs e) {
  //   //empty implementation
  // }

  private void label1_Click(object sender, EventArgs e) {}

  private void label2_Click(object sender, EventArgs e) {}
}
