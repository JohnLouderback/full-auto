using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Core.Utils;

namespace IdentifyMonitorsUtil;

public partial class NumberForm : Form {
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

      // Make the form "click-through" by setting the WS_EX_LAYERED and WS_EX_TRANSPARENT styles.
      var hwnd    = (HWND)Handle;
      var exStyle = hwnd.GetWindowExStyle();
      hwnd.SetWindowExStyle(
        exStyle | WINDOW_EX_STYLE.WS_EX_LAYERED | WINDOW_EX_STYLE.WS_EX_TRANSPARENT
      );
    };
  }


  // protected override void OnPaintBackground(PaintEventArgs e) {
  //   //empty implementation
  // }

  private void label1_Click(object sender, EventArgs e) {}

  private void label2_Click(object sender, EventArgs e) {}
}
