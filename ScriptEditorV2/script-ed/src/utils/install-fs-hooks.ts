import { EventEmitter } from "events";
import * as fs from "fs";
import * as path from "path";
import { promisify } from "util";

import { Directory, File } from "./vfs/schema";
import * as vfs from "./vfs/utils";

(() => {
  if ((globalThis as Record<string, unknown>).fsHooksInstalled) {
    console.log("fs hooks already installed");
    return;
  }

  console.log("installing fs hooks");

  type Writeable<T extends { [x: string]: any }, K extends string> = {
    [P in K]: T[P];
  };

  const fsProxy = fs as Writeable<typeof fs, keyof typeof fs>;

  // const fsPromisesProxy = fs.promises as Writeable<
  //   typeof fs.promises,
  //   keyof typeof fs.promises
  // >;

  // Helper to wrap asynchronous functions while preserving __promisify__.

  function copyProperties<T extends Function>(original: T, wrapper: T): void {
    // Get both string and symbol properties.
    const propNames = Object.getOwnPropertyNames(original);
    const propSymbols = Object.getOwnPropertySymbols(original);
    for (const key of [...propNames, ...propSymbols]) {
      // Skip the "length", "name", and "prototype" properties if desired.
      if (["length", "name", "prototype"].includes(key as string)) continue;
      const descriptor = Object.getOwnPropertyDescriptor(original, key);
      if (descriptor) {
        // Ensure we can rewrite properties (e.g., they're not read-only).
        descriptor.writable = true;
        Object.defineProperty(wrapper, key, descriptor);
      }
    }
  }

  function wrapAsyncMethod<T extends Function>(original: T, wrapper: T): T {
    // Copy all own properties from original to wrapper
    copyProperties(original, wrapper);
    // Also copy the promisify-related properties explicitly (if not already copied)
    // (wrapper as any).__promisify__ = (original as any).__promisify__;
    // (wrapper as any)[promisify.custom] = (original as any)[promisify.custom];
    return wrapper;
  }

  // --- Synchronous functions (unchanged) ---

  const oldReadFileSync = fsProxy.readFileSync;
  // @ts-expect-error
  fsProxy.readFileSync = (...args) => {
    if (args.length > 0 && typeof args[0] === "string") {
      const filePath = args[0] as string;
      const fileText = vfs.getFileText(filePath);
      if (fileText !== undefined) {
        console.log(`Returning virtual file text for: ${filePath}`);
        return fileText;
      }
    }
    return oldReadFileSync(...args);
  };

  const oldReaddirSync = fsProxy.readdirSync;
  // @ts-expect-error
  fsProxy.readdirSync = (...args) => {
    if (args.length > 0 && typeof args[0] === "string") {
      const dirPath = args[0] as string;
      const contents = vfs.readDirectory(dirPath);
      if (contents !== undefined) {
        console.log(`Returning virtual directory contents for: ${dirPath}`);
        // If the options object has withFileTypes set to true, we should return an array of fs.Dirent objects.
        if (
          args[1] &&
          "withFileTypes" in (args[1] as Record<string, unknown>) &&
          (args[1] as Record<string, unknown>).withFileTypes
        ) {
          const dirEnts = contents.map(
            (name) => new VFSPathDirEnt(path.join(dirPath, name))
          );
          console.log(
            JSON.stringify(
              dirEnts.map((dirent) => {
                return {
                  name: dirent.name,
                  path: dirent.path,
                  isFile: dirent.isFile(),
                  isDirectory: dirent.isDirectory(),
                };
              }),
              null,
              2
            )
          );
          return dirEnts;
        }
        console.log(JSON.stringify(contents, null, 2));
        return contents;
      }
      // Because the VFS is relative to every real directory, we should always concatenate
      // the real directory contents with the virtual directory contents.
      // @ts-expect-error
      const realContents = oldReaddirSync(...args);
      const virtualContents = vfs.readDirectory("/");
      console.log(
        `Returning combined real and virtual directory contents for: ${dirPath}`
      );
      if (virtualContents !== undefined) {
        // If the options object has withFileTypes set to true, we should return an array of fs.Dirent objects.
        if (
          args[1] &&
          "withFileTypes" in (args[1] as Record<string, unknown>) &&
          (args[1] as Record<string, unknown>).withFileTypes
        ) {
          const dirEnts = realContents.concat(
            virtualContents.map((name) => new VFSPathDirEnt(name))
          );
          console.log(
            JSON.stringify(
              dirEnts.map((dirent) => {
                return {
                  name: dirent.name,

                  path: dirent.path,
                  isFile: dirent.isFile(),
                  isDirectory: dirent.isDirectory(),
                };
              }),
              null,
              2
            )
          );
          return dirEnts;
        }

        // Otherwise, just return the names as strings.
        const combinedContents = realContents.concat(virtualContents);
        console.log(JSON.stringify(combinedContents, null, 2));
        return combinedContents;
      }
    }
    // @ts-expect-error
    return oldReaddirSync(...args);
  };

  function fileStatFromEntry(entry: File | Directory): fs.Stats {
    const now = new Date();
    const nowMs = now.getTime();
    return {
      isFile: () => vfs.isFile(entry),
      isDirectory: () => vfs.isDirectory(entry),
      isBlockDevice() {
        return false;
      },
      isCharacterDevice() {
        return false;
      },
      isSymbolicLink() {
        return false;
      },
      isFIFO() {
        return false;
      },
      isSocket() {
        return false;
      },

      dev: 0,
      ino: 0,
      mode: 0,
      nlink: 0,
      uid: 0,
      gid: 0,
      rdev: 0,
      size: 0,
      blksize: 0,
      blocks: 0,
      atime: now,
      mtime: now,
      ctime: now,
      birthtime: now,
      atimeMs: nowMs,
      mtimeMs: nowMs,
      ctimeMs: nowMs,
      birthtimeMs: nowMs,
    };
  }

  /**
   * A virtual file system path directory entry.
   */
  class VFSPathDirEnt extends fs.Dirent {
    constructor(public absPath: string) {
      super();
      this.name = path.basename(absPath);
      this.path = path.dirname(path.resolve(absPath));
      this.parentPath = this.path;
    }

    public override isFile() {
      return vfs.isFile(vfs.getEntry(this.absPath));
    }

    public override isDirectory() {
      return vfs.isDirectory(vfs.getEntry(this.absPath));
    }

    public override isBlockDevice() {
      return false;
    }

    public override isCharacterDevice() {
      return false;
    }

    public override isSymbolicLink() {
      return false;
    }

    public override isFIFO() {
      return false;
    }

    public override isSocket() {
      return false;
    }
  }

  const oldStatSync = fsProxy.statSync;
  // @ts-expect-error
  fsProxy.statSync = (...args) => {
    if (args.length > 0 && typeof args[0] === "string") {
      const filePath = args[0] as string;
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        console.log(`Returning virtual file stats for: ${filePath}`);
        return fileStatFromEntry(entry);
      }
    }
    return oldStatSync(...args);
  };

  const oldExistsSync = fsProxy.existsSync;
  fsProxy.existsSync = (...args) => {
    if (args.length > 0 && typeof args[0] === "string") {
      const filePath = args[0] as string;
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        console.log(`Returning virtual file exists for: ${filePath}`);
        return true;
      }
    }
    return oldExistsSync(...args);
  };

  /**
   * A no-op file watcher that implements fs.FSWatcher in a robust way.
   * It inherits from EventEmitter so any .on(...), .once(...), etc.
   * calls won't crash consumer code, but it never actually emits events.
   */
  class NoOpFileWatcher extends EventEmitter implements fs.FSWatcher {
    public constructor() {
      super();
    }

    /**
     * Closes the watcher. Since this is a no-op watcher, there's nothing to close.
     */
    public close(): void {
      // No-op
    }

    /**
     * The fs.FSWatcher interface requires ref() and unref(), which typically
     * control the watcher’s event loop behavior. Here, we just return `this`.
     */
    public ref(): this {
      return this;
    }

    public unref(): this {
      return this;
    }
  }

  const oldWatch = fsProxy.watch;

  fsProxy.watch = (...args) => {
    if (args.length > 0 && typeof args[0] === "string") {
      const filePath = args[0] as string;
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        console.log(`Returning virtual watcher for: ${filePath}`);
        return new NoOpFileWatcher();
      }
    }
    // @ts-expect-error
    return oldWatch(...args);
  };

  const oldWatchFile = fsProxy.watchFile;
  // @ts-expect-error
  fsProxy.watchFile = (...args) => {
    if (args.length > 0 && typeof args[0] === "string") {
      const filePath = args[0] as string;
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        console.log(`Returning virtual file watcher for: ${filePath}`);
        return new NoOpFileWatcher();
      }
    }
    // @ts-expect-error
    return oldWatchFile(...args);
  };

  const oldUnwatchFile = fsProxy.unwatchFile;
  fsProxy.unwatchFile = (...args) => {
    if (args.length > 0 && typeof args[0] === "string") {
      const filePath = args[0] as string;
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        console.log(`Unwatching virtual file: ${filePath}`);
        return;
      }
    }
    // @ts-expect-error
    return oldUnwatchFile(...args);
  };

  // --- Virtual File Descriptor management ---
  let nextVirtualFd = 10000;
  const virtualFdMap = new Map<
    number,
    { entry: File | Directory; pos: number; path: string }
  >();

  // --- Asynchronous functions (wrapped with __promisify__ preservation) ---

  // mkdir (no-op virtual paths)
  // const oldMkdir = fsProxy.mkdir;
  // fsProxy.mkdir = wrapAsyncMethod(oldMkdir, function (
  //   dirPath: string,
  //   optionsOrCallback?: any,
  //   maybeCallback?: any
  // ) {
  //   if (typeof dirPath === "string" && vfs.getEntry(dirPath) !== undefined) {
  //     console.log(`Attempted mkdir on readonly VFS for: ${dirPath}`);
  //     const cb =
  //       typeof optionsOrCallback === "function"
  //         ? optionsOrCallback
  //         : maybeCallback;
  //     if (cb) {
  //       process.nextTick(() => cb(new Error("Readonly file system")));
  //       return;
  //     }
  //     throw new Error("Readonly file system");
  //   }
  //   return oldMkdir.apply(fsProxy, arguments);
  // } as typeof fs.mkdir);

  // open (only allow read-only)
  const oldOpen = fsProxy.open;
  fsProxy.open = wrapAsyncMethod(oldOpen, function (
    filePath: string,
    flags: string,
    modeOrCallback?: any,
    maybeCallback?: any
  ) {
    if (typeof filePath === "string") {
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        if (flags !== "r" && flags !== "rs") {
          console.log(
            `Attempted open for writing on readonly VFS: ${filePath}`
          );
          const cb =
            typeof modeOrCallback === "function"
              ? modeOrCallback
              : maybeCallback;
          process.nextTick(() => cb(new Error("Readonly file system")));
          return;
        }
        console.log(`Returning virtual open for readonly file: ${filePath}`);
        const cb =
          typeof modeOrCallback === "function" ? modeOrCallback : maybeCallback;
        const fd = nextVirtualFd++;
        virtualFdMap.set(fd, { entry, pos: 0, path: filePath });
        process.nextTick(() => cb(null, fd));
        return;
      }
    }
    return oldOpen.apply(fsProxy, arguments);
  } as typeof fs.open);

  // close
  const oldClose = fsProxy.close;
  fsProxy.close = wrapAsyncMethod(oldClose, function (
    fd: number,
    callback: Function
  ) {
    if (virtualFdMap.has(fd)) {
      console.log(`Returning virtual close for fd: ${fd}`);
      virtualFdMap.delete(fd);
      process.nextTick(() => callback(null));
      return;
    }
    return oldClose.apply(fsProxy, arguments);
  } as typeof fs.close);

  // read
  const oldRead = fsProxy.read;
  fsProxy.read = wrapAsyncMethod(oldRead, function (
    fd: number,
    buffer: Buffer,
    offset: number,
    length: number,
    position: number | null,
    callback: Function
  ) {
    if (virtualFdMap.has(fd)) {
      console.log(`Returning virtual read for fd: ${fd}`);
      const vfd = virtualFdMap.get(fd)!;
      if (vfs.isFile(vfd.entry)) {
        const fileText = vfs.getFileText(vfd.path) || "";
        const data = Buffer.from(fileText);
        const readPos = position === null ? vfd.pos : position;
        const bytesAvailable = data.length - readPos;
        const bytesToRead = Math.min(length, bytesAvailable);
        data.copy(buffer, offset, readPos, readPos + bytesToRead);
        if (position === null) {
          vfd.pos += bytesToRead;
        }
        process.nextTick(() => callback(null, bytesToRead, buffer));
        return;
      } else {
        process.nextTick(() => callback(new Error("Not a file")));
        return;
      }
    }
    return oldRead.apply(fsProxy, arguments);
  } as typeof fs.read);

  // write (no-op on readonly VFS)
  const oldWrite = fsProxy.write;
  fsProxy.write = wrapAsyncMethod(oldWrite, function (
    fd: number,
    buffer: Buffer | string,
    offsetOrEncoding?: any,
    lengthOrCallback?: any,
    positionOrCallback?: any,
    maybeCallback?: any
  ) {
    if (virtualFdMap.has(fd)) {
      console.log(`Attempted virtual write on readonly VFS for fd: ${fd}`);
      const cb =
        typeof maybeCallback === "function"
          ? maybeCallback
          : typeof positionOrCallback === "function"
          ? positionOrCallback
          : lengthOrCallback;
      // Writing to files is not supported in the VFS and is a no-op as such.
      process.nextTick(() => cb(null));
      return;
    }
    return oldWrite.apply(fsProxy, arguments);
  } as typeof fs.write);

  // fdatasync
  const oldFdatasync = fsProxy.fdatasync;
  fsProxy.fdatasync = wrapAsyncMethod(oldFdatasync, function (
    fd: number,
    callback: Function
  ) {
    if (virtualFdMap.has(fd)) {
      console.log(`Returning virtual fdatasync for fd: ${fd}`);
      process.nextTick(() => callback(null));
      return;
    }
    return oldFdatasync.apply(fsProxy, arguments);
  } as typeof fs.fdatasync);

  // lstat
  const oldLstat = fsProxy.lstat;
  fsProxy.lstat = wrapAsyncMethod(oldLstat, function lstatHook(
    this: typeof fs,
    filePath: string,
    optionsOrCallback?: any,
    maybeCallback?: any
  ) {
    // 1) Identify callback vs. options
    let callback: Function | undefined;
    let options: Record<string, unknown> = {};
    if (typeof optionsOrCallback === "function") {
      callback = optionsOrCallback;
    } else {
      options = optionsOrCallback || {};
      callback = maybeCallback;
    }

    // 2) If no callback is given, delegate to the custom promisified version
    if (typeof callback !== "function") {
      return (fsProxy.lstat as any)[promisify.custom].call(
        this,
        filePath,
        options
      );
    }

    // 3) If a callback is provided, do the existing "check VFS or fallback" logic
    if (typeof filePath === "string") {
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        console.log(`Returning virtual lstat for: ${filePath}`);
        const stats = fileStatFromEntry(entry);
        process.nextTick(() => callback(null, stats));
        return;
      }
    }

    return oldLstat.apply(fsProxy, arguments);
  } as typeof fs.lstat);

  // stat
  const oldStat = fsProxy.stat;
  fsProxy.stat = wrapAsyncMethod(oldStat, function statHook(
    this: typeof fs,
    filePath: string,
    optionsOrCallback?: any,
    maybeCallback?: any
  ) {
    let callback: Function | undefined;
    let options: Record<string, unknown> = {};
    if (typeof optionsOrCallback === "function") {
      callback = optionsOrCallback;
    } else {
      options = optionsOrCallback || {};
      callback = maybeCallback;
    }

    if (typeof callback !== "function") {
      // No callback => use promisified approach
      return (fsProxy.stat as any)[promisify.custom].call(
        this,
        filePath,
        options
      );
    }

    if (typeof filePath === "string") {
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        console.log(`Returning virtual stat for: ${filePath}`);
        const stats = fileStatFromEntry(entry);
        process.nextTick(() => callback(null, stats));
        return;
      }
    }
    return oldStat.apply(fsProxy, arguments);
  } as typeof fs.stat);

  // readdir
  const oldReaddir = fsProxy.readdir;
  fsProxy.readdir = wrapAsyncMethod(oldReaddir, function (
    this: typeof fs,
    dirPath: string,
    optionsOrCallback: any,
    maybeCallback?: any
  ) {
    let callback: Function | undefined;
    let options: any = {};
    if (typeof optionsOrCallback === "function") {
      callback = optionsOrCallback;
    } else {
      options = optionsOrCallback || {};
      callback = maybeCallback;
    }

    // If no callback is provided, delegate to the promisified implementation.
    if (typeof callback !== "function") {
      return (fsProxy.readdir as any)[promisify.custom].call(
        this,
        dirPath,
        options
      );
    }

    if (typeof dirPath === "string") {
      const virtualContents = vfs.readDirectory(dirPath);
      if (virtualContents !== undefined) {
        console.log(`Returning virtual directory contents for: ${dirPath}`);
        if (options.withFileTypes) {
          const dirents = virtualContents.map((name) => {
            return new VFSPathDirEnt(path.join(dirPath, name));
          });
          console.log(
            JSON.stringify(
              dirents.map((dirent) => {
                return {
                  name: dirent.name,
                  path: dirent.path,
                  isFile: dirent.isFile(),
                  isDirectory: dirent.isDirectory(),
                };
              }),
              null,
              2
            )
          );
          process.nextTick(() => callback(null, dirents));
        } else {
          console.log(JSON.stringify(virtualContents, null, 2));
          process.nextTick(() => callback(null, virtualContents));
        }
        return;
      }

      // Fallback: call the original readdir, then merge in virtual contents from "/"
      return oldReaddir.call(
        fsProxy,
        dirPath,
        options,
        function (
          err: NodeJS.ErrnoException | null,
          realContents: string[] | fs.Dirent[]
        ) {
          if (err) {
            return callback(err);
          }
          const virtualRootContents = vfs.readDirectory("/");
          if (virtualRootContents !== undefined) {
            if (options.withFileTypes) {
              const realDirents = realContents as fs.Dirent[];
              const virtualDirents = virtualRootContents.map(
                (name) => new VFSPathDirEnt(name)
              );
              const combinedDirents = realDirents.concat(virtualDirents);
              console.log(
                JSON.stringify(
                  combinedDirents.map((dirent) => {
                    return {
                      name: dirent.name,

                      path: dirent.path,
                      isFile: dirent.isFile(),
                      isDirectory: dirent.isDirectory(),
                    };
                  }),
                  null,
                  2
                )
              );
              return callback(null, combinedDirents);
            } else {
              const combinedContents = realContents.concat(virtualRootContents);
              console.log(JSON.stringify(combinedContents, null, 2));
              return callback(null, combinedContents as string[]);
            }
          }
          return callback(null, realContents);
        }
      );
    }
    return oldReaddir.apply(fsProxy, arguments);
  } as typeof fs.readdir);

  // readFile
  const oldReadFile = fsProxy.readFile;
  fsProxy.readFile = wrapAsyncMethod(oldReadFile, function (
    filePath: string | Buffer,
    optionsOrCallback: any,
    maybeCallback?: any
  ) {
    if (typeof filePath === "string") {
      const fileText = vfs.getFileText(filePath);
      if (fileText !== undefined) {
        console.log(`Returning virtual readFile for: ${filePath}`);
        const data = Buffer.from(fileText);
        if (typeof optionsOrCallback === "function") {
          process.nextTick(() => optionsOrCallback(null, data));
        } else if (typeof maybeCallback === "function") {
          process.nextTick(() => maybeCallback(null, data));
        }
        return;
      }
    }
    return oldReadFile.apply(fsProxy, arguments);
  } as typeof fs.readFile);

  // exists
  const oldExists = fsProxy.exists;
  fsProxy.exists = wrapAsyncMethod(oldExists, function (
    filePath: string,
    callback: Function
  ) {
    if (typeof filePath === "string") {
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        console.log(`Returning virtual exists for: ${filePath}`);
        process.nextTick(() => callback(true));
        return;
      }
    }
    return oldExists.apply(fsProxy, arguments);
  } as typeof fs.exists);

  // chmod (no-op on readonly VFS)
  const oldChmod = fsProxy.chmod;
  fsProxy.chmod = wrapAsyncMethod(oldChmod, function (
    filePath: string,
    mode: any,
    callback: Function
  ) {
    if (typeof filePath === "string" && vfs.getEntry(filePath) !== undefined) {
      console.log(`Attempted virtual chmod on readonly VFS for: ${filePath}`);
      // Changing file permissions is not supported in the VFS and is a no-op as such.
      process.nextTick(() => callback(null));
      return;
    }
    return oldChmod.apply(fsProxy, arguments);
  } as typeof fs.chmod);

  // rmdir (no-op on readonly VFS)
  const oldRmdir = fsProxy.rmdir;
  fsProxy.rmdir = wrapAsyncMethod(oldRmdir, function (
    dirPath: string,
    optionsOrCallback: any,
    maybeCallback?: any
  ) {
    if (typeof dirPath === "string" && vfs.getEntry(dirPath) !== undefined) {
      console.log(`Attempted virtual rmdir on readonly VFS for: ${dirPath}`);
      const cb =
        typeof optionsOrCallback === "function"
          ? optionsOrCallback
          : maybeCallback;
      // Removing directories is not supported in the VFS and is a no-op as such.
      process.nextTick(() => cb(null));
      return;
    }
    return oldRmdir.apply(fsProxy, arguments);
  } as typeof fs.rmdir);

  // unlink (no-op on readonly VFS)
  const oldUnlink = fsProxy.unlink;
  fsProxy.unlink = wrapAsyncMethod(oldUnlink, function (
    filePath: string,
    callback: Function
  ) {
    if (typeof filePath === "string" && vfs.getEntry(filePath) !== undefined) {
      console.log(`Attempted virtual unlink on readonly VFS for: ${filePath}`);
      // Removing files is not supported in the VFS and is a no-op as such.
      process.nextTick(() => callback(null));
      return;
    }
    return oldUnlink.apply(fsProxy, arguments);
  } as typeof fs.unlink);

  // rename (no-op readonly VFS)
  const oldRename = fsProxy.rename;
  fsProxy.rename = wrapAsyncMethod(oldRename, function (
    oldPath: string,
    newPath: string,
    callback: Function
  ) {
    if (
      typeof oldPath === "string" &&
      typeof newPath === "string" &&
      (vfs.getEntry(oldPath) !== undefined ||
        vfs.getEntry(newPath) !== undefined)
    ) {
      console.log(
        `Attempted virtual rename on readonly VFS from: ${oldPath} to: ${newPath}`
      );
      // Renaming files is not supported in the VFS and is a no-op as such.
      process.nextTick(() => callback(null));
      return;
    }
    return oldRename.apply(fsProxy, arguments);
  } as typeof fs.rename);

  // futimes (no-op readonly VFS)
  const oldFutimes = fsProxy.futimes;
  fsProxy.futimes = wrapAsyncMethod(oldFutimes, function (
    fd: number,
    atime: Date | number,
    mtime: Date | number,
    callback: Function
  ) {
    if (virtualFdMap.has(fd)) {
      console.log(`Attempted virtual futimes on readonly VFS for fd: ${fd}`);
      // Changing file times is not supported in the VFS and is a no-op as such.
      process.nextTick(() => callback(null));
      return;
    }
    return oldFutimes.apply(fsProxy, arguments);
  } as typeof fs.futimes);

  // truncate (no-op readonly VFS)
  const oldTruncate = fsProxy.truncate;
  fsProxy.truncate = wrapAsyncMethod(oldTruncate, function (
    filePath: string,
    len: number,
    callback: Function
  ) {
    if (typeof filePath === "string" && vfs.getEntry(filePath) !== undefined) {
      console.log(
        `Attempted virtual truncate on readonly VFS for: ${filePath}`
      );
      // Truncating files is not supported in the VFS and is a no-op as such.
      process.nextTick(() => callback(null));
      return;
    }
    return oldTruncate.apply(fsProxy, arguments);
  } as typeof fs.truncate);

  // realpath
  const oldRealpath = fsProxy.realpath;
  fsProxy.realpath = wrapAsyncMethod(oldRealpath, function realpathHook(
    this: typeof fs,
    filePath: string,
    optionsOrCallback?: any,
    maybeCallback?: any
  ) {
    // 1) Determine if second param is a callback or an options object
    let callback: Function | undefined;
    // let options: any = {};
    if (typeof optionsOrCallback === "function") {
      callback = optionsOrCallback;
    } else {
      // options = optionsOrCallback || {};
      callback = maybeCallback;
    }

    // 3) If a callback is present, do the "check VFS or fallback" logic
    if (typeof filePath === "string") {
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        // If the file/dir is found in the VFS, return some canonical path or the path as-is.
        console.log(`Returning virtual realpath for: ${filePath}`);
        // For example, just return the original path, or do your own resolution:
        if (callback) {
          process.nextTick(() => callback(null, filePath));
        }
        return;
      }
    }

    // 4) Fallback to the original realpath if it's not in the VFS
    return oldRealpath.apply(fsProxy, arguments);
  } as typeof fs.realpath);

  const oldRealpathNative = fsProxy.realpath.native;

  fsProxy.realpath.native = wrapAsyncMethod(oldRealpathNative, function (
    this: typeof fs,
    filePath: string,
    optionsOrCallback?: any,
    maybeCallback?: any
  ) {
    // 1) Determine if second param is a callback or an options object
    let callback: Function | undefined;
    // let options: any = {};
    if (typeof optionsOrCallback === "function") {
      callback = optionsOrCallback;
    } else {
      // options = optionsOrCallback || {};
      callback = maybeCallback;
    }

    // 3) If a callback is present, do the "check VFS or fallback" logic
    if (typeof filePath === "string") {
      const entry = vfs.getEntry(filePath);
      if (entry !== undefined) {
        // If the file/dir is found in the VFS, return some canonical path or the path as-is.
        console.log(`Returning virtual realpath for: ${filePath}`);
        // For example, just return the original path, or do your own resolution:
        if (callback) {
          process.nextTick(() => callback(null, filePath));
        }
        return;
      }
    }

    // 4) Fallback to the original realpath if it's not in the VFS
    return oldRealpathNative.apply(fsProxy, arguments);
  } as typeof fs.realpath.native);

  (globalThis as Record<string, unknown>).fsHooksInstalled = true;
  console.log("fs hooks installed");
})();
