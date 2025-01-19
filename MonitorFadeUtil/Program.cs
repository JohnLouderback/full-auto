using System.Runtime.InteropServices;
using Timer = System.Threading.Timer;

namespace MonitorFadeUtil;

public class TransparentOverlay : Form {
  private const int  GWL_EXSTYLE       = -20;
  private const int  WS_EX_LAYERED     = 0x80000;
  private const int  WS_EX_TRANSPARENT = 0x20;
  private const uint LWA_ALPHA         = 0x2;

  private static readonly int fadeOutDelay = 150; // milliseconds

  private static readonly byte
    darkestAlpha = 128; // transparency level (0=fully transparent, 255=fully opaque)

  private static TransparentOverlay[] overlays;
  private static Timer                fadeTimer;
  private static Point                lastCursorPos;
  private static Timer                fadeTimeout;
  private static TransparentOverlay   activeOverlay;


  public TransparentOverlay(Rectangle bounds) {
    FormBorderStyle = FormBorderStyle.None;
    BackColor       = Color.Black;
    Opacity         = 1.0;
    Bounds          = bounds;
    TopMost         = true;
    ShowInTaskbar   = false;
    StartPosition   = FormStartPosition.Manual;

    var style = GetWindowLong(Handle, GWL_EXSTYLE);
    SetWindowLong(
      Handle,
      GWL_EXSTYLE,
      new nint(style.ToInt64() | WS_EX_LAYERED | WS_EX_TRANSPARENT)
    );
    SetLayeredWindowAttributes(Handle, 0, darkestAlpha, LWA_ALPHA);
  }


  public static void Main() {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    var screens = Screen.AllScreens;
    overlays = new TransparentOverlay[screens.Length];

    for (var i = 0; i < screens.Length; i++) {
      overlays[i] = new TransparentOverlay(screens[i].Bounds);
      overlays[i].Show();
    }

    fadeTimer = new Timer(state => UpdateTransparency(), null, 0, 16); // 16ms = ~60fps

    Application.Run();
  }


  protected override void OnShown(EventArgs e) {
    base.OnShown(e);
    SetLayeredWindowAttributes(Handle, 0, darkestAlpha, LWA_ALPHA);
  }


  private static void ApplyTransparency(TransparentOverlay targetOverlay) {
    foreach (var overlay in overlays) {
      if (overlay == targetOverlay) {
        SetLayeredWindowAttributes(overlay.Handle, 0, 0, LWA_ALPHA); // Fully transparent
        activeOverlay = overlay;
      }
      else {
        SetLayeredWindowAttributes(overlay.Handle, 0, darkestAlpha, LWA_ALPHA);
      }
    }
  }


  [DllImport("user32.dll", SetLastError = true)]
  private static extern nint GetWindowLong(nint hWnd, int nIndex);


  [DllImport("user32.dll")]
  private static extern bool SetLayeredWindowAttributes(
    nint hwnd,
    uint crKey,
    byte bAlpha,
    uint dwFlags
  );


  [DllImport("user32.dll", SetLastError = true)]
  private static extern nint SetWindowLong(nint hWnd, int nIndex, nint dwNewLong);


  private static void UpdateTransparency() {
    var cursorPos = Cursor.Position;

    if (cursorPos != lastCursorPos) {
      lastCursorPos = cursorPos;

      foreach (var overlay in overlays) {
        if (overlay.Bounds.Contains(cursorPos)) {
          if (overlay != activeOverlay) {
            fadeTimeout?.Dispose();
            fadeTimeout = new Timer(
              _ => ApplyTransparency(overlay),
              null,
              fadeOutDelay,
              Timeout.Infinite
            );
          }
          else {
            fadeTimeout?.Dispose(); // Cancel the timeout if cursor returns to the active overlay
          }
        }
      }
    }
  }
}
