using System.Collections;
using System.ComponentModel;
using Windows.Win32.System.Threading;
using Core.Utils;
using GameLauncher.Script.Utils;
using GameLauncher.Script.Utils.CodeGenAttributes;
using Microsoft.ClearScript;
using Microsoft.Win32.SafeHandles;
using static Windows.Win32.PInvoke;
using static Core.Utils.Win32Ex;
using static GameLauncher.Script.Utils.JSTypeConverter;

namespace GameLauncher.Script.Objects;

/// <summary>
///   Represents a process that is or was running on the system.
/// </summary>
[TypeScriptExport]
public class Process : ObjectBase {
  private readonly System.Diagnostics.Process     process;
  private readonly ScriptEngine                   engine;
  private          TaskCompletionSource<object?>? exitedTcs;

  /// <summary>
  ///   The names of the process. For example: <c> "chrome" </c>.
  /// </summary>
  [ScriptMember("name")]
  public string Name { get; }

  /// <summary>
  ///   The full path to the process. For example:
  ///   <c> "C:\Program Files\Google\Chrome\Application\chrome.exe" </c>. This value will be
  ///   <see langword="null" /> if access to the process is denied due to permissions.
  /// </summary>
  [ScriptMember("fullPath")]
  public string? FullPath { get; }

  /// <summary>
  ///   The process ID, which is unique for each process. For example: <c> 1234 </c>.
  /// </summary>
  [ScriptMember("pid")]
  public int Pid { get; }

  /// <summary>
  ///   <para>
  ///     Specifies whether the process is protected. This is a special type of process that is
  ///     protected from being terminated or modified by other processes. This is used for processes
  ///     that are critical to the system, such as the Windows kernel and other system processes.
  ///   </para>
  ///   <para>
  ///     This property is useful if you need to check if a process is protected before attempting to
  ///     terminate it or modify it. If the process is protected, your script will need to run with
  ///     elevated privileges to modify it.
  ///   </para>
  /// </summary>
  /// <exception cref="InvalidOperationException">
  ///   If the process could not be opened.
  /// </exception>
  [ScriptMember("isProtected")]
  public unsafe bool IsProtected {
    get {
      var proc = OpenProcess(
        PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION,
        bInheritHandle: false,
        (uint)Pid
      );

      if (proc == 0) {
        throw new InvalidOperationException(
          $"Failed to open process {process.ProcessName} ({Pid})."
        );
      }

      _PsProtection processProtectionInfo = new();
      var           bytesReturned         = 0;

      // Use the internal NtQueryInformationProcess function to get the process protection
      // information.
      NtQueryInformationProcess(
        proc,
        PROCESSINFOCLASS.ProcessProtectionInformation,
        ref processProtectionInfo,
        sizeof(_PsProtection),
        ref bytesReturned
      );

      // Close the process handle.
      if (CloseHandle(proc) == 0) {
        throw new InvalidOperationException(
          $"Failed to close handle of process {process.ProcessName} ({Pid})."
        );
      }

      // If the process is not protected, the type will be PsProtectedTypeNone.
      return processProtectionInfo.Type != PsProtectedType.PsProtectedTypeNone;
    }
  }

  [ScriptMember("exited")]
  public Task Exited {
    get {
      // If there is already a TaskCompletionSource, return the task.
      if (exitedTcs != null) {
        return exitedTcs.Task;
      }

      // Otherwise, create a new TaskCompletionSource and register a wait for the process to exit.
      exitedTcs = new TaskCompletionSource<object?>();

      // Get the handle of the process.
      var handle = OpenProcess(
        PROCESS_ACCESS_RIGHTS.PROCESS_SYNCHRONIZE,
        bInheritHandle: false,
        (uint)Pid
      );
      if (handle == 0) {
        throw new InvalidOperationException(
          $"Failed to open process {process.ProcessName} ({Pid})."
        );
      }

      // Use the handle to register a wait for the process to exit.
      ThreadPool.RegisterWaitForSingleObject(
        new ManualResetEvent(false)
          { SafeWaitHandle = new SafeWaitHandle(handle, ownsHandle: true) },
        static (state, _) => ((TaskCompletionSource<object?>)state!).TrySetResult(null),
        exitedTcs,
        Timeout.Infinite,
        executeOnlyOnce: true
      );

      return exitedTcs.Task;
    }
  }


  internal Process(System.Diagnostics.Process process) {
    engine       = AppState.ScriptEngine;
    this.process = process;
    Name         = process.ProcessName;
    try {
      FullPath = process.MainModule?.FileName;
    }
    // Catch the Win32Exception with error code 5 (access denied) when trying to get the full path.
    catch (Win32Exception e) when (e.NativeErrorCode == 5) {
      FullPath = null;
    }

    Pid = process.Id;
  }


  /// <summary>
  ///   Forcefully terminates the process.
  /// </summary>
  [ScriptMember("kill")]
  public void Kill() {
    process.Kill();
  }


