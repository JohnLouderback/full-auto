using Windows.Graphics;
using Windows.UI.ViewManagement;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using DownscalerV3.Contracts.Services;
using DownscalerV3.Core.Contracts.Models;
using DownscalerV3.Core.Models;
using DownscalerV3.Core.Utils;
using DownscalerV3.Helpers;
using DownscalerV3.ViewModels;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace DownscalerV3;

public sealed partial class MainWindow : WindowEx {
  private readonly DispatcherQueue dispatcherQueue;

  private readonly UISettings settings;

  public MainViewModel ViewModel { get; } = App.GetService<MainViewModel>();

  public IWindowEventHandlerService WindowEventHandlerService { get; } =
    App.GetService<IWindowEventHandlerService>();

  public IAppState AppState { get; } = App.GetService<IAppState>();


  public MainWindow() {
    InitializeComponent();

    AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
    Content = null;
    Title   = "AppDisplayName".GetLocalized();

    // Theme change code picked from https://github.com/microsoft/WinUI-Gallery/pull/1239
    dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    settings        = new UISettings();
    settings.ColorValuesChanged +=
      Settings_ColorValuesChanged; // cannot use FrameworkElement.ActualThemeChanged event

    // Set the app state's downscale window to the main window.
    var hwnd = new HWND(this.GetWindowHandle());

    AppState.DownscaleWindow = new Win32Window {
      Hwnd        = hwnd,
      ClassName   = hwnd.GetClassName(),
      ProcessName = hwnd.GetProcessName(),
      Title       = hwnd.GetWindowText()
    };

    // Set the window styles for the main window.
    hwnd.SetWindowStyle(WINDOW_STYLE.WS_OVERLAPPED);

    // Set the window's initial size.
    AppWindow.Resize(new SizeInt32((int)AppState.WindowWidth, (int)AppState.WindowHeight));

    // Initialize the window event handler service with this window's handle.
    WindowEventHandlerService.InitializeForWindow(new HWND(this.GetWindowHandle()));
  }


  // this handles updating the caption button colors correctly when indows system theme is changed
  // while the app is open
  private void Settings_ColorValuesChanged(UISettings sender, object args) {
    // This calls comes off-thread, hence we will need to dispatch it to current app's thread
    dispatcherQueue.TryEnqueue(
      () => { TitleBarHelper.ApplySystemThemeToCaptionButtons(); }
    );
  }
}
