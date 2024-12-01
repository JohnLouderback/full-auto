using Windows.Win32;
using Windows.Win32.Foundation;
using Microsoft.Win32.SafeHandles;

namespace Downscaler.Core.Utils;

/// <summary>
///   Represents a safe handle for a module. A "safe handle" is a handle that is automatically
///   released when it is no longer needed. This wraps the native <see cref="HMODULE" /> type.
/// </summary>
public class HModuleSafeHandle : SafeHandleZeroOrMinusOneIsInvalid {
  public HModuleSafeHandle() : base(true) {}


  protected override bool ReleaseHandle() {
    return PInvoke.FreeLibrary((HMODULE)handle);
  }
}
