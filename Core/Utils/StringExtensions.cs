using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace Core.Utils;

/// <summary>
///   A collection of extensions methods for the <see cref="PWSTR" /> and <see cref="PCWSTR" /> types.
/// </summary>
public static class StringExtensions {
  /// <summary>
  ///   Converts a string to a <see cref="PCWSTR" /> - a pointer to a null-terminated constant "wide"
  ///   string in the Windows API. Make sure to free the memory after use, using
  ///   <see cref="Marshal.FreeHGlobal" />.
  /// </summary>
  /// <param name="str"> The string to convert. </param>
  /// <returns> A <see cref="PCWSTR" /> pointing to the string. </returns>
  public static unsafe PCWSTR ToPCWSTR(this string? str) {
    return new PCWSTR((char*)Marshal.StringToHGlobalUni(str));
  }


  /// <summary>
  ///   Converts a string to a <see cref="PWSTR" /> - a pointer to a null-terminated "wide" string
  ///   in the Windows API. Make sure to free the memory after use, using
  ///   <see cref="Marshal.FreeHGlobal" />.
  /// </summary>
  /// <param name="str"> The string to convert. </param>
  /// <returns> A <see cref="PWSTR" /> pointing to the string. </returns>
  public static unsafe PWSTR ToPWSTR(this string? str) {
    return new PWSTR((char*)Marshal.StringToHGlobalUni(str));
  }
}
