using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GenericModLauncher;

public static class Utils {
  /// <summary>
  ///   Given a filesystem path, loads an image from that path and returns it as an
  ///   <see cref="ImageSource" />.
  /// </summary>
  /// <param name="path"> The path to the image file.</param>
  /// <returns> An <see cref="ImageSource" /> representing the image at the specified path.</returns>
  public static ImageSource LoadImage(string path) {
    var bitmap = new BitmapImage();
    bitmap.BeginInit();
    bitmap.UriSource   = new Uri(path, UriKind.RelativeOrAbsolute);
    bitmap.CacheOption = BitmapCacheOption.OnLoad;
    bitmap.EndInit();
    bitmap.Freeze(); // Makes the image thread-safe and immutable
    return bitmap;
  }
}
