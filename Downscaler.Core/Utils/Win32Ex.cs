using System.ComponentModel.DataAnnotations;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Downscaler.Core.Utils;

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
}
