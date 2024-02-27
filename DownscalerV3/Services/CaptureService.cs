using Windows.Win32.Foundation;
using DownscalerV3.Contracts.Services;
using Microsoft.UI.Xaml.Controls;

namespace DownscalerV3.Services;

/// <inheritdoc />
public class CaptureService : ICaptureService {
  /// <inheritdoc />
  public event EventHandler<(double newFrameRate, double newFrameTime)>? FrameRateChanged;


  /// <inheritdoc />
  public void EndCapture() {
    throw new NotImplementedException();
  }


  /// <inheritdoc />
  public void PickAndCaptureWindow(SwapChainPanel swapChainPanel) {
    throw new NotImplementedException();
  }


  /// <inheritdoc />
  public void StartCapture(HWND window, SwapChainPanel swapChainPanel) {
    throw new NotImplementedException();
  }
}
