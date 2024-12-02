using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace Core.Utils;

public static class StringExtensions {
  /// <summary>
  ///   Converts a string to a <see cref="PWSTR" /> - a pointer to a null-terminated Unicode string
  ///   in the Windows API.
  /// </summary>
  /// <param name="str"> The string to convert. </param>
  /// <returns> A <see cref="PWSTR" /> pointing to the string. </returns>
  public static unsafe PWSTR ToPWSTR(this string? str) {
    return new PWSTR((char*)Marshal.StringToHGlobalUni(str));
  }
}
