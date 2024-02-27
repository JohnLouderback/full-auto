using System.Runtime.CompilerServices;
using Windows.Win32.Foundation;

// ReSharper disable InconsistentNaming

namespace DownscalerV3.Core.Utils;

/// <summary>
///   A series of utility functions that mimic the behavior of some Win32 macros.
/// </summary>
public static class Macros {
  public const int NULL    = 0;
  public const int nullptr = 0;


  /// <summary>
  ///   Extracts the x-coordinate from the given LPARAM value. The x-coordinate is the low-order word
  ///   of the given value. For example, if the hex value is 0x12345678, the x-coordinate is 0x5678
  ///   (or 22040 in decimal).
  /// </summary>
  /// <param name="lp"> The LPARAM value to extract the x-coordinate from. </param>
  /// <returns> The x-coordinate of the given LPARAM value. </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GET_X_LPARAM(LPARAM lp) {
    return LOWORD((int)lp);
  }


  /// <summary>
  ///   Extracts the y-coordinate from the given LPARAM value. The y-coordinate is the high-order word
  ///   of the given value. For example, if the hex value is 0x12345678, the y-coordinate is 0x1234
  ///   (or 4660 in decimal).
  /// </summary>
  /// <param name="lp"> The LPARAM value to extract the y-coordinate from. </param>
  /// <returns> The y-coordinate of the given LPARAM value. </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int GET_Y_LPARAM(LPARAM lp) {
    return HIWORD((int)lp);
  }


  /// <summary>
  ///   Extracts the high-order word from the given value. A "word" is a 16-bit value. In the binary
  ///   representation of the given value, the high-order word is the most significant word. For
  ///   example, if the value is 0b00001100_00000000_10000000_00000001, the high-order word is
  ///   0b00001100_00000000. Or, in hex, if the value is 0x0C008001, the high-order word is 0x0C00.
  /// </summary>
  /// <param name="l"> The value to extract the high-order word from. </param>
  /// <returns> The high-order word of the given value. </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int HIWORD(int l) {
    return l >> 16;
  }


  /// <summary>
  ///   Extracts the low-order word from the given value. A "word" is a 16-bit value. In the binary
  ///   representation of the given value, the low-order word is the least significant word. For
  ///   example, if the value is 0b00001100_00000000_10000000_00000001, the low-order word is
  ///   0b10000000_00000001. Or, in hex, if the value is 0x0C008001, the low-order word is 0x8001.
  /// </summary>
  /// <param name="l"> The value to extract the low-order word from. </param>
  /// <returns> The low-order word of the given value. </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static int LOWORD(int l) {
    return l & 0xFFFF;
  }


  /// <summary>
  ///   Determines whether the given HRESULT value indicates success. An HRESULT value is a 32-bit
  ///   value that is used to indicate success or failure. If the value is greater than or equal to
  ///   0, the operation was successful. If the value is less than 0, the operation failed.
  /// </summary>
  /// <param name="hr"> The HRESULT value to check. </param>
  /// <returns>
  ///   <see langword="true" /> if the operation was successful; otherwise, <see langword="false" />.
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool SUCCEEDED(HRESULT hr) {
    return hr >= 0;
  }
}
