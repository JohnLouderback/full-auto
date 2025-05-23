/**
 * Represents a process that is or was running on the system.
 *
 */
// Auto-generated from C# type Process
export interface Process {
    /**
     * The names of the process. For example: `"chrome"`.
     *
     */
    readonly name: string;
    /**
     * The full path to the process. For example: `"C:\Program
     * Files\Google\Chrome\Application\chrome.exe"`. This value will be `null` if
     * access to the process is denied due to permissions.
     *
     */
    readonly fullPath?: string;
    /**
     * The process ID, which is unique for each process. For example: `1234`.
     *
     */
    readonly pid: number;
    /**
     * Specifies whether the process is protected. This is a special type of
     * process that is protected from being terminated or modified by other
     * processes. This is used for processes that are critical to the system, such
     * as the Windows kernel and other system processes.
     * 
     * This property is useful if you need to check if a process is protected
     * before attempting to terminate it or modify it. If the process is
     * protected, your script will need to run with elevated privileges to modify
     * it.
     *
     */
    readonly isProtected: boolean;
    readonly exited: Promise<void>;
    /**
     * Forcefully terminates the process.
     *
     */
    kill(): void;
    /**
     * Lists the current child processes of the process.
     *
     * @returns A list of child processes.
     */
    listChildren(): Array<Process>;
    /**
     * Sets the process affinity. The affinity can be one of the following:
     * - `"all"` - All processors.
     * - `Array<number>` - An array of processor numbers. For example: `[0, 2, 4]`
     * will set the affinity to processors 0, 2, and 4. This is a 0-based index,
     * so processor "0" is the first processor.
     * - `number` - A number of processors to use. For example: `2` will set the
     * affinity to 2 processors. "0" is an invalid value as the process must use
     * at least one processor. This range is sequential, so if you set the
     * affinity to 3 processors, processors 0, 1, and 2 will be used.
     *
     * @param affinity The affinity to set.
     */
    setAffinity(affinity: "all"): void;
    /**
     * Sets the process affinity. The affinity can be one of the following:
     * - `"all"` - All processors.
     * - `Array<number>` - An array of processor numbers. For example: `[0, 2, 4]`
     * will set the affinity to processors 0, 2, and 4. This is a 0-based index,
     * so processor "0" is the first processor.
     * - `number` - A number of processors to use. For example: `2` will set the
     * affinity to 2 processors. "0" is an invalid value as the process must use
     * at least one processor. This range is sequential, so if you set the
     * affinity to 3 processors, processors 0, 1, and 2 will be used.
     *
     * @param affinity The affinity to set.
     */
    setAffinity(affinity: Array<number>): void;
    /**
     * Sets the process affinity. The affinity can be one of the following:
     * - `"all"` - All processors.
     * - `Array<number>` - An array of processor numbers. For example: `[0, 2, 4]`
     * will set the affinity to processors 0, 2, and 4. This is a 0-based index,
     * so processor "0" is the first processor.
     * - `number` - A number of processors to use. For example: `2` will set the
     * affinity to 2 processors. "0" is an invalid value as the process must use
     * at least one processor. This range is sequential, so if you set the
     * affinity to 3 processors, processors 0, 1, and 2 will be used.
     *
     * @param affinity The affinity to set.
     */
    setAffinity(affinity: number): void;
    /**
     * Sets the priority of the process. The priority can be one of the following:
     * - `"idle"` - Process whose threads run only when the system is idle. The
     * threads of the process are preempted by the threads of any process running
     * in a higher priority class. An example is a screen saver. The idle-priority
     * class is inherited by child processes.
     * - `"below normal"` - Process whose threads run at a lower priority than
     * normal but higher than idle.
     * - `"normal"` - Process with no special scheduling needs and whose threads
     * run at normal priority. This is the default priority class for a process.
     * - `"above normal"` - Process whose threads run at a higher priority than
     * normal but lower than high.
     * - `"high"` - Process that performs time-critical tasks that must be
     * executed immediately. The threads of the process preempt the threads of
     * normal or idle priority class processes. An example is the Task List, which
     * must respond quickly when called by the user, regardless of the load on the
     * operating system. Use extreme care when using the high-priority class,
     * because a high-priority class application can use nearly all available CPU
     * time.
     * - `"realtime"` - Process that has the highest possible priority. The
     * threads of the process preempt the threads of all other processes,
     * including operating system processes performing important tasks. For
     * example, a real-time process that executes for more than a very brief
     * interval can cause disk caches not to flush or cause the mouse to be
     * unresponsive.
     *
     * @param priority The priority to set.
     */
    setPriority(priority: "idle" | "below normal" | "normal" | "above normal" | "high" | "realtime"): void;
}
