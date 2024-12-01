using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Win32.Foundation;
using Downscaler.Core.Contracts.Models.AppState;
using Downscaler.Core.Models;
using Downscaler.Core.Utils;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
// For SwapChainPanel in WinUI 3
// As of my last update, capture APIs might still be under Windows.*
using DirectXPixelFormat =
  Windows.Graphics.DirectX.DirectXPixelFormat; // For composition in UWP/WinUI

// Additional using directives as needed for interop or specific graphics tasks

namespace Downscaler.Helpers.Graphics;

public class SimpleCapturer : ICapturer {
  private readonly GraphicsCaptureItem item;
  private readonly SwapChainPanel      swapChainPanel;

  // ReSharper disable once InconsistentNaming - AppState is accessed like global static class.
  private readonly IAppState                  AppState;
  private readonly DispatcherQueue?           dispatcherQueue;
  private          CanvasSwapChain            swapChain;
  private          IDirect3DDevice            device;
  private          CanvasDevice               canvasDevice;
  private          Direct3D11CaptureFramePool framePool;
  private          GraphicsCaptureSession     session;
  private          Stopwatch                  stopwatch;

  /// <summary>
  ///   The number of frames that have been processed since the last FPS report.
  /// </summary>
  private int frameCount;

  /// <summary>
  ///   The time in seconds since the last FPS report.
  /// </summary>
  private double lastFpsReport;

  /// <summary>
  ///   The current frames per second.
  /// </summary>
  private double fps;

  /// <summary>
  ///   The time in milliseconds that it took to process the last frame.
  /// </summary>
  private double frameTime;

  /// <summary>
  ///   The frame processor used to process the frames and render them to the swap chain.
  /// </summary>
  private CanvasFrameProcessor frameProcessor;

  private Win32Window windowToScale;

  public delegate void FrameRateChangedEventHandler(double newFrameRate, double newFrameTime);

  /// <inheritdoc />
  public event FrameRateChangedEventHandler? FrameRateChanged;


  /// <summary>
  ///   Instantiates a new <c> SimpleCapturer </c> with the provided <see cref="GraphicsCaptureItem" />
  ///   and <see cref="SwapChainPanel" /> and <see cref="HWND" />.
  /// </summary>
  /// <param name="item"> The window to capture. </param>
  /// <param name="panel"> The panel to render the captured frames to. </param>
  /// <param name="appState"> The application state. </param>
  /// <param name="dispatcherQueue">
  ///   The dispatcher queue to use for the capturer. Captured frames will be rendered on this
  ///   dispatcher queue. If omitted, the renderer will use a free-threaded approach entirely
  ///   independent of the UI thread. The trade-off between the two approaches is that the
  ///   free-threaded approach is faster but may cause issues with UI synchronization, while the
  ///   dispatcher queue approach is slower but is guaranteed to be thread-safe and synchronized with
  ///   the UI - possibly preventing lag spikes and other issues.
  /// </param>
  public SimpleCapturer(
    GraphicsCaptureItem item,
    SwapChainPanel panel,
    IAppState appState,
    DispatcherQueue? dispatcherQueue = null
  ) {
    this.item            = item;
    swapChainPanel       = panel;
    AppState             = appState;
    this.dispatcherQueue = dispatcherQueue;

    InitializeCapture();
  }


  public void Close() {
    // Cleanup logic remains the same
    framePool?.Dispose();
    session?.Dispose();
  }


  public void StartCapture() {
    session.StartCapture();
  }


  private void InitializeCapture() {
    var size        = item.Size;
    var pixelFormat = DirectXPixelFormat.B8G8R8A8UIntNormalized;

    canvasDevice = CanvasDevice.GetSharedDevice();
    device       = canvasDevice;

    if (dispatcherQueue is not null) {
      framePool = Direct3D11CaptureFramePool.Create(
        canvasDevice,
        pixelFormat,
        // 2, // Double buffer
        1, // Single buffer
        size
      );
    }
    else {
      framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
        canvasDevice,
        pixelFormat,
        2, // Double buffer i.e. VSync
        // 1, // Single buffer
        size
      );
    }

    session                        = framePool.CreateCaptureSession(item);
    session.IsCursorCaptureEnabled = false;

    // If there is a difference in DPI between the downscale window and the window being scaled,
    // then we'll need to adjust the size of the swap chain to account for the DPI difference.
    windowToScale = AppState.WindowToScale;
    var dpiScaleFactor = AppState.DownscaleWindow.GetMonitor().GetDpi() /
                         (float)windowToScale.GetMonitor().GetDpi();

    // Listen for frame arrival events.
    framePool.FrameArrived += OnFrameArrived;

    // Initialize the swap chain
    swapChain = new CanvasSwapChain(
      canvasDevice,
      // size.Width * dpiScaleFactor,
      // size.Height * dpiScaleFactor,
      AppState.DownscaleWidth * dpiScaleFactor,
      AppState.DownscaleHeight * dpiScaleFactor,
      //windowToScale.GetMonitor().GetDpi()

      // Needs investigating. Even when the source and downscale windows have "120" for their
      // DPI, the frame comes back at 96 DPI. If this value _is not_ 96 DPI, the rendered frame
      // will appear slightly blurry even when nearest neighbor interpolation is used.
      96
    );

    // Initialize the frame processor
    frameProcessor = new CanvasFrameProcessor(canvasDevice, swapChain, in windowToScale);

    // Create a CanvasSwapChainPanel and assign the swap chain to it.
    var swapChainPanelControl = new CanvasSwapChainPanel {
      SwapChain = swapChain
    };

    // Add the CanvasSwapChainPanel to the SwapChainPanel's Children collection.
    swapChainPanel.Children.Insert(0, swapChainPanelControl);

    // Ensure it takes the full size of the SwapChainPanel.
    swapChainPanelControl.Width  = swapChainPanel.ActualWidth;
    swapChainPanelControl.Height = swapChainPanel.ActualHeight;

    // Set up the frame rate monitoring behavior.
    stopwatch     = Stopwatch.StartNew();
    frameCount    = 0;
    lastFpsReport = 0;
  }


  private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args) {
    using (var frame = sender.TryGetNextFrame()) {
      // Process the frame and render it to the swap chain
      frameProcessor.ProcessFrame(frame);
    }

    // Increment the frame count and don't check for overflow.
    unchecked {
      frameCount++;
    }

    // Update the frame count and FPS if necessary
    UpdateFps();
  }


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void UpdateFps() {
    // If the stopwatch has been running for at least a 750 ms, calculate the new FPS.
    if (stopwatch.Elapsed.TotalSeconds - lastFpsReport >= 0.75) {
      // For performance, we don't check for overflow in any of the following operations.
      unchecked {
        var newFPS = frameCount / (stopwatch.Elapsed.TotalSeconds - lastFpsReport);
        frameTime = 1000.0 / newFPS;

        // Reset counters
        frameCount    = 0;
        lastFpsReport = stopwatch.Elapsed.TotalSeconds;

        // Round the FPS to the nearest integer and check if it has changed. If it has, raise the event.
        if ((int)newFPS != (int)fps) {
          fps = newFPS;
          FrameRateChanged?.Invoke(
            Math.Round(newFPS, MidpointRounding.AwayFromZero),
            frameTime
          );
        }
      }
    }
  }
}
