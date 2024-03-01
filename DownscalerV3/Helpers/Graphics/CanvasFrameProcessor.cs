using System.Runtime.CompilerServices;
using Windows.Graphics.Capture;
using Microsoft.Graphics.Canvas;
using Microsoft.UI;

namespace DownscalerV3.Helpers.Graphics;

public class CanvasFrameProcessor {
  /// <summary>
  ///   The device used for creating the canvas bitmap.
  /// </summary>
  private readonly CanvasDevice canvasDevice;

  /// <summary>
  ///   The swap chain used for rendering the frame on the back buffer.
  /// </summary>
  private readonly CanvasSwapChain swapChain;

  /// <summary>
  ///   The bitmap that the frame will be drawn to.
  /// </summary>
  private CanvasBitmap frameBitmap;


  /// <summary>
  ///   Instantiates a new <c> CanvasFrameProcessor </c> with the provided <see cref="CanvasDevice" />.
  /// </summary>
  /// <param name="device"> The device used for creating the frame bitmap. </param>
  /// <param name="swapChain"> The swap chain used for rendering the frame. </param>
  public CanvasFrameProcessor(CanvasDevice device, CanvasSwapChain swapChain) {
    canvasDevice   = device;
    this.swapChain = swapChain;
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
      drawingSession.DrawImage(frameBitmap);
    }

    // Present the contents of the swap chain to the screen
    swapChain.Present(1);
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
        frameBitmap.SizeInPixels.Width != frame.ContentSize.Width ||
        frameBitmap.SizeInPixels.Height != frame.ContentSize.Height
       ) {
      frameBitmap = CanvasBitmap.CreateFromDirect3D11Surface(canvasDevice, frame.Surface);
    }
  }
}
