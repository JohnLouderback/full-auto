using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Core.Utils;

/// <summary>
///   Includes overloads of Win32 API functions that are not included in the Windows.Win32 namespace.
///   This is largely for a more convenient API that toes the line between the C# and C++ APIs.
/// </summary>
public static class Win32Ex {
  /// <inheritdoc cref="Windows.Win32.PInvoke.GetModuleHandle(PCWSTR)" />
  [SupportedOSPlatform("windows5.1.2600")]
  public static unsafe FreeLibrarySafeHandle GetModuleHandle(string? lpModuleName = null) {
    fixed (char* lpModuleNameLocal = lpModuleName) {
      var __result = PInvoke.GetModuleHandle(lpModuleNameLocal);
      return new FreeLibrarySafeHandle(__result, false);
    }
  }


  /// <inheritdoc cref="Windows.Win32.PInvoke.GetModuleHandle(PCWSTR)" />
  [SupportedOSPlatform("windows5.1.2600")]
  public static unsafe HMODULE GetModuleHandle([AllowedValues(0)] int lpModuleName) {
    if (lpModuleName != 0) {
      throw new ArgumentException(
        $"This overload of {
          nameof(GetModuleHandle)
        } expects the integral value to be zero - representing a null pointer. Non-zero values are not supported.",
        nameof(lpModuleName)
      );
    }

    // We don't actually care about the value of lpModuleName, so we can just pass null as that is
    // what "0" represents in the C++ API.
    string? nullLpModuleName = null;
    fixed (char* lpModuleNameLocal = nullLpModuleName) {
      return PInvoke.GetModuleHandle(lpModuleNameLocal);
    }
  }


  /// <summary>
  ///   Changes an attribute of the specified window. The function also sets a value at the specified
  ///   offset in the extra window memory.
  /// </summary>
  /// <param name="hWnd"> A handle to the window and, indirectly, the class to which the window belongs. </param>
  /// <param name="nIndex"> The zero-based offset to the value to be set. </param>
  /// <param name="dwNewLong"> The replacement value. </param>
  /// <returns>
  ///   If the function succeeds, the return value is the previous value of the specified offset.
  ///   Otherwise, the return value is zero.
  /// </returns>
  public static nint SetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex, int dwNewLong) {
    // If the pointer size is 32 bits, we call the 32-bit version of the function. Otherwise, we call
    // the 64-bit version.
    if (NativeUtils.GetPointerSize() == PointerSize.PointerSize32) {
      return new nint(_SetWindowLong(hWnd, (int)nIndex, dwNewLong)).ToInt32();
    }

    return (nint)_SetWindowLongPtr(hWnd, (int)nIndex, dwNewLong).ToInt64();
  }


  [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
  private static extern int _SetWindowLong(nint hWnd, int nIndex, int dwNewLong);


  [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
  private static extern nint _SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);
}
