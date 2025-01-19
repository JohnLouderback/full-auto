using System.Runtime.InteropServices;
using Microsoft.Win32;
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

  private static List<TransparentOverlay> overlays;
  private static Timer                    fadeTimer;
  private static Point                    lastCursorPos;
  private static Timer                    fadeTimeout;
  private static TransparentOverlay       activeOverlay;

  // Store transparency state for each overlay
  private static readonly Dictionary<Rectangle, byte> overlayStates = new();

  // Flag to track if the initial alpha value was passed to the constructor. This disables things
  // like fading in the overlay when it's first shown.
  private readonly bool initialAlphaWasPassed;

  /// <summary>
  ///   The unique identifier for the overlay based on the monitor it represents.
  /// </summary>
  public string ID { get; }


  public TransparentOverlay(string id, Rectangle bounds, byte? initialAlpha = null) {
    ID              = id;
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

    if (initialAlpha != null) {
      initialAlphaWasPassed = true;
    }

    // If an initial alpha value is provided, use it; otherwise, set the initial alpha based on
    // the cursor position.
    var alpha = initialAlpha ?? (Bounds.Contains(Cursor.Position) ? (byte)0 : darkestAlpha);

    SetLayeredWindowAttributes(Handle, 0, alpha, LWA_ALPHA);

    if (initialAlpha == 0) {
      activeOverlay = this;
    }

    overlayStates[Bounds] = alpha; // Save initial state
  }


  public static void Main() {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    CreateOverlays();

    // Monitor for display settings changes.
    SystemEvents.DisplaySettingsChanged += (sender, args) => RecreateOverlays();

    // We track the cursor position to determine when to fade the overlays in/out.
    fadeTimer = new Timer(state => UpdateTransparency(), null, 0, 16); // 16ms = ~60fps

    Application.Run();
  }


  protected override void OnShown(EventArgs e) {
    base.OnShown(e);
    // If an initial alpha value was not passed to the constructor, set the initial alpha based on
    // the cursor position. Play a fade-in animation if the cursor is not over the overlay.
    if (!initialAlphaWasPassed) {
      if (!Bounds.Contains(Cursor.Position)) {
        // Tween to darkened transparency
        TweenAlpha(this, 0, darkestAlpha);
      }
      else {
        SetLayeredWindowAttributes(Handle, 0, 0, LWA_ALPHA);
        activeOverlay = this;
      }
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
          overlay.Invoke(() => SetLayeredWindowAttributes(overlay.Handle, 0, 0, LWA_ALPHA));
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


  private static void CreateOverlays() {
    var screens = Screen.AllScreens;
    overlays = new List<TransparentOverlay>(screens.Length);

    // For each screen, create a transparent overlay.
    for (var i = 0; i < screens.Length; i++) {
      var bounds  = screens[i].Bounds;
      var id      = screens[i].DeviceName;
      var overlay = new TransparentOverlay(id, bounds);
      overlays.Add(overlay);
      overlay.Show();
    }
  }


  private static byte GetCurrentAlpha(TransparentOverlay overlay) {
    // Placeholder for logic to retrieve the current alpha value of the overlay.
    // This value should ideally be cached or tracked when alpha changes occur.
    return overlayStates.TryGetValue(overlay.Bounds, out var alpha) ? alpha : darkestAlpha;
  }


  [DllImport("user32.dll", SetLastError = true)]
  private static extern nint GetWindowLong(nint hWnd, int nIndex);


  private static void RecreateOverlays() {
    var screens = Screen.AllScreens;

    // Create a dictionary of overlays by ID for quick lookup.
    var overlaysByID = overlays.ToDictionary(overlay => overlay.ID);

    // Create a list of all overlays that will correspond to a screen, so we can remove any that
    // are no longer needed.
    var mostRecentOverlays = new List<TransparentOverlay>(screens.Length);

    for (var i = 0; i < screens.Length; i++) {
      // If the overlay already exists, update its bounds.
      if (overlaysByID.TryGetValue(screens[i].DeviceName, out var overlay)) {
        overlay.Bounds = screens[i].Bounds;
        mostRecentOverlays.Add(overlay);
      }
      else {
        // Otherwise, create a new overlay.
        var bounds = screens[i].Bounds;
        var id     = screens[i].DeviceName;
        overlay = new TransparentOverlay(id, bounds);
        overlay.Show();
        overlays.Add(overlay);
        mostRecentOverlays.Add(overlay);
      }
    }

    // Finally, remove any overlays that are no longer needed.
    foreach (var overlay in overlays.ToList()) {
      if (!mostRecentOverlays.Contains(overlay)) {
        overlay.Close();
        overlays.Remove(overlay);
      }
    }
  }


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
    var stepCount    = 30; // Number of steps for the tween
    var stepDuration = tweenDuration / stepCount; // Duration per step in milliseconds
    var stepSize     = (endAlpha - startAlpha) / (float)stepCount;

    var   currentStep = 0;
    Timer tweenTimer  = null;
    tweenTimer = new Timer(
      _ => {
        if (currentStep >= stepCount) {
          overlay.Invoke(() => SetLayeredWindowAttributes(overlay.Handle, 0, endAlpha, LWA_ALPHA));
          overlayStates[overlay.Bounds] = endAlpha; // Save final alpha state
          tweenTimer.Dispose();
          return;
        }

        var currentAlpha = (byte)(startAlpha + stepSize * currentStep);
        overlay.Invoke(
          () => SetLayeredWindowAttributes(overlay.Handle, 0, currentAlpha, LWA_ALPHA)
        );
        overlayStates[overlay.Bounds] = currentAlpha; // Update alpha state during tween
        currentStep++;
      },
      null,
      0,
      stepDuration
    );
  }


  private static void UpdateTransparency() {
    var cursorPos = Cursor.Position;

    if (cursorPos == lastCursorPos) return;

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
          fadeTimeout?.Dispose();
        }
      }
    }
  }
}
