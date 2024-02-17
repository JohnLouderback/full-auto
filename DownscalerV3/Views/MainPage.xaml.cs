using Windows.Graphics.Capture;
using DownscalerV3.Helpers.Graphics;
using DownscalerV3.Utils;
using DownscalerV3.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DownscalerV3.Views;

public sealed partial class MainPage : Page {
  private readonly GraphicsCapturePicker picker = new();
  private SimpleCapturer? simpleCapturer;

  public MainViewModel ViewModel { get; }


  public MainPage() {
    ViewModel = App.GetService<MainViewModel>();
    InitializeComponent();
    FpsContainer.RegisterPropertyChangedCallback(VisibilityProperty, Fps_VisibilityChanged);
  }


  private async void DebugButton_OnClick(object sender, RoutedEventArgs e) {
    // First open the debug sub menu.
    DebugSettingsFlyout.IsOpen = true;
    // Then, focus the first item in the sub menu.
    DebugSettingsFlyout.Focus(FocusState.Programmatic);

    // var mainWindow = App.MainWindow;
    // var hwnd       = WindowNative.GetWindowHandle(mainWindow);
    // var picker     = new GraphicsCapturePicker();
    // InitializeWithWindow.Initialize(picker, hwnd);
    // var item = await picker.PickSingleItemAsync();
    //
    // if (item == null) {
    //   var dialog = new MessageDialog("No item was selected.");
    //   await dialog.ShowAsync();
    //   return;
    // }
    //
    // simpleCapturer = new SimpleCapturer(item, SwapChainPanel);
    // simpleCapturer.FrameRateChanged += (newFrameRate, newFrameTime) => {
    //   DispatcherQueue.TryEnqueue(
    //     () => {
    //       ViewModel.FrameRate = newFrameRate;
    //       ViewModel.FrameTime = newFrameTime;
    //       UpdatePositions();
    //     }
    //   );
    // };
    // simpleCapturer.StartCapture();
  }


  private void Fps_OnLoaded(object sender, RoutedEventArgs e) {
    UpdatePositions();
  }


  private void Fps_VisibilityChanged(DependencyObject sender, DependencyProperty dp) {
    // If the FPS text is visible, then update the positions.
    if (((UIElement)sender).Visibility == Visibility.Visible) {
      // We need to wait until after the layout has been updated before we can get the correct
      // positions. So, we'll use the dispatcher queue to set a timeout for 5ms.
      DispatcherQueue.SetTimeout(
          () => { UpdatePositions(); },
          5
        );
    }
  }


  private void MainPage_OnSizeChanged(object sender, SizeChangedEventArgs e) {
    UpdatePositions();
  }


  private void UpdatePositions() {
    // Get the widths for each element so we can calculate the correct positions.
    var fpsWidth       = Fps.ActualWidth;
    var frameTimeWidth = FrameTime.ActualWidth;

    // Get whichever was the largest width.
    var largestWidth = Math.Max(fpsWidth, frameTimeWidth);

    var canvasWidth = Canvas.ActualWidth;

    // Move the FPS text to right edge with 5 pixels padding.
    Canvas.SetLeft(FpsContainer, canvasWidth - largestWidth - 5);

    // Set the fps container width to the largest width.
    FpsContainer.Width = largestWidth;
  }
}
