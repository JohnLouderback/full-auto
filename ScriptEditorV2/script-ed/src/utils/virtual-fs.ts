import * as path from "path";

export type File = string;

export interface Directory {
  [key: string]: File | Directory;
}

const virtualFileSystem: Directory = {
  "tsconfig.json": `{
  "compilerOptions": {
    "module": "ESNext",
    "target": "ESNext",
    "lib": ["es6", "WebWorker"],
    "inlineSourceMap": true,
    "inlineSources": true,
    "typeRoots": ["../Schema/"],
    "paths": {
      "@library/*": ["./Libs/*"]
    }
  },
  "include": ["./Schema/**/*.d.ts", "./**/*.ts"],
  "exclude": ["node_modules"]
}`,
  Libs: {
    "Application.ts": `export class Application
{
    constructor() {}
}`,
  },
};

const lookupCache = new Map<string, File | Directory | null>();

export function isFile(entry?: File | Directory): entry is File {
  return typeof entry === "string";
}

export function isDirectory(entry?: File | Directory): entry is Directory {
  return typeof entry === "object" && entry !== null;
}

/**
 * Recursively flattens the virtual file system into a map whose keys are
 * relative paths (using the OS path separator) and whose values are file or directory entries.
 */
function flattenVFS(
  entry: File | Directory,
  currentPath: string = ""
): Map<string, File | Directory> {
  const map = new Map<string, File | Directory>();
  if (isFile(entry)) {
    map.set(currentPath, entry);
  } else {
    if (currentPath) {
      map.set(currentPath, entry);
    }
    for (const key in entry) {
      if (Object.prototype.hasOwnProperty.call(entry, key)) {
        const child = entry[key];
        const childPath = currentPath ? path.join(currentPath, key) : key;
        const childMap = flattenVFS(child, childPath);
        for (const [childKey, childEntry] of childMap) {
          map.set(childKey, childEntry);
        }
      }
    }
  }
  return map;
}

// Pre-flatten the virtual file system once.
const flatVFS = flattenVFS(virtualFileSystem);

/**
 * Gets a file or directory from the virtual file system, if it exists.
 * It tries every possible relative suffix of the provided filePath.
 */
export function getEntry(filePath: string): File | Directory | undefined {
  console.log(`getEntry called with: ${filePath}`);
  if (lookupCache.has(filePath)) {
    console.log(`Cache hit for: ${filePath}`);
    const cached = lookupCache.get(filePath);
    return cached === null ? undefined : cached;
  }

  // Normalize the file path so that it uses the correct path separator.
  const normalizedPath = path.normalize(filePath);
  // Split the normalized path into parts and filter out any empty segments.
  const parts = normalizedPath.split(path.sep).filter(Boolean);

  // Try every possible suffix.
  for (let i = 0; i < parts.length; i++) {
    const candidate = parts.slice(i).join(path.sep);
    console.log(`Checking candidate: ${candidate}`);
    if (flatVFS.has(candidate)) {
      console.log(`Found candidate: ${candidate}`);
      const entry = flatVFS.get(candidate)!;
      lookupCache.set(filePath, entry);
      return entry;
    }
  }
  console.log(`No entry found for: ${filePath}`);
  lookupCache.set(filePath, null);
  return undefined;
}

/**
 * Gets the text of a file from the virtual file system, if it exists.
 */
export function getFileText(filePath: string): string | undefined {
  const entry = getEntry(filePath);
  return isFile(entry) ? entry : undefined;
}

/**
 * Gets the physical root for a matching path in the virtual file system.
 * For example, for an absolute file path like
 * "c:/Users/John/Documents/ExampleScripts/Libs/Application.ts", if "Libs/Application.ts"
 * exists in the VFS, this function returns "c:/Users/John/Documents/ExampleScripts/".
 */
export function getRootForMatchingPath(filePath: string): string {
  const normalizedPath = path.normalize(filePath);
  const parts = normalizedPath.split(path.sep).filter(Boolean);
  for (let i = 0; i < parts.length; i++) {
    const candidate = parts.slice(i).join(path.sep);
    if (flatVFS.has(candidate)) {
      return parts.slice(0, i).join(path.sep);
    }
  }
  return "";
}

/**
 * Reads a directory from the virtual file system, returning a list of file and directory names.
 */
export function readDirectory(filePath: string): string[] | undefined {
  const entry = getEntry(filePath);
  return isDirectory(entry) ? Object.keys(entry) : undefined;
}

/**
 * Reads a directory from the virtual file system, returning a list of absolute file paths.
 */
export function readDirectoryAbsolutePaths(
  filePath: string
): string[] | undefined {
  const entry = getEntry(filePath);
  return isDirectory(entry)
    ? Object.keys(entry).map((name) => path.join(filePath, name))
    : undefined;
}
