using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using Windows.Win32.Foundation;
using DownscalerV3.Contracts.Services;
using DownscalerV3.Core.Utils;
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
    CreateCaptureItemForWindow(window);
  }


  private GraphicsCaptureItem CreateCaptureItemForWindow(HWND window) {
    var captureItemPtr = window.CreateCaptureItem();

    if (captureItemPtr == nint.Zero) {
      throw new InvalidOperationException("Failed to create capture item.");
    }

    // Marshal the COM pointer into a .NET object.
    // This is not a recommended practice for WinRT objects and could fail.
    var captureItem = Marshal.GetObjectForIUnknown(captureItemPtr);

    // Attempt to cast the object to the desired WinRT type.
    var graphicsCaptureItem = captureItem as GraphicsCaptureItem;

    if (graphicsCaptureItem == null) {
      throw new InvalidCastException("Failed to cast to GraphicsCaptureItem.");
    }

    return graphicsCaptureItem;
  }
}