  /// <summary>
  ///   Lists the current child processes of the process.
  /// </summary>
  /// <returns> A list of child processes. </returns>
  [ScriptMember("listChildren")]
  public JSArray<Process> ListChildren() {
    return JSArray<Process>.FromIEnumerable(
      process.GetChildProcesses()
        .Select<System.Diagnostics.Process, Process>(
          proc =>
            new Process(proc)
        )
    );
  }


  /// <summary>
  ///   Sets the process affinity. The affinity can be one of the following:
  ///   <list type="bullet">
  ///     <item>
  ///       <c>"all"</c> - All processors.
  ///     </item>
  ///     <item>
  ///       <c>Array&lt;number&gt;</c> - An array of processor numbers. For example: <c>[0, 2, 4]</c>
  ///       will set the affinity to processors 0, 2, and 4. This is a 0-based index, so processor "0"
  ///       is the first processor.
  ///     </item>
  ///     <item>
  ///       <c>number</c> - A number of processors to use. For example: <c>2</c> will set the
  ///       affinity to 2 processors. "0" is an invalid value as the process must use at least
  ///       one processor. This range is sequential, so if you set the affinity to 3 processors,
  ///       processors 0, 1, and 2 will be used.
  ///     </item>
  ///   </list>
  /// </summary>
  /// <param name="affinity"> The affinity to set. </param>
  [ScriptMember("setAffinity")]
  public void SetAffinity(
    [TsTypeOverride(
      """ "all" """
    )]
    string affinity
  ) {
    // Check if the affinity is "all".
    if (affinity.ToLower() is not "all") {
      throw new ArgumentException(
        $"Invalid affinity. Expected string value to be \"all\", but got \"{affinity}\"."
      );
    }

    // Get the number of processors on the system.;
    GetSystemInfo(out var systemInfo);
    var numProcessors = systemInfo.dwNumberOfProcessors;

    // Set the process affinity to all processors based on the number of processors.
    SetAffinity((int)numProcessors);
  }


  /// <inheritdoc cref="SetAffinity(string)" />
  [ScriptMember("setAffinity")]
  public void SetAffinity(
    IEnumerable<int> affinity
  ) {
    // Convert the array of integers to a bitmask. The bitmask is a 32-bit integer, so we can only
    // use the first 32 processors.
    var mask = 0;

    // For each processor in the array, set the corresponding bit in the bitmask.
    foreach (var i in affinity) {
      mask |= 1 << i;
    }

    // Open the process with the specified access rights.
    var proc = OpenProcess(
      PROCESS_ACCESS_RIGHTS.PROCESS_SET_INFORMATION,
      bInheritHandle: false,
      (uint)Pid
    );

    if (proc == 0) {
      throw new InvalidOperationException(
        $"Failed to open process {process.ProcessName} ({Pid})."
      );
    }

    // Set the process affinity to the bitmask.
    if (SetProcessAffinityMask(proc, (nuint)mask) == 0) {
      var formattedBitMask = IntToBinaryString(mask);
      throw new InvalidOperationException(
        $"Failed to set affinity of process {process.ProcessName} ({Pid}) to {formattedBitMask}."
      );
    }

    // Close the process handle.
    if (CloseHandle(proc) == 0) {
      throw new InvalidOperationException(
        $"Failed to close handle of process {process.ProcessName} ({Pid})."
      );
    }
  }


  /// <inheritdoc cref="SetAffinity(string)" />
  [ScriptMember("setAffinity")]
  public void SetAffinity(
    int affinity
  ) {
    // The affinity parameter presents a number of processors to use, starting from the first one.
    // For example, if you set the affinity to 3, the process will use processors 0, 1, and 2.
    // The bitmask is a 32-bit integer, so we can only use the first 32 processors.
    var mask = 0;

    // For each processor, set the corresponding bit in the bitmask.
    for (var i = 0; i < affinity; i++) {
      mask |= 1 << i;
    }

    // Open the process with the specified access rights.
    var proc = OpenProcess(
      PROCESS_ACCESS_RIGHTS.PROCESS_SET_INFORMATION,
      bInheritHandle: false,
      (uint)Pid
    );

    if (proc == 0) {
      throw new InvalidOperationException(
        $"Failed to open process {process.ProcessName} ({Pid})."
      );
    }

    // Set the process affinity to the bitmask.
    if (SetProcessAffinityMask(proc, (nuint)mask) == 0) {
      var formattedBitMask = IntToBinaryString(mask);
      throw new InvalidOperationException(
        $"Failed to set affinity of process {process.ProcessName} ({Pid}) to {formattedBitMask}."
      );
    }

    // Close the process handle.
    if (CloseHandle(proc) == 0) {
      throw new InvalidOperationException(
        $"Failed to close handle of process {process.ProcessName} ({Pid})."
      );
    }
  }


