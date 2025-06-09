using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Core.Utils;
using static Windows.Win32.PInvoke;
using static Core.Utils.Macros;

namespace GameLauncher.Forms;

public sealed class MatteForm : Form {
  private readonly HWND  targetWindow;
  private readonly Color matteColor;


  public MatteForm(HWND targetWindow, Color matteColor) {
    this.targetWindow = targetWindow;
    this.matteColor   = matteColor;

    InitializeForm();
    RegisterEvents();
  }


  private void CenterChildWindow() {
    // Take the provided target window and center it within the matte area.
    if (targetWindow != NULL &&
        targetWindow.IsWindow()) {
      // Center the target window within the matte form.
      GetWindowRect(targetWindow, out var targetRect);
      GetClientRect((HWND)Handle, out var matteRect);

      // The centering logic calculates the position of the target window using the matte form's
      // client area dimensions.
      var x = (matteRect.Width - targetRect.Width) / 2;
      var y = (matteRect.Height - targetRect.Height) / 2;

      // Finally, set the position of the target window to center it within the matte form.
      SetWindowPos(
        targetWindow,
        (HWND)NULL,
        x,
        y,
        cx: 0,
        cy: 0,
        SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOSIZE
      );
    }
  }


  private void InitializeForm() {
    BackColor       = matteColor;
    FormBorderStyle = FormBorderStyle.None;
    StartPosition   = FormStartPosition.Manual;
    WindowState     = FormWindowState.Maximized;
    TopMost         = true;
    ShowInTaskbar   = false;
  }


  private void OnFormClosed(object? sender, FormClosedEventArgs e) {
    // When the matte form is closed, we need to reset the target window's parent to NULL
    // so it can function normally again. Ensure that the target window is still valid
    // before attempting to set its parent.
    if (targetWindow != NULL &&
        targetWindow.IsWindow()) {
      SetParent(targetWindow, (HWND)NULL);
      SetWindowPos(
        targetWindow,
        (HWND)NULL,
        X: 0,
        Y: 0,
        cx: 0,
        cy: 0,
        SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOSIZE
      );
    }
  }


  private void OnLoad(object? sender, EventArgs e) {
    // Take the provided target window and make it a child of this matte form, centering it
    // within the matte area.
    if (targetWindow != NULL &&
        targetWindow.IsWindow()) {
      // Set the matte form as the parent of the target window.
      SetParent(targetWindow, (HWND)Handle);

      // Center the target window within the matte form.
      CenterChildWindow();
    }
  }


  private void OnResize(object? sender, EventArgs e) {
    // When the matte form is resized, we need to re-center the target window within the
    // matte area.
    CenterChildWindow();
  }


  private void RegisterEvents() {
    Load       += OnLoad;
    FormClosed += OnFormClosed;
    Resize     += OnResize;
  }
}
