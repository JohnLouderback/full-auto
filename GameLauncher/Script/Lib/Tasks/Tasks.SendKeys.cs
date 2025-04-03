using GameLauncher.Utils.SendKeys;

namespace GameLauncher.Script;

public partial class Tasks {
  /// <summary>
  ///   <para>
  ///     Synthesizes keystrokes from a SendKeys-style string. These are either handled by the
  ///     currently focused window or the system.
  ///   </para>
  ///   <para>
  ///     <b>Examples of input:</b><br />
  ///     <list type="bullet">
  ///       <item><c>SendKeys("^a");</c> will send Ctrl + A.</item>
  ///       <item><c>SendKeys("{F1}");</c> will send F1.</item>
  ///       <item><c>SendKeys("#!^a");</c> will send Windows + Alt + Ctrl + A.</item>
  ///       <item><c>SendKeys("abcd");</c> will send a, b, c, d.</item>
  ///       <item><c>SendKeys("{Enter}");</c> will send Enter.</item>
  ///       <item><c>SendKeys("{Del 4}");</c> will send Del 4 times.</item>
  ///       <item><c>SendKeys("Hello World!");</c> will send Hello World!.</item>
  ///     </list>
  ///   </para>
  ///   <para>
  ///     Note that, while this method cannot be awaited, there is an inherent asynchronicity in
  ///     synthesizing keystrokes. The keystrokes are sent to the system's input queue, and the
  ///     synthesized keystrokes may not be processed immediately. This is due to the nature of the
  ///     input queue and the way the operating system handles input events.
  ///   </para>
  /// </summary>
  /// <param name="keys"></param>
  public static void SendKeys(string keys) {
    var parser     = new SendKeysParser(keys);
    var keystrokes = parser.Parse();
    KeySender.Send(keystrokes);
  }
}