  [HideFromTypeScript]
  [ScriptMember("setAffinity")]
  public void SetAffinity(
    ScriptObject affinity
  ) {
    if (IsArray(affinity)) {
      SetAffinity(((IEnumerable)affinity).Cast<int>());
      return;
    }

    throw new ArgumentException(
      $"Invalid affinity. Expected an array of integers, but got \"{GetJSType(affinity)}\"."
    );
  }


  /// <summary>
  ///   Sets the priority of the process. The priority can be one of the following:
  ///   <list type="bullet">
  ///     <item>
  ///       <c>"idle"</c> - Process whose threads run only when the system is idle. The threads of the
  ///       process are preempted by the threads of any process running in a higher priority class. An
  ///       example is a screen saver. The idle-priority class is inherited by child processes.
  ///     </item>
  ///     <item>
  ///       <c>"below normal"</c> - Process whose threads run at a lower priority than normal but
  ///       higher than idle.
  ///     </item>
  ///     <item>
  ///       <c>"normal"</c> - Process with no special scheduling needs and whose threads run at normal
  ///       priority. This is the default priority class for a process.
  ///     </item>
  ///     <item>
  ///       <c>"above normal"</c> - Process whose threads run at a higher priority than normal but
  ///       lower than high.
  ///     </item>
  ///     <item>
  ///       <c>"high"</c> - Process that performs time-critical tasks that must be executed
  ///       immediately. The threads of the process preempt the threads of normal or idle priority
  ///       class processes. An example is the Task List, which must respond quickly when called by
  ///       the user, regardless of the load on the operating system. Use extreme care when using the
  ///       high-priority class, because a high-priority class application can use nearly all
  ///       available CPU time.
  ///     </item>
  ///     <item>
  ///       <c>"realtime"</c> - Process that has the highest possible priority. The threads of the
  ///       process preempt the threads of all other processes, including operating system processes
  ///       performing important tasks. For example, a real-time process that executes for more than a
  ///       very brief interval can cause disk caches not to flush or cause the mouse to be
  ///       unresponsive.
  ///     </item>
  ///   </list>
  /// </summary>
  /// <param name="priority"> The priority to set. </param>
  [ScriptMember("setPriority")]
  public void SetPriority(
    [TsTypeOverride(
      """ "idle" | "below normal" | "normal" | "above normal" | "high" | "realtime" """
    )]
    string priority
  ) {
    var procPriority = priority.ToLower() switch {
      "idle" => PROCESS_CREATION_FLAGS.IDLE_PRIORITY_CLASS,
      "below normal" => PROCESS_CREATION_FLAGS.BELOW_NORMAL_PRIORITY_CLASS,
      "normal" => PROCESS_CREATION_FLAGS.NORMAL_PRIORITY_CLASS,
      "above normal" => PROCESS_CREATION_FLAGS.ABOVE_NORMAL_PRIORITY_CLASS,
      "high" => PROCESS_CREATION_FLAGS.HIGH_PRIORITY_CLASS,
      "realtime" => PROCESS_CREATION_FLAGS.REALTIME_PRIORITY_CLASS,
      _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, message: null)
    };
    // Open the process with the specified access rights.
    var proc = OpenProcess(
      PROCESS_ACCESS_RIGHTS.PROCESS_SET_INFORMATION,
      bInheritHandle: false,
      (uint)Pid
    );

    if (proc == 0) {
      throw new InvalidOperationException(
        $"Failed to open process {process.ProcessName} ({Pid})."
      );
    }

    if (SetPriorityClass(proc, procPriority) == 0) {
      throw new InvalidOperationException(
        $"Failed to set priority of process {process.ProcessName} ({Pid})."
      );
    }

    // Close the process handle.
    if (CloseHandle(proc) == 0) {
      throw new InvalidOperationException(
        $"Failed to close handle of process {process.ProcessName} ({Pid})."
      );
    }
  }


  /// <summary>
  ///   Creates a new instance of the <see cref="Process" /> class from the specified process ID.
  /// </summary>
  /// <param name="pid"> The process ID of the process to create. </param>
  /// <returns> A new instance of the <see cref="Process" /> class. </returns>
  internal static Process FromID(
    int pid
  ) {
    var process = System.Diagnostics.Process.GetProcessById(pid);
    return new Process(process);
  }


  /// <summary>
  ///   Converts an integer to a binary string. For instance
  ///   <c> "00000000_00000000_00000000_00000001" </c>
  /// </summary>
  /// <param name="value"> The integer to convert. </param>
  /// <returns> The binary string representation of the integer. </returns>
  private static string IntToBinaryString(
    int value
  ) {
    // Get the size of the integer in bytes.
    var size = sizeof(int);

    // Convert the integer to a binary string and pad it with leading zeros to make it 32 bits long.
    var binary = Convert.ToString(value, toBase: 2).PadLeft(size * 8, paddingChar: '0');

    // Partition the binary string into groups of 8 bits.
    var groups = Enumerable.Range(start: 0, size)
      .Select(i => binary.Substring(i * 8, length: 8));

    // Join the groups with underscores.
    return string.Join("_", groups);
  }
}
