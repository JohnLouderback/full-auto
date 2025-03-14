import * as fs from "fs";

import * as vfs from "./virtual-fs";

console.log("installing fs hooks");

type Writeable<T extends { [x: string]: any }, K extends string> = {
  [P in K]: T[P];
};

const fsProxy = fs as Writeable<typeof fs, keyof typeof fs>;

const oldReadFileSync = fsProxy.readFileSync;

// @ts-expect-error
fsProxy.readFileSync = (...args) => {
  // If the first argument is a file path string, we'll check if it exists in the virtual
  // file system and return the contents if it does.
  if (args.length > 0 && typeof args[0] === "string") {
    const filePath = args[0] as string;
    const fileText = vfs.getFileText(filePath);
    if (fileText !== undefined) {
      console.log(`Returning virtual file text for: ${filePath}`);
      return fileText;
    }
  }

  // Otherwise, we'll return the result of the original readFileSync function.
  return oldReadFileSync(...args);
};

const oldReaddirSync = fsProxy.readdirSync;

// @ts-expect-error
fsProxy.readdirSync = (...args) => {
  // If the first argument is a directory path string, we'll check if it exists in the virtual
  // file system and return the contents if it does.
  if (args.length > 0 && typeof args[0] === "string") {
    const dirPath = args[0] as string;
    const contents = vfs.readDirectory(dirPath);
    if (contents !== undefined) {
      console.log(`Returning virtual directory contents for: ${dirPath}`);
      return contents;
    }
  }

  // Otherwise, we'll return the result of the original readdirSync function.
  // @ts-expect-error
  return oldReaddirSync(...args);
};

/**
 * Gets the file stats for a virtual file or directory entry.
 */
function fileStatFromEntry(entry: vfs.File | vfs.Directory): fs.Stats {
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

const oldStatSync = fsProxy.statSync;

// @ts-expect-error
fsProxy.statSync = (...args) => {
  // If the first argument is a file path string, we'll check if it exists in the virtual
  // file system and return the file stats if it does.
  if (args.length > 0 && typeof args[0] === "string") {
    const filePath = args[0] as string;
    const entry = vfs.getEntry(filePath);
    if (entry !== undefined) {
      console.log(`Returning virtual file stats for: ${filePath}`);
      return fileStatFromEntry(entry);
    }
  }

  // Otherwise, we'll return the result of the original statSync function.
  return oldStatSync(...args);
};

const oldExistsSync = fsProxy.existsSync;

fsProxy.existsSync = (...args) => {
  // If the first argument is a file or directory path string, we'll check if it exists in the
  // virtual file system and return true if it does.
  if (args.length > 0 && typeof args[0] === "string") {
    const filePath = args[0] as string;
    const entry = vfs.getEntry(filePath);
    if (entry !== undefined) {
      console.log(`Returning virtual file exists for: ${filePath}`);
      return true;
    }
  }

  // Otherwise, we'll return the result of the original existsSync function.
  return oldExistsSync(...args);
};

const oldWatch = fsProxy.watch;

// @ts-expect-error
fsProxy.watch = (...args) => {
  // If the first argument is a file or directory path string, we'll check if it exists in the
  // virtual file system and return a watcher if it does.
  if (args.length > 0 && typeof args[0] === "string") {
    const filePath = args[0] as string;
    const entry = vfs.getEntry(filePath);
    if (entry !== undefined) {
      console.log(`Returning virtual file watcher for: ${filePath}`);
      return {
        close() {
          console.log(`Closing virtual file watcher for: ${filePath}`);
        },
      };
    }
  }

  // Otherwise, we'll return the result of the original watch function.
  // @ts-expect-error
  return oldWatch(...args);
};

const oldWatchFile = fsProxy.watchFile;

// @ts-expect-error
fsProxy.watchFile = (...args) => {
  // If the first argument is a file path string, we'll check if it exists in the virtual
  // file system and return a watcher if it does.
  if (args.length > 0 && typeof args[0] === "string") {
    const filePath = args[0] as string;
    const entry = vfs.getEntry(filePath);
    if (entry !== undefined) {
      console.log(`Returning virtual file watcher for: ${filePath}`);
      return {
        close() {
          console.log(`Closing virtual file watcher for: ${filePath}`);
        },
      };
    }
  }

  // Otherwise, we'll return the result of the original watchFile function.
  // @ts-expect-error
  return oldWatchFile(...args);
};

const oldUnwatchFile = fsProxy.unwatchFile;

fsProxy.unwatchFile = (...args) => {
  // If the first argument is a file path string, we'll check if it exists in the virtual
  // file system and return a watcher if it does.
  if (args.length > 0 && typeof args[0] === "string") {
    const filePath = args[0] as string;
    const entry = vfs.getEntry(filePath);
    if (entry !== undefined) {
      console.log(`Unwatching virtual file: ${filePath}`);
      return;
    }
  }

  // Otherwise, we'll return the result of the original unwatchFile function.
  // @ts-expect-error
  return oldUnwatchFile(...args);
};
