import { DirectoryEntry } from "./DirectoryEntry";
import { File } from "./File";

/**
 * Represents a directory in the file system. A directory is a container that
 * can hold files and other directories. It can be used to organize files in a
 * hierarchical structure.
 *
 */
// Auto-generated from C# type Directory
export interface Directory extends DirectoryEntry {
    /**
     * Retrieves all entries (files and directories) within this directory.
     *
     */
    readonly entries: Array<DirectoryEntry>;
    /**
     * Retrieves a list of all subdirectories within this directory.
     *
     */
    readonly subDirs: Array<Directory>;
    /**
     * Retrieves a list of all files within this directory.
     *
     */
    readonly files: Array<File>;
}
