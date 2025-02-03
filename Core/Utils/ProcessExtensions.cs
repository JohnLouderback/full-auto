using System.Diagnostics;
using System.Management;

namespace Core.Utils;

// using System.Management;
public static class ProcessExtensions {
  /// <summary>
  ///   Gets the child processes of the specified process.
  /// </summary>
  /// <param name="process"> The process whose child processes to get. </param>
  /// <returns> The child processes of the specified process. </returns>
  /// <seealso href="https://stackoverflow.com/a/38614443" />
  public static IList<Process> GetChildProcesses(this Process process) {
    return new ManagementObjectSearcher(
        $"Select * From Win32_Process Where ParentProcessID={process.Id}"
      )
      .Get()
      .Cast<ManagementObject>()
      .Select(
        mo =>
          Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]))
      )
      .ToList();
  }
}
