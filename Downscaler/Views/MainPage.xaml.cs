using Windows.Graphics.Capture;
using Downscaler.Helpers.Graphics;
using Downscaler.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Downscaler.Views;

public sealed partial class MainPage : Page {
  private readonly GraphicsCapturePicker picker = new();
  private          SimpleCapturer?       simpleCapturer;

  public MainViewModel ViewModel { get; }


  public MainPage() {
    ViewModel = App.GetService<MainViewModel>();
    InitializeComponent();
  }


  private void MainPage_OnLoaded(object sender, RoutedEventArgs e) {
    ViewModel.StartCapture(SwapChainPanel, DispatcherQueue);
  }
}
