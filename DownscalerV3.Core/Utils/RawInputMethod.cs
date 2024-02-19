namespace DownscalerV3.Core.Utils;

/// <summary>
///   Represents the method that is used to process the input from a raw input device.
///   <see
///     href="https://docs.microsoft.com/windows/win32/api/winuser/ns-winuser-rawinputheader#members" />
/// </summary>
public enum RawInputMethod {
  /// <summary>
  ///   The input is coming from a mouse.
  /// </summary>
  RIM_TYPEMOUSE = 0,

  /// <summary>
  ///   The input is coming from a keyboard.
  /// </summary>
  RIM_TYPEKEYBOARD = 1,

  /// <summary>
  ///   The input is coming from an HID (Human-interface-device) that is not a keyboard or a mouse.
  /// </summary>
  RIM_TYPEHID = 2
}
