import { Directory } from "./Directory";
import { File } from "./File";

/**
 * Represents a directory entry in the file system. A directory entry either
 * is a file or a folder within a directory.
 *
 */
// Auto-generated from C# type DirectoryEntry
export interface DirectoryEntry {
    readonly path: string;
    /**
     * Checks if this directory entry is a directory.
     *
     * @returns Returns `true` if this entry is a directory; otherwise, `false` .
     */
    isDirectory(): this is Directory;
    /**
     * Checks if this directory entry is a file.
     *
     * @returns Returns `true` if this entry is a file; otherwise, `false`.
     */
    isFile(): this is File;
}
