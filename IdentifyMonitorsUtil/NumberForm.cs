namespace IdentifyMonitorsUtil;

public partial class NumberForm : Form {
  public NumberForm() {
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
  }


  protected override void OnPaintBackground(PaintEventArgs e) {
    //empty implementation
  }
}
