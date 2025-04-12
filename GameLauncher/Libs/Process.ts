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
     * Files\Google\Chrome\Application\chrome.exe"`.
     *
     */
    readonly fullPath: string;
    /**
     * The process ID, which is unique for each process. For example: `1234`.
     *
     */
    readonly pid: number;
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
}
