import type * as ts from "typescript/lib/tsserverlibrary";

function init(mod: { typescript: typeof ts }) {
  function create(info: ts.server.PluginCreateInfo) {
    // Grab the server host
    const serverHost = info.serverHost;

    const logger = info.project.projectService.logger;
    logger.info("[VFS Plugin] create called with project: " + info.project.projectName);

    // --- Virtual contents ---
    const virtualTsconfigContent = `{
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
}`;

    let virtualTsconfigPath = "/tsconfig.json"; // default value
    let virtualFiles: Record<string, string> = {};
    let virtualDirectories = new Set<string>();

    /**
     * Recompute virtual file system paths.
     */
    function updateVirtualHosts() {
      updateVirtualTsconfigPath(); // This updates virtualTsconfigPath based on candidate files.
      // Compute the base directory from the virtual tsconfig path.
      const baseDir = virtualTsconfigPath.substring(0, virtualTsconfigPath.lastIndexOf("/"));
      virtualFiles = {};
      virtualFiles[virtualTsconfigPath] = virtualTsconfigContent;
      virtualFiles[baseDir + '/Libs/Application.ts'] = `// Application.ts
export class Application {
  public run(): void {
    console.log('Running...');
  }
}`;
      virtualFiles[baseDir + '/Libs/BoundingBox.ts'] = `// BoundingBox.ts
export interface IBoundingBox {
  x: number;
  y: number;
  width: number;
  height: number;
}`;
      virtualFiles[baseDir + '/Libs/Test.ts'] = `// Test.ts
console.log('Test file');`;

      virtualDirectories = new Set<string>([baseDir, baseDir + '/Libs']);
    }

    // Call once initially.
    updateVirtualHosts();

    // --- Helper: Normalize paths to use forward slashes ---
    function normalizePath(path: string): string {
      const normalized = path.replace(/\\/g, "/");
      logger.info("[VFS Plugin] normalizePath: " + path + " => " + normalized);
      return normalized;
    }

    // --- Helper: Get the directory portion of a path ---
    function getDirectory(filePath: string): string {
      const normalized = normalizePath(filePath);
      const lastSlash = normalized.lastIndexOf("/");
      return normalized.substring(0, lastSlash);
    }

    // --- Helper: Get the common parent directory among an array of directories ---
    function getCommonParent(paths: string[]): string {
      if (!paths.length) {
        return "/";
      }
      const splitPaths = paths.map((p) =>
        normalizePath(p).split("/").filter((seg) => seg.length > 0)
      );
      let commonParts = splitPaths[0];
      for (let i = 1; i < splitPaths.length; i++) {
        const parts = splitPaths[i];
        let j = 0;
        while (j < commonParts.length && j < parts.length && commonParts[j] === parts[j]) {
          j++;
        }
        commonParts = commonParts.slice(0, j);
        if (commonParts.length === 0) break;
      }
      return "/" + commonParts.join("/");
    }

    // --- Helper: Check if any parent directory of a file contains a real tsconfig.json ---
    function hasRealTsconfig(fileName: string): boolean {
      let current = normalizePath(fileName);
      // Remove file portion.
      current = getDirectory(current);
      while (current && current !== "/") {
        const candidate = current + "/tsconfig.json";
        if (oldFileExists(candidate)) {
          logger.info("[VFS Plugin] Found real tsconfig at: " + candidate);
          return true;
        }
        const lastSlash = current.lastIndexOf("/");
        if (lastSlash < 1) break;
        current = current.substring(0, lastSlash);
      }
      return false;
    }

    // --- Capture original host functions ---
    const oldFileExists =
      info.languageServiceHost.fileExists?.bind(info.languageServiceHost) ||
      ((fileName: string) => false);
    const oldReadFile =
      info.languageServiceHost.readFile?.bind(info.languageServiceHost) ||
      ((fileName: string, encoding?: string) => undefined);
    const oldDirectoryExists =
      info.languageServiceHost.directoryExists?.bind(info.languageServiceHost) ||
      ((dirName: string) => false);
    const oldGetDirectories =
      info.languageServiceHost.getDirectories?.bind(info.languageServiceHost) ||
      ((dirName: string) => [] as string[]);
    const oldGetScriptFileNames =
      info.languageServiceHost.getScriptFileNames?.bind(info.languageServiceHost) ||
      (() => [] as string[]);

    // --- Compute virtual tsconfig path based on candidate files ---
    // We consider only the directories of script files that don't have a real tsconfig in any parent.

    function updateVirtualTsconfigPath() {
      const allFiles = oldGetScriptFileNames();
      logger.info("[VFS Plugin] original getScriptFileNames: " + allFiles.join(", "));
      // Filter candidate files: files that do NOT have a real tsconfig in any parent.
      const candidateFiles = allFiles.filter((f) => !hasRealTsconfig(f));
      logger.info("[VFS Plugin] candidate files for virtual tsconfig: " + candidateFiles.join(", "));
      // Convert each candidate file to its parent directory.
      const candidateDirs = candidateFiles.map(getDirectory);
      logger.info("[VFS Plugin] candidate directories: " + candidateDirs.join(", "));
      if (candidateDirs.length) {
        const commonParent = getCommonParent(candidateDirs);
        virtualTsconfigPath = commonParent + "/tsconfig.json";
        logger.info("[VFS Plugin] computed virtual tsconfig path: " + virtualTsconfigPath);
      } else {
        virtualTsconfigPath = "/tsconfig.json";
        logger.info("[VFS Plugin] no candidate directories; using default virtual tsconfig path: " + virtualTsconfigPath);
      }
    }

    // --- Helper: Determine if a given path is virtual ---
    function isVirtualFile(path: string): boolean {
      return Object.prototype.hasOwnProperty.call(virtualFiles, path);
    }
    function isVirtualDirectory(path: string): boolean {
      return virtualDirectories.has(path);
    }

    // --- Patch fileExists ---
    info.languageServiceHost.fileExists = (fileName: string): boolean => {
      const normalized = normalizePath(fileName);
      if (isVirtualFile(normalized)) {
        logger.info("[VFS Plugin] fileExists virtual file found: " + fileName);
        return true;
      }
      const result = oldFileExists(fileName);
      logger.info("[VFS Plugin] fileExists default for: " + fileName + " -> " + result);
      return result;
    };

    // --- Patch readFile ---
    info.languageServiceHost.readFile = (fileName: string, encoding?: string): string | undefined => {
      const normalized = normalizePath(fileName);
      if (isVirtualFile(normalized)) {
        logger.info("[VFS Plugin] readFile: Returning virtual file content -> " + fileName);
        return virtualFiles[normalized];
      }
      const result = oldReadFile(fileName, encoding);
      logger.info("[VFS Plugin] readFile default for: " + fileName);
      return result;
    };

    // --- Patch directoryExists ---
    info.languageServiceHost.directoryExists = (directoryName: string): boolean => {
      const normalized = normalizePath(directoryName);
      if (isVirtualDirectory(normalized)) {
        logger.info("[VFS Plugin] directoryExists: Virtual directory exists -> " + directoryName);
        return true;
      }
      const result = oldDirectoryExists(directoryName);
      logger.info("[VFS Plugin] directoryExists default for: " + directoryName + " -> " + result);
      return result;
    };

    // --- Patch getDirectories ---
    info.languageServiceHost.getDirectories = (directoryName: string): string[] => {
      const normalized = normalizePath(directoryName);
      let result = oldGetDirectories(directoryName);
      // If this is the base directory, ensure that Libs is visible.
      const baseDir = virtualTsconfigPath.substring(0, virtualTsconfigPath.lastIndexOf("/"));
      if (normalized === baseDir) {
        if (!result.includes("Libs")) {
          result = [...result, "Libs"];
        }
        logger.info("[VFS Plugin] getDirectories virtual for: " + directoryName + " -> " + result.join(", "));
        return result;
      }
      result = oldGetDirectories(directoryName);
      logger.info("[VFS Plugin] getDirectories default for: " + directoryName + " -> " + result.join(", "));
      return result;
    };

    // --- Patch getScriptFileNames ---
    info.languageServiceHost.getScriptFileNames = (): string[] => {
      updateVirtualHosts();
      let files = oldGetScriptFileNames();
      // Ensure the computed virtual tsconfig is included.
      if (!files.some(f => normalizePath(f) === virtualTsconfigPath)) {
        files.push(virtualTsconfigPath);
        logger.info("[VFS Plugin] Added virtual tsconfig at: " + virtualTsconfigPath);
      }
      // Add virtual Libs files.
      for (const key of Object.keys(virtualFiles)) {
        // Skip the tsconfig itself.
        if (key === virtualTsconfigPath) {
          continue;
        }
        if (!files.some(f => normalizePath(f) === key)) {
          files.push(key);
          logger.info("[VFS Plugin] Added virtual file: " + key);
        }
      }
      logger.info("[VFS Plugin] final getScriptFileNames: " + files.join(", "));
      return files;
    };

    // --- Patch getCurrentDirectory() if not provided ---
    if (!info.languageServiceHost.getCurrentDirectory) {
      info.languageServiceHost.getCurrentDirectory = () => {
        logger.info("[VFS Plugin] getCurrentDirectory patched to return '/'");
        return "/";
      };
    }

    // --- Store original host methods ---
    const oldHostFileExists = serverHost.fileExists?.bind(serverHost) ?? ((path) => false);
    const oldHostReadFile = serverHost.readFile?.bind(serverHost) ?? ((path) => undefined);
    const oldHostDirectoryExists = serverHost.directoryExists?.bind(serverHost) ?? ((path) => false);
    const oldHostGetDirectories = serverHost.getDirectories?.bind(serverHost) ?? ((path) => [] as string[]);
    const oldHostReadDirectory = serverHost.readDirectory?.bind(serverHost) ?? ((path, exts, excl, incl, depth) => []);
    const oldHostRealpath = serverHost.realpath?.bind(serverHost) ?? ((path) => path);
    const oldHostWatchFile = serverHost.watchFile?.bind(serverHost);
    const oldHostWatchDirectory = serverHost.watchDirectory?.bind(serverHost);

    // --- Patch serverHost methods ---
    serverHost.fileExists = (path: string): boolean => {
      const normalized = normalizePath(path);
      if (isVirtualFile(normalized)) {
        logger.info(`[VFS Plugin] fileExists: Virtual file exists -> ${normalized}`);
        return true;
      }
      const result = oldHostFileExists(path);
      logger.info(`[VFS Plugin] fileExists: ${path} -> ${result}`);
      return result;
    };

    serverHost.readFile = (path: string, encoding?: string): string | undefined => {
      const normalized = normalizePath(path);
      if (isVirtualFile(normalized)) {
        logger.info(`[VFS Plugin] readFile: Returning virtual file content -> ${normalized}`);
        return virtualFiles[normalized];
      }
      const result = oldHostReadFile(path, encoding);
      logger.info(`[VFS Plugin] readFile: ${path} -> ${result ? "Success" : "Not Found"}`);
      return result;
    };

    serverHost.directoryExists = (path: string): boolean => {
      const normalized = normalizePath(path);
      if (isVirtualDirectory(normalized)) {
        logger.info(`[VFS Plugin] directoryExists: Virtual directory exists -> ${normalized}`);
        return true;
      }
      const result = oldHostDirectoryExists(path);
      logger.info(`[VFS Plugin] directoryExists: ${path} -> ${result}`);
      return result;
    };

    serverHost.getDirectories = (path: string): string[] => {
      const normalized = normalizePath(path);
      let result = oldHostGetDirectories(path);
      // If this is our base directory, add "Libs"
      const baseDir = virtualTsconfigPath.substring(0, virtualTsconfigPath.lastIndexOf("/"));
      if (normalized === baseDir && !result.includes("Libs")) {
        result = [...result, "Libs"];
      }
      logger.info(`[VFS Plugin] getDirectories: ${path} -> ${result.join(", ")}`);
      return result;
    };

    serverHost.readDirectory = (
      path: string,
      extensions?: readonly string[],
      exclude?: readonly string[],
      include?: readonly string[],
      depth?: number
    ): string[] => {
      const normalized = normalizePath(path);
      let result = oldHostReadDirectory(path, extensions, exclude, include, depth);
      if (isVirtualDirectory(normalized)) {
        // Append all virtual files that belong to this directory.
        result = [...result, ...Object.keys(virtualFiles).filter(f => f.startsWith(normalized))];
      }
      logger.info(`[VFS Plugin] readDirectory: ${path} -> ${result.join(", ")}`);
      return result;
    };

    serverHost.realpath = (path: string): string => {
      const normalized = normalizePath(path);
      if (isVirtualFile(normalized) || isVirtualDirectory(normalized)) {
        logger.info(`[VFS Plugin] realpath: Virtual path -> ${normalized}`);
        return normalized;
      }
      const result = oldHostRealpath(path);
      logger.info(`[VFS Plugin] realpath: ${path} -> ${result}`);
      return result;
    };

    if (oldHostWatchFile) {
      serverHost.watchFile = (fileName, callback, pollingInterval, options) => {
        logger.info(`[VFS Plugin] watchFile: Watching ${fileName}`);
        return oldHostWatchFile(fileName, callback, pollingInterval, options);
      };
    }

    if (oldHostWatchDirectory) {
      serverHost.watchDirectory = (directory, callback, recursive, options) => {
        logger.info(`[VFS Plugin] watchDirectory: Watching ${directory}`);
        return oldHostWatchDirectory(directory, callback, recursive, options);
      };
    }

    logger.info("[VFS Plugin] Plugin initialization complete.");
    return info.languageService;
  }

  return { create };
}

export = init;
