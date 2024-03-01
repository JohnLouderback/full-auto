using Windows.Graphics.Capture;
using Windows.UI.Popups;
using Windows.Win32.Foundation;
using DownscalerV3.Contracts.Services;
using DownscalerV3.Core.Contracts.Models;
using DownscalerV3.Core.Utils;
using DownscalerV3.Helpers.Graphics;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace DownscalerV3.Services;

/// <inheritdoc />
public class CaptureService : ICaptureService {
  private readonly IAppState  AppState;
  private          ICapturer? capturer;

  /// <inheritdoc />
  public event EventHandler<(double newFrameRate, double newFrameTime)>? FrameRateChanged;


  public CaptureService(IAppState appState) {
    AppState = appState;
  }


  /// <inheritdoc />
  public void EndCapture() {
    throw new NotImplementedException();
  }


  /// <inheritdoc />
  public async Task PickAndCaptureWindow(SwapChainPanel swapChainPanel) {
    var picker = new GraphicsCapturePicker();
    var hwnd   = AppState.DownscaleWindow.Hwnd;
    InitializeWithWindow.Initialize(picker, hwnd);
    var captureItem = await picker.PickSingleItemAsync();

    if (captureItem == null) {
      var dialog = new MessageDialog("No item was selected.");
      await dialog.ShowAsync();
    }

    InitializeCapture(captureItem!, swapChainPanel);
  }


  /// <inheritdoc />
  public void StartCapture(HWND window, SwapChainPanel swapChainPanel) {
    var captureItem = CreateCaptureItemForWindow(window);
    InitializeCapture(captureItem, swapChainPanel);
  }


  /// <inheritdoc />
  public void StartCapture(SwapChainPanel swapChainPanel) {
    StartCapture(AppState.WindowToScale.Hwnd, swapChainPanel);
  }


  /// <summary>
  ///   Creates a <see cref="GraphicsCaptureItem" /> for the specified window.
  /// </summary>
  /// <param name="window"> The window to create a capture item for. </param>
  /// <returns> The capture item for the specified window. </returns>
  /// <exception cref="InvalidOperationException">
  ///   Thrown if the capture item could not be created using the native WinRT API.
  /// </exception>
  /// <exception cref="InvalidCastException">
  ///   Thrown if the capture item was created but could not be cast to the desired WinRT type.
  /// </exception>
  private GraphicsCaptureItem CreateCaptureItemForWindow(HWND window) {
    // The WinRT API for capturing a window has some limitations. One of those limitations is that
    // the window we're capturing cannot have an owner. If the window has an owner, then we need to
    // remove it before we can capture the window.
    var captureItemPtr = window.RemoveOwner().CreateCaptureItem();

    if (captureItemPtr == nint.Zero) {
      throw new InvalidOperationException("Failed to create capture item.");
    }

    // Attempt to cast the object to the desired WinRT type. This is necessary as we only have an
    // opaque pointer to the object from the "unmanaged" side of the application. Fortunately,
    // the managed WinRT API provides a way to cast opaque pointers to WinRT objects. Because we
    // know the actual type of the object we're pointer to, we can call the "FromAbi" method on the
    // GraphicsCaptureItem class to cast the opaque pointer to a managed object.
    var graphicsCaptureItem = GraphicsCaptureItem.FromAbi(captureItemPtr);

    if (graphicsCaptureItem == null) {
      throw new InvalidCastException("Failed to cast to GraphicsCaptureItem.");
    }

    return graphicsCaptureItem;
  }


  private void InitializeCapture(GraphicsCaptureItem item, SwapChainPanel panel) {
    // If there is already a capturer, then close it before creating a new one.
    capturer?.Close();

    capturer = new SimpleCapturer(item, panel);
    capturer.FrameRateChanged += (newFrameRate, newFrameTime) => {
      FrameRateChanged?.Invoke(this, (newFrameRate, newFrameTime));
    };
    capturer.StartCapture();
  }
}
