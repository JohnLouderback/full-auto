using Windows.Foundation;
using Windows.Graphics;
using Windows.UI.ViewManagement;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Downscaler.Helpers;
using Downscaler.ViewModels;
using DownscalerV3.Contracts.Services;
using DownscalerV3.Core.Contracts.Models.AppState;
using DownscalerV3.Core.Models;
using DownscalerV3.Core.Utils;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;

namespace Downscaler;

public sealed partial class MainWindow : WindowEx {
  private readonly DispatcherQueue dispatcherQueue;

  private readonly UISettings settings;

  public MainViewModel ViewModel { get; } = App.GetService<MainViewModel>();

  public IWindowEventHandlerService WindowEventHandlerService { get; } =
    App.GetService<IWindowEventHandlerService>();

  public IAppState AppState { get; } = App.GetService<IAppState>();


  public MainWindow() {
    InitializeComponent();

    AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/downscaler.ico"));
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

    if (AppState.InitialX is not null &&
        AppState.InitialY is not null) {
      AppWindow.Move(new PointInt32(AppState.InitialX.Value, AppState.InitialY.Value));
    }

    // Report the height of the window to the view model.
    ViewModel.WindowHeight = (int)AppState.WindowHeight;

    // Initialize the window event handler service with this window's handle.
    WindowEventHandlerService.InitializeForWindow(new HWND(this.GetWindowHandle()));
  }


  /// <inheritdoc />
  protected override void OnPositionChanged(PointInt32 position) {
    // Maintain the window's size across monitors.
    AppWindow.Resize(new SizeInt32((int)AppState.WindowWidth, (int)AppState.WindowHeight));
    ViewModel.WindowWidth  = (int)AppState.WindowWidth;
    ViewModel.WindowHeight = (int)AppState.WindowHeight;
    OnSizeChanged(new Size(AppState.WindowWidth, (double)AppState.WindowHeight));
    ViewModel.RaisePositionChanged();
    base.OnPositionChanged(position);
  }


  /// <inheritdoc />
  protected override bool OnSizeChanged(Size newSize) {
    // Update the view model's window width and height. We cast to int because the value of newSize
    // should be in device pixels, which are inherently integral.
    ViewModel.WindowWidth  = (int)newSize.Width;
    ViewModel.WindowHeight = (int)newSize.Height;
    return base.OnSizeChanged(newSize);
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
