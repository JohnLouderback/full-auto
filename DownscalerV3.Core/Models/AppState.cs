using DownscalerV3.Core.Contracts.Models;
using DownscalerV3.Core.Utils;
using static DownscalerV3.Core.Utils.Macros;

namespace DownscalerV3.Core.Models;

public class AppState : IAppState {
  private uint?   downscaleWidth;
  private uint?   downscaleHeight;
  private uint?   windowWidth;
  private uint?   windowHeight;
  private double? downscaleFactor;

  /// <inheritdoc />
  public uint WindowWidth {
    get {
      if (windowWidth is not null) {
        return windowWidth.Value;
      }

      return DownscaleWidth;
    }

    set => windowWidth = value;
  }

  /// <inheritdoc />
  public uint WindowHeight {
    get {
      if (windowHeight is not null) {
        return windowHeight.Value;
      }

      return DownscaleHeight;
    }

    set => windowHeight = value;
  }

  /// <inheritdoc />
  public double DownscaleFactor {
    get {
      // If the downscale factor has been set, return it.
      if (downscaleFactor is not null) {
        return downscaleFactor.Value;
      }

      // Otherwise, use the width and height to calculate the downscale factor based on their values
      // compared to the window-to-scale's width and height.
      if (downscaleWidth is not null ||
          downscaleHeight is not null) {
        if (WindowToScale.Hwnd.Value is nullptr) {
          throw new InvalidOperationException(
            "Window to scale must be set before calculating downscale factor."
          );
        }

        var width  = downscaleWidth ?? (uint)WindowToScale.GetClientWidth();
        var height = downscaleHeight ?? (uint)WindowToScale.GetClientHeight();

        return (double)WindowToScale.GetClientWidth() / width;
      }

      throw new InvalidOperationException(
        "The operation cannot be performed because neither the downscale factor nor the downscale width/height has been set."
      );
    }
    set {
      if (downscaleFactor is not null) {
        throw new InvalidOperationException("Downscale factor has already been set.");
      }

      if (downscaleWidth is not null ||
          downscaleHeight is not null) {
        throw new InvalidOperationException(
          "Downscale width or height has already been set. Only downscale factor or width/height can be set, not both."
        );
      }

      downscaleFactor = value;
    }
  }

  /// <inheritdoc />
  public uint DownscaleWidth {
    get {
      if (downscaleWidth is not null) {
        return downscaleWidth.Value;
      }

      if (downscaleFactor is not null) {
        if (WindowToScale.Hwnd.Value is nullptr) {
          throw new InvalidOperationException(
            "Window to scale must be set before calculating downscale width."
          );
        }

        return (uint)(WindowToScale.GetClientWidth() / downscaleFactor.Value);
      }

      throw new InvalidOperationException(
        "The operation cannot be performed because neither the downscale factor nor the downscale width/height has been set."
      );
    }

    set {
      if (downscaleWidth is not null) {
        throw new InvalidOperationException("Downscale width has already been set.");
      }

      if (downscaleFactor is not null) {
        throw new InvalidOperationException(
          "Downscale factor has already been set. Only downscale width or factor can be set, not both."
        );
      }

      downscaleWidth = value;
    }
  }

  /// <inheritdoc />
  public uint DownscaleHeight {
    get {
      if (downscaleHeight is not null) {
        return downscaleHeight.Value;
      }

      if (downscaleFactor is not null) {
        if (WindowToScale.Hwnd.Value is nullptr) {
          throw new InvalidOperationException(
            "Window to scale must be set before calculating downscale height."
          );
        }

        return (uint)(WindowToScale.GetClientHeight() / downscaleFactor.Value);
      }

      throw new InvalidOperationException(
        "The operation cannot be performed because neither the downscale factor nor the downscale width/height has been set."
      );
    }

    set {
      if (downscaleHeight is not null) {
        throw new InvalidOperationException("Downscale height has already been set.");
      }

      if (downscaleFactor is not null) {
        throw new InvalidOperationException(
          "Downscale factor has already been set. Only downscale height or factor can be set, not both."
        );
      }

      downscaleHeight = value;
    }
  }

  /// <inheritdoc />
  public AspectRatio AspectRatio { get; set; }

  /// <inheritdoc />
  public Win32Window WindowToScale { get; set; }

  /// <inheritdoc />
  public Win32Window DownscaleWindow { get; set; }

  /// <inheritdoc />
  public IEnumerable<Win32Window> AllWindows { get; set; }
}
