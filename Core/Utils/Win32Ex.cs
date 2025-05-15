using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Core.Utils;

[Flags]
public enum ProcessAccess {
  AllAccess               = 0x001FFFFF,
  Terminate               = 0x00000001,
  CreateThread            = 0x00000002,
  VirtualMemoryOperation  = 0x00000008,
  VirtualMemoryRead       = 0x00000010,
  VirtualMemoryWrite      = 0x00000020,
  DuplicateHandle         = 0x00000040,
  CreateProcess           = 0x000000080,
  SetQuota                = 0x00000100,
  SetInformation          = 0x00000200,
  QueryInformation        = 0x00000400,
  QueryLimitedInformation = 0x00001000,
  Synchronize             = 0x00100000
}

[Flags]
public enum PROCESSINFOCLASS {
  ProcessProtectionInformation = 0x3D
}

[StructLayout(LayoutKind.Sequential)]
public struct _PsProtection {
  public PsProtectedType   Type;
  public PsProtectedSigner Signer;
  public bool              Audit;
}

[Flags]
public enum PsProtectedType {
  PsProtectedTypeNone           = 0x0,
  PsProtectedTypeProtectedLight = 0x1,
  PsProtectedTypeProtected      = 0x2,
  PsProtectedTypeMax            = 0x3
}

[Flags]
public enum PsProtectedSigner {
  PsProtectedSignerNone         = 0x0,
  PsProtectedSignerAuthenticode = 0x1,
  PsProtectedSignerCodeGen      = 0x2,
  PsProtectedSignerAntimalware  = 0x3,
  PsProtectedSignerLsa          = 0x4,
  PsProtectedSignerWindows      = 0x5,
  PsProtectedSignerWinTcb       = 0x6,
  PsProtectedSignerMax          = 0x7
}

/// <summary>
///   Includes overloads of Win32 API functions that are not included in the Windows.Win32 namespace.
///   This is largely for a more convenient API that toes the line between the C# and C++ APIs.
/// </summary>
public static class Win32Ex {
  [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
  public static extern int _GetWindowLong(HWND hWnd, int nIndex);


  [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
  public static extern nint _GetWindowLongPtr(HWND hWnd, int nIndex);


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
  ///   Retrieves information about the specified window. The function also retrieves the value at a
  ///   specified offset into the extra window memory.
  /// </summary>
  /// <param name="hWnd"> A handle to the window and, indirectly, the class to which the window belongs. </param>
  /// "
  /// <param name="nIndex"> The zero-based offset to the value to be retrieved. </param>
  /// <returns>
  ///   If the function succeeds, the return value is the requested value. Otherwise, the return value is
  ///   zero.
  /// </returns>
  public static nint GetWindowLong(HWND hWnd, WINDOW_LONG_PTR_INDEX nIndex) {
    // If the pointer size is 32 bits, we call the 32-bit version of the function. Otherwise, we call
    // the 64-bit version.
    if (NativeUtils.GetPointerSize() == PointerSize.PointerSize32) {
      return new nint(_GetWindowLong(hWnd, (int)nIndex));
    }

    return _GetWindowLongPtr(hWnd, (int)nIndex);
  }


  [DllImport("ntdll.dll", SetLastError = true)]
  public static extern int NtQueryInformationProcess(
    nint processHandle,
    PROCESSINFOCLASS processInformationClass,
    ref _PsProtection processInformation,
    int processInformationLength,
    ref int returnLength
  );


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
