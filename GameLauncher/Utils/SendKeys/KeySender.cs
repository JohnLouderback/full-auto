using System.Runtime.InteropServices;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using static Windows.Win32.PInvoke;

namespace GameLauncher.Utils.SendKeys;

/// <summary>
///   Accepts a string formatted according to the AutoHotkey SendKeys syntax and synthesizes
///   keystrokes.
/// </summary>
public class KeySender {
  /// <summary>
  ///   Synthesizes keystrokes from a SendKeys-style string.
  /// </summary>
  /// <param name="keys">
  ///   A string formatted according to the AutoHotkey SendKeys syntax (e.g. "^a", "{Enter}", "{Del 4}",
  ///   etc.).
  /// </param>
  public static void Send(string keys) {
    var parser     = new SendKeysParser(keys);
    var keystrokes = parser.Parse();
    Send(keystrokes);
  }


  /// <summary>
  ///   Synthesizes keystrokes from an enumerable of keystroke events.
  /// </summary>
  /// <param name="keystrokes">An enumerable of keystroke events.</param>
  public static void Send(IEnumerable<Keystroke> keystrokes) {
    // Convert to a list to avoid multiple enumerations.
    var keystrokeList = keystrokes.ToList();

    // Create an array of INPUT structures, one per keystroke.
    var inputs = new INPUT[keystrokeList.Count];
    for (var i = 0; i < keystrokeList.Count; i++) {
      var ks = keystrokeList[i];
      inputs[i] = new INPUT {
        type = INPUT_TYPE.INPUT_KEYBOARD,
        Anonymous = new INPUT._Anonymous_e__Union {
          ki = new KEYBDINPUT {
            wVk         = MapKey(ks.Key),
            wScan       = 0,
            dwFlags     = ks.IsDown ? 0u : KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP,
            time        = 0,
            dwExtraInfo = nuint.Zero
          }
        }
      };
    }

    // Send the synthesized inputs via the Windows API.
    unsafe {
      fixed (INPUT* pInputs = inputs) {
        var cbSize = Marshal.SizeOf<INPUT>();
        var sent   = SendInput((uint)inputs.Length, pInputs, cbSize);
        if (sent != inputs.Length) {
          throw new InvalidOperationException("Not all input events were successfully sent.");
        }
      }
    }
  }


  /// <summary>
  ///   Maps a string key name to its corresponding virtual key code.
  ///   For literal single-character keys, the uppercase ASCII code is returned.
  /// </summary>
  /// <param name="key">The key name (e.g., "a", "Enter", "Ctrl").</param>
  /// <returns>The virtual key code as a <see cref="VIRTUAL_KEY" />.</returns>
  /// <exception cref="ArgumentException">Thrown if the key is not recognized.</exception>
  private static VIRTUAL_KEY MapKey(string key) {
    switch (key.ToLowerInvariant()) {
      case "ctrl":
        return VIRTUAL_KEY.VK_CONTROL;
      case "shift":
        return VIRTUAL_KEY.VK_SHIFT;
      case "alt":
        return VIRTUAL_KEY.VK_MENU;
      case "win":
        return VIRTUAL_KEY.VK_LWIN;
      case "enter":
        return VIRTUAL_KEY.VK_RETURN;
      case "del":
      case "delete":
        return VIRTUAL_KEY.VK_DELETE;
      default:
        if (key.Length == 1) {
          return (VIRTUAL_KEY)char.ToUpperInvariant(key[0]);
        }

        throw new ArgumentException($"Unknown key: {key}");
    }
  }
}
