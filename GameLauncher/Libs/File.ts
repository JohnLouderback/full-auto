import { DirectoryEntry } from "./DirectoryEntry";
import { ReadBytesCallback } from "./ReadBytesCallback";
import { ReadLinesCallback } from "./ReadLinesCallback";

// Auto-generated from C# type File
export interface File extends DirectoryEntry {
    /**
     * Reads the entire content of the file as a byte array asynchronously.
     *
     * @returns A promise that resolves when the file is fully read, returning a
     * `Uint8Array` containing the file's bytes.
     */
    readAllBytes(): Promise<Uint8Array>;
    /**
     * Reads the content of the file assuming it is a text file and returns it as
     * a string.
     *
     */
    readAllText(): Promise<string>;
    /**
     * Asynchronously reads the file byte by byte and invokes the specified
     * callback for each byte. This method is useful for processing binary files
     * or when you need to handle each byte individually, such as for custom
     * parsing or processing of binary data.
     *
     * @param callback A callback to be called for each byte read from the file.
     * The callback receives a single byte as an argument, which represents the
     * current byte read from the file.
     */
    readBytes(callback: ReadBytesCallback): Promise<void>;
    /**
     * Asynchronously reads the file line by line and invokes the specified
     * callback for each line.
     *
     * @param callback A callback to be called for each line of text.
     */
    readLines(callback: ReadLinesCallback): Promise<void>;
}
