using System.Runtime.InteropServices;
using Timer = System.Threading.Timer;

namespace MonitorFadeUtil;

public class TransparentOverlay : Form {
  private const int  GWL_EXSTYLE       = -20;
  private const int  WS_EX_LAYERED     = 0x80000;
  private const int  WS_EX_TRANSPARENT = 0x20;
  private const uint LWA_ALPHA         = 0x2;

  private static readonly int fadeOutDelay  = 150; // milliseconds
  private static readonly int tweenDuration = 150; // milliseconds for tweening

  private static readonly bool
    enableTweening = true; // Configurable option to enable/disable tweening

  private static readonly byte
    darkestAlpha = 200; // transparency level (0=fully transparent, 255=fully opaque)

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

    if (!Bounds.Contains(Cursor.Position)) {
      SetLayeredWindowAttributes(Handle, 0, darkestAlpha, LWA_ALPHA);
    }
    else {
      SetLayeredWindowAttributes(Handle, 0, 0, LWA_ALPHA);
      activeOverlay = this;
    }
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

    // We track the cursor position to determine when to fade the overlays in/out.
    fadeTimer = new Timer(state => UpdateTransparency(), null, 0, 16); // 16ms = ~60fps

    Application.Run();
  }


  protected override void OnShown(EventArgs e) {
    base.OnShown(e);
    if (!Bounds.Contains(Cursor.Position)) {
      // Tween to darkened transparency
      TweenAlpha(this, 0, darkestAlpha);
    }
    else {
      SetLayeredWindowAttributes(Handle, 0, 0, LWA_ALPHA);
      activeOverlay = this;
    }
  }


  private static void ApplyTransparency(TransparentOverlay targetOverlay) {
    foreach (var overlay in overlays) {
      if (overlay == targetOverlay) {
        if (enableTweening) {
          // Tween to full transparency
          TweenAlpha(overlay, darkestAlpha, 0);
        }
        else {
          overlay.Invoke(
            () => SetLayeredWindowAttributes(overlay.Handle, 0, 0, LWA_ALPHA)
          ); // Fully transparent
        }

        activeOverlay = overlay;
      }
      else {
        if (enableTweening) {
          // Tween to darkened transparency
          TweenAlpha(overlay, 0, darkestAlpha);
        }
        else {
          overlay.Invoke(
            () => SetLayeredWindowAttributes(overlay.Handle, 0, darkestAlpha, LWA_ALPHA)
          );
        }
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


  private static void TweenAlpha(TransparentOverlay overlay, byte startAlpha, byte endAlpha) {
    // Smoothly transition the overlay's alpha value over the tweenDuration
    var stepCount    = 30; // Number of steps for the tween
    var stepDuration = tweenDuration / stepCount; // Duration per step in milliseconds
    var stepSize     = (endAlpha - startAlpha) / (float)stepCount; // Alpha increment per step

    var   currentStep = 0;
    Timer tweenTimer  = null;
    tweenTimer = new Timer(
      _ => {
        if (currentStep >= stepCount) {
          overlay.Invoke(() => SetLayeredWindowAttributes(overlay.Handle, 0, endAlpha, LWA_ALPHA));
          tweenTimer.Dispose();
          return;
        }

        var currentAlpha = (byte)(startAlpha + stepSize * currentStep);
        overlay.Invoke(
          () => SetLayeredWindowAttributes(overlay.Handle, 0, currentAlpha, LWA_ALPHA)
        );
        currentStep++;
      },
      null,
      0,
      stepDuration
    );
  }


  private static void UpdateTransparency() {
    var cursorPos = Cursor.Position;

    // If the cursor hasn't moved, don't do anything.
    if (cursorPos == lastCursorPos) return;

    // Otherwise, store the new cursor position and check for overlays.
    lastCursorPos = cursorPos;

    foreach (var overlay in overlays) {
      // If any given overlay contains the cursor, we set a timeout to fade it in.
      if (overlay.Bounds.Contains(cursorPos)) {
        // If the cursor is not over the "active" overlay, set a timeout to fade it in.
        if (overlay != activeOverlay) {
          fadeTimeout?.Dispose();
          fadeTimeout = new Timer(
            _ => ApplyTransparency(overlay),
            null,
            fadeOutDelay,
            Timeout.Infinite
          );
        }
        // If the cursor is over the already active overlay, cancel any timeouts that may have been
        // set if the cursor previously left the overlay. For example: If the cursor quickly moved
        // from one overlay to another and back, we don't want the overlay to fade out because it
        // returned to the original overlay before the timeout expired.
        else {
          fadeTimeout?.Dispose(); // Cancel the timeout if cursor returns to the active overlay
        }
      }
    }
  }
}
