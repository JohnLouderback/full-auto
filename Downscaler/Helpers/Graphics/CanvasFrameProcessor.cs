using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Graphics.Capture;
using DownscalerV3.Core.Models;
using DownscalerV3.Core.Utils;
using Microsoft.Graphics.Canvas;
using Microsoft.UI;

namespace Downscaler.Helpers.Graphics;

public class CanvasFrameProcessor {
  /// <summary>
  ///   The device used for creating the canvas bitmap.
  /// </summary>
  private readonly CanvasDevice canvasDevice;

  /// <summary>
  ///   The swap chain used for rendering the frame on the back buffer.
  /// </summary>
  private readonly CanvasSwapChain swapChain;

  private readonly Win32Window sourceWindow;

  private readonly Rect destRect;

  /// <summary>
  ///   The bitmap that the frame will be drawn to.
  /// </summary>
  private CanvasBitmap frameBitmap;

  private Rect srcRect;


  /// <summary>
  ///   Instantiates a new <c> CanvasFrameProcessor </c> with the provided <see cref="CanvasDevice" />.
  /// </summary>
  /// <param name="device"> The device used for creating the frame bitmap. </param>
  /// <param name="swapChain"> The swap chain used for rendering the frame. </param>
  /// <param name="sourceWindow">
  ///   The window from which the frame was captured. Used to crop the source rect down to just the
  ///   client area of the window.
  /// </param>
  public CanvasFrameProcessor(
    CanvasDevice device,
    CanvasSwapChain swapChain,
    in Win32Window sourceWindow
  ) {
    canvasDevice      = device;
    this.swapChain    = swapChain;
    this.sourceWindow = sourceWindow;
    destRect          = new Rect(0, 0, swapChain.Size.Width, swapChain.Size.Height);
  }


  /// <summary>
  ///   Processes the provided frame by drawing it to the swap chain.
  /// </summary>
  /// <param name="frame"> The frame to process. </param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ProcessFrame(Direct3D11CaptureFrame frame) {
    // Ensure the bitmap is created and is the correct size.
    EnsureBitmap(frame);

    // The frame is drawn to the bitmap.
    using (var drawingSession = swapChain.CreateDrawingSession(Colors.Black)) {
      drawingSession.DrawImage(
        frameBitmap,
        destRect,
        srcRect,
        1.0f,
        CanvasImageInterpolation.NearestNeighbor
      );
    }

    // Present the contents of the swap chain to the screen
    swapChain.Present(0);
  }


  /// <summary>
  ///   Ensures that the frame bitmap is created and is the correct size. If the bitmap is not
  ///   created, it is created. If the size of the frame has changed, the bitmap is recreated.
  /// </summary>
  /// <param name="frame"> The frame to ensure the bitmap for. </param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void EnsureBitmap(Direct3D11CaptureFrame frame) {
    // The frame bitmap is created if it is null or if the size of the frame has changed.
    if (frameBitmap == null ||
        // Check to see if the difference in size is greater than 1 pixel. This is to account for
        // floating point errors.
        Math.Abs(frameBitmap.Size.Width - frame.ContentSize.Width) > 1 ||
        Math.Abs(frameBitmap.Size.Height - frame.ContentSize.Height) > 1
       ) {
      frameBitmap = CanvasBitmap.CreateFromDirect3D11Surface(canvasDevice, frame.Surface);

      // Get the area to crop to based on the source window's client area relative to the window. We
      // don't, for example, want to show the window "chrome" (the window's title bar, borders, etc.)
      var crop = sourceWindow.GetClientRectRelativeToWindow();

      srcRect = new Rect(
        crop.left,
        crop.top,
        crop.Width,
        crop.Height
      );
    }
  }
}
