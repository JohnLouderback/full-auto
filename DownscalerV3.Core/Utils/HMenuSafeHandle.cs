using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Microsoft.Win32.SafeHandles;

namespace DownscalerV3.Core.Utils;

/// <summary>
///   Represents a safe handle for a menu. A "safe handle" is a handle that is automatically
///   released when it is no longer needed. This wraps the native <see cref="HMENU" /> type.
/// </summary>
public class HMenuSafeHandle : SafeHandleZeroOrMinusOneIsInvalid {
  public HMenuSafeHandle() : base(true) {}


  // This constructor allows you to instantiate with a pre-existing handle.
  public HMenuSafeHandle(nint preexistingHandle, bool ownsHandle)
    : base(ownsHandle) {
    SetHandle(preexistingHandle);
  }


  protected override bool ReleaseHandle() {
    return PInvoke.DestroyMenu((HMENU)handle);
  }
}
