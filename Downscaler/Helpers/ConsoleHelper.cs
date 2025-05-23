using System.Runtime.InteropServices;

namespace Downscaler.Helpers;

internal static class ConsoleHelper {
  private const int ATTACH_PARENT_PROCESS = -1;
  private const int STD_OUTPUT_HANDLE     = -11;
  private const int STD_ERROR_HANDLE      = -12;


  public static void TryAttachToParentConsole() {
    // Check if the process is running in a console window
    if (AttachConsole(ATTACH_PARENT_PROCESS)) {
      var stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
      if (stdOutHandle != nint.Zero &&
          stdOutHandle != new nint(-1)) {
        var writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
        Console.SetOut(writer);
      }

      var stdErrHandle = GetStdHandle(STD_ERROR_HANDLE);
      if (stdErrHandle != nint.Zero &&
          stdErrHandle != new nint(-1)) {
        var errWriter = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
        Console.SetError(errWriter);
      }
    }
  }


  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool AttachConsole(int dwProcessId);


  [DllImport("kernel32.dll")] private static extern nint GetStdHandle(int nStdHandle);
}
