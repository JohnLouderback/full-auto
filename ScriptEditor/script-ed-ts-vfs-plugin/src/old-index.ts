import type * as ts from "typescript/lib/tsserverlibrary";
import {
  convertCompilerOptionsFromJson,
  parseConfigFileTextToJson,
  sys,
} from "typescript/lib/tsserverlibrary";

class VFSPlugin {
  private virtualTsconfigPath: string = "/tsconfig.json";
  private virtualFiles: Record<string, string> = {};
  private virtualDirectories: Set<string> = new Set();
  // New member to track the shared virtual root
  private sharedVirtualRoot: string = "/";

  // Replace getters with regular fields
  private oldFileExists: (fileName: string) => boolean;
  private oldGetCompilationSettings: () => ts.CompilerOptions;
  private oldReadFile: (
    fileName: string,
    encoding?: string
  ) => string | undefined;
  private oldDirectoryExists: (dirName: string) => boolean;
  private oldGetDirectories: (dirName: string) => string[];
  private oldGetScriptFileNames: () => string[];

  private oldHostFileExists: (path: string) => boolean;
  private oldHostReadFile: (
    path: string,
    encoding?: string
  ) => string | undefined;
  private oldHostDirectoryExists: (path: string) => boolean;
  private oldHostGetDirectories: (path: string) => string[];
  private oldHostReadDirectory: (
    path: string,
    exts?: readonly string[],
    excl?: readonly string[],
    incl?: readonly string[],
    depth?: number
  ) => string[];
  private oldHostRealpath: (path: string) => string;
  private oldHostWatchFile?: any;
  private oldHostWatchDirectory?: any;

  constructor(private info: ts.server.PluginCreateInfo) {
    // Grab the server host
    const serverHost = info.serverHost;

    const logger = info.project.projectService.logger;
    logger.info(
      "[VFS Plugin] create called with project: " + info.project.projectName
    );

    // Initialize the fields from the existing methods
    this.oldFileExists =
      info.languageServiceHost.fileExists?.bind(info.languageServiceHost) ||
      ((fileName: string) => false);
    this.oldGetCompilationSettings =
      info.languageServiceHost.getCompilationSettings.bind(
        info.languageServiceHost
      ) || (() => ({} as ts.CompilerOptions));
    this.oldReadFile =
      info.languageServiceHost.readFile?.bind(info.languageServiceHost) ||
      ((fileName: string, encoding?: string) => undefined);
    this.oldDirectoryExists =
      info.languageServiceHost.directoryExists?.bind(
        info.languageServiceHost
      ) || ((dirName: string) => false);
    this.oldGetDirectories =
      info.languageServiceHost.getDirectories?.bind(info.languageServiceHost) ||
      ((dirName: string) => [] as string[]);
    this.oldGetScriptFileNames =
      info.languageServiceHost.getScriptFileNames?.bind(
        info.languageServiceHost
      ) || (() => [] as string[]);

    this.oldHostFileExists =
      serverHost.fileExists?.bind(serverHost) || ((path: string) => false);
    this.oldHostReadFile =
      serverHost.readFile?.bind(serverHost) ||
      ((path: string, encoding?: string) => undefined);
    this.oldHostDirectoryExists =
      serverHost.directoryExists?.bind(serverHost) || ((path: string) => false);
    this.oldHostGetDirectories =
      serverHost.getDirectories?.bind(serverHost) ||
      ((path: string) => [] as string[]);
    this.oldHostReadDirectory =
      serverHost.readDirectory?.bind(serverHost) ||
      ((path, exts, excl, incl, depth) => []);
    this.oldHostRealpath =
      serverHost.realpath?.bind(serverHost) || ((path: string) => path);
    this.oldHostWatchFile = serverHost.watchFile?.bind(serverHost);
    this.oldHostWatchDirectory = serverHost.watchDirectory?.bind(serverHost);

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

    this.virtualTsconfigPath = "/tsconfig.json"; // default value
    this.virtualFiles = {};
    this.virtualDirectories = new Set<string>();

    this.patchSys();
    this.updateVirtualHosts();
    this.patchLanguageServiceHost();
    this.patchServerHost();
    this.patchProject();

    this.info.project.projectService.openExternalProject({
      options: {
        configFilePath: this.virtualTsconfigPath,
      },
      projectFileName: "virtual-project",
      rootFiles: [
        {
          fileName: this.virtualTsconfigPath,
          scriptKind: "TS",
          hasMixedContent: false,
        },
      ],
    });
  }

  private get serverHost() {
    return this.info.serverHost;
  }

  // Helpers
  private normalizePath(path: string): string {
    const normalized = path.replace(/\\/g, "/").replace(/^\//, "");
    this.info.project.projectService.logger.info(
      "[Helper:normalizePath] " + path + " => " + normalized
    );
    return normalized;
  }

  private getDirectory(filePath: string): string {
    const normalized = this.normalizePath(filePath);
    const lastSlash = normalized.lastIndexOf("/");
    return normalized.substring(0, lastSlash);
  }

  private getCommonParent(paths: string[]): string {
    if (!paths.length) return "/";
    const splitPaths = paths.map((p) =>
      this.normalizePath(p)
        .split("/")
        .filter((seg) => seg.length > 0)
    );
    let commonParts = splitPaths[0];
    for (let i = 1; i < splitPaths.length; i++) {
      const parts = splitPaths[i];
      let j = 0;
      while (
        j < commonParts.length &&
        j < parts.length &&
        commonParts[j] === parts[j]
      ) {
        j++;
      }
      commonParts = commonParts.slice(0, j);
      if (commonParts.length === 0) break;
    }
    return commonParts.join("/");
  }

  private hasRealTsconfig(fileName: string): boolean {
    let current = this.normalizePath(fileName);
    current = this.getDirectory(current);
    while (current && current !== "/") {
      const candidate = current + "/tsconfig.json";
      // Using the getter for oldFileExists
      if (this.oldFileExists(candidate)) {
        this.info.project.projectService.logger.info(
          "[VFS Plugin] Found real tsconfig at: " + candidate
        );
        return true;
      }
      const lastSlash = current.lastIndexOf("/");
      if (lastSlash < 1) break;
      current = current.substring(0, lastSlash);
    }
    return false;
  }

  // Modify updateVirtualTsconfigPath to compute sharedVirtualRoot
  private updateVirtualTsconfigPath() {
    const allFiles = this.oldGetScriptFileNames();
    this.info.project.projectService.logger.info(
      "[VFS Plugin] original getScriptFileNames: " + allFiles.join(", ")
    );
    const candidateFiles = allFiles.filter((f) => !this.hasRealTsconfig(f));
    this.info.project.projectService.logger.info(
      "[VFS Plugin] candidate files for virtual tsconfig: " +
        candidateFiles.join(", ")
    );
    const candidateDirs = candidateFiles.map((f) => this.getDirectory(f));
    this.info.project.projectService.logger.info(
      "[VFS Plugin] candidate directories: " + candidateDirs.join(", ")
    );
    if (candidateDirs.length) {
      const commonParent = this.getCommonParent(candidateDirs);
      this.sharedVirtualRoot = this.normalizePath(commonParent);
      this.virtualTsconfigPath = this.sharedVirtualRoot + "/tsconfig.json";
      this.info.project.projectService.logger.info(
        "[VFS Plugin] computed sharedVirtualRoot: " + this.sharedVirtualRoot
      );
      this.info.project.projectService.logger.info(
        "[VFS Plugin] computed virtual tsconfig path: " +
          this.virtualTsconfigPath
      );
    } else {
      this.sharedVirtualRoot = "/";
      this.virtualTsconfigPath = "/tsconfig.json";
      this.info.project.projectService.logger.info(
        "[VFS Plugin] no candidate directories; using default virtual tsconfig path: " +
          this.virtualTsconfigPath
      );
    }
  }

  // Modify updateVirtualHosts to use sharedVirtualRoot
  private updateVirtualHosts() {
    this.updateVirtualTsconfigPath();
    const baseDir = this.sharedVirtualRoot;
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
    this.virtualFiles = {};
    this.virtualFiles[this.virtualTsconfigPath] = virtualTsconfigContent;
    this.virtualFiles[baseDir + "/Libs/Application.ts"] = `// Application.ts
export class Application {
  public run(): void {
    console.log('Running...');
  }
}`;
    this.virtualFiles[baseDir + "/Libs/BoundingBox.ts"] = `// BoundingBox.ts
export interface IBoundingBox {
  x: number;
  y: number;
  width: number;
  height: number;
}`;
    this.virtualFiles[baseDir + "/Libs/Test.ts"] = `// Test.ts
console.log('Test file');`;
    this.virtualDirectories = new Set<string>([baseDir, baseDir + "/Libs"]);
  }

  private isVirtualFile(path: string): boolean {
    this.info.project.projectService.logger.info(
      "Checking if virtual file exists: " +
        path +
        "\nAgainst possible virtual files: " +
        Object.keys(this.virtualFiles).join(",\n")
    );
    return Object.prototype.hasOwnProperty.call(this.virtualFiles, path);
  }
  private isVirtualDirectory(path: string): boolean {
    return this.virtualDirectories.has(path);
  }

  // Patching project methods
  private patchProject() {
    const logger = this.info.project.projectService.logger;
    // Patch getScriptFileNames
    this.info.project.getScriptFileNames = (): string[] => {
      this.updateVirtualHosts();
      let files = this.oldGetScriptFileNames();
      if (
        !files.some((f) => this.normalizePath(f) === this.virtualTsconfigPath)
      ) {
        files.push(this.virtualTsconfigPath);
        logger.info(
          "[Project:getScriptFileNames] Added virtual tsconfig at: " +
            this.virtualTsconfigPath
        );
      }
      for (const key of Object.keys(this.virtualFiles)) {
        if (key === this.virtualTsconfigPath) continue;
        if (!files.some((f) => this.normalizePath(f) === key)) {
          files.push(key);
          logger.info(
            "[Project:getScriptFileNames] Added virtual file: " + key
          );
        }
      }
      logger.info(
        "[Project:getScriptFileNames] final files: " + files.join(", ")
      );
      return files;
    };

    // Patch fileExists
    this.info.project.fileExists = (fileName: string): boolean => {
      const normalized = this.normalizePath(fileName);
      if (this.isVirtualFile(normalized)) {
        logger.info("[Project:fileExists] Virtual file exists -> " + fileName);
        return true;
      }
      const result = this.oldFileExists(fileName);
      logger.info(
        "[Project:fileExists] Default for: " + fileName + " -> " + result
      );
      return result;
    };

    // Patch readFile
    this.info.project.readFile = (
      fileName: string,
      encoding?: string
    ): string | undefined => {
      const normalized = this.normalizePath(fileName);
      if (this.isVirtualFile(normalized)) {
        logger.info(
          "[Project:readFile] Returning virtual file content -> " + fileName
        );
        return this.virtualFiles[normalized];
      }
      const result = this.oldReadFile(fileName, encoding);
      logger.info("[Project:readFile] Default for: " + fileName);
      return result;
    };
  }

  // Patching sys methods
  private patchSys() {
    const logger = this.info.project.projectService.logger;
    // Patch fileExists
    sys.fileExists = (path: string): boolean => {
      const normalized = this.normalizePath(path);
      if (this.isVirtualFile(normalized)) {
        logger.info("[SYS:fileExists] Virtual file exists -> " + path);
        return true;
      }
      const result = this.oldFileExists(path);
      logger.info("[SYS:fileExists] Default for: " + path + " -> " + result);
      return result;
    };

    // Patch readFile
    sys.readFile = (path: string, encoding?: string): string | undefined => {
      const normalized = this.normalizePath(path);
      if (this.isVirtualFile(normalized)) {
        logger.info("[SYS:readFile] Returning virtual file content -> " + path);
        return this.virtualFiles[normalized];
      }
      const result = this.oldReadFile(path, encoding);
      logger.info("[SYS:readFile] Default for: " + path);
      return result;
    };

    // Patch directoryExists
    sys.directoryExists = (path: string): boolean => {
      const normalized = this.normalizePath(path);
      if (this.isVirtualDirectory(normalized)) {
        logger.info(
          "[SYS:directoryExists] Virtual directory exists -> " + path
        );
        return true;
      }
      const result = this.oldDirectoryExists(path);
      logger.info(
        "[SYS:directoryExists] Default for: " + path + " -> " + result
      );
      return result;
    };

    // Patch getDirectories
    sys.getDirectories = (path: string): string[] => {
      const normalized = this.normalizePath(path);
      let result = this.oldGetDirectories(path);
      const baseDir = this.virtualTsconfigPath.substring(
        0,
        this.virtualTsconfigPath.lastIndexOf("/")
      );
      if (normalized === baseDir && !result.includes("Libs")) {
        result = [...result, "Libs"];
      }
      logger.info(
        "[SYS:getDirectories] For: " + path + " -> " + result.join(", ")
      );
      return result;
    };

    // Patch readDirectory
    sys.readDirectory = (
      path: string,
      extensions?: readonly string[],
      exclude?: readonly string[],
      include?: readonly string[],
      depth?: number
    ): string[] => {
      const normalized = this.normalizePath(path);
      let result = this.oldHostReadDirectory(
        path,
        extensions,
        exclude,
        include,
        depth
      );
      if (this.isVirtualDirectory(normalized)) {
        result = [
          ...result,
          ...Object.keys(this.virtualFiles).filter((f) =>
            f.startsWith(normalized)
          ),
        ];
      }
      logger.info(
        "[SYS:readDirectory] For: " + path + " -> " + result.join(", ")
      );
      return result;
    };

    // Patch realpath
    sys.realpath = (path: string): string => {
      const normalized = this.normalizePath(path);
      if (
        this.isVirtualFile(normalized) ||
        this.isVirtualDirectory(normalized)
      ) {
        logger.info("[SYS:realpath] Virtual path -> " + normalized);
        return normalized;
      }
      const result = this.oldHostRealpath(path);
      logger.info("[SYS:realpath] For: " + path + " -> " + result);
      return result;
    };

    // Patch watchFile if available
    sys.watchFile = (
      fileName: string,
      callback: ts.FileWatcherCallback,
      pollingInterval?: number,
      options?: ts.WatchOptions
    ): ts.FileWatcher => {
      const normalized = this.normalizePath(fileName);
      const logger = this.info.project.projectService.logger;
      if (this.isVirtualFile(normalized)) {
        logger.info(
          `[SYS:watchFile] Virtual file detected (${fileName}). Returning dummy watcher.`
        );
        // Return a dummy watcher that does nothing.
        return {
          close: () => {
            /* nothing to dispose for virtual files */
          },
        };
      }
      logger.info(`[SYS:watchFile] Watching real file ${fileName}`);
      return this.oldHostWatchFile!(
        fileName,
        callback,
        pollingInterval,
        options
      );
    };

    // Patch watchDirectory if available
    sys.watchDirectory = (
      directory: string,
      callback: ts.DirectoryWatcherCallback,
      recursive?: boolean,
      options?: ts.WatchOptions
    ): ts.FileWatcher => {
      const normalized = this.normalizePath(directory);
      const logger = this.info.project.projectService.logger;
      if (this.isVirtualDirectory(normalized)) {
        logger.info(
          `[SYS:watchDirectory] Virtual directory detected (${directory}). Returning dummy watcher.`
        );
        return {
          close: () => {
            /* nothing to dispose for virtual directories */
          },
        };
      }
      logger.info(`[SYS:watchDirectory] Watching real directory ${directory}`);
      return this.oldHostWatchDirectory!(
        directory,
        callback,
        recursive,
        options
      );
    };
  }

  // Patching languageServiceHost methods
  private patchLanguageServiceHost() {
    const logger = this.info.project.projectService.logger;
    // Patch fileExists
    this.info.languageServiceHost.fileExists = (fileName: string): boolean => {
      const normalized = this.normalizePath(fileName);
      if (this.isVirtualFile(normalized)) {
        logger.info("[LSH:fileExists] Virtual file found: " + fileName);
        return true;
      }
      const result = this.oldFileExists(fileName);
      logger.info(
        "[LSH:fileExists] Default for: " + fileName + " -> " + result
      );
      return result;
    };
    // Patch getCompilationSettings
    this.info.languageServiceHost.getCompilationSettings =
      (): ts.CompilerOptions => {
        const result = this.oldGetCompilationSettings();
        logger.info(
          "[LSH:getCompilationSettings] Default -> " + JSON.stringify(result)
        );
        // Get the compiler options from the virtual tsconfig
        const virtualTsconfig = this.virtualFiles[this.virtualTsconfigPath];

        if (!virtualTsconfig) {
          logger.info(
            "[LSH:getCompilationSettings] No virtual tsconfig found at: " +
              this.virtualTsconfigPath
          );
          return result;
        }

        const parsed = parseConfigFileTextToJson(
          this.virtualTsconfigPath,
          virtualTsconfig
        );
        if (parsed.error) {
          logger.info(
            "[LSH:getCompilationSettings] Error parsing virtual tsconfig: " +
              JSON.stringify(parsed.error)
          );
          return result;
        }

        // Update the paths array to replace leading "./" with the virtual root for the libraries.
        parsed.config.compilerOptions.paths = Object.fromEntries(
          Object.entries(parsed.config.compilerOptions.paths).map(
            ([key, value]) => {
              return [
                key,
                (value as Array<string>).map((v: string) =>
                  v.startsWith("./")
                    ? this.sharedVirtualRoot + v.substring(1)
                    : v
                ),
              ];
            }
          )
        );

        logger.info(
          "[LSH:getCompilationSettings] Resolved paths -> " +
            JSON.stringify(parsed.config.paths)
        );

        const virtualOptions = convertCompilerOptionsFromJson(
          parsed.config.compilerOptions,
          ""
        );
        if (virtualOptions.errors.length) {
          logger.info(
            "[LSH:getCompilationSettings] Error converting options: " +
              JSON.stringify(virtualOptions.errors)
          );
          return result;
        }

        const newCompilerOptions = { ...result, ...virtualOptions.options };
        logger.info(
          "[LSH:getCompilationSettings] Merged options -> " +
            JSON.stringify(newCompilerOptions)
        );
        return newCompilerOptions;
      };
    // Patch readFile
    this.info.languageServiceHost.readFile = (
      fileName: string,
      encoding?: string
    ): string | undefined => {
      const normalized = this.normalizePath(fileName);
      if (this.isVirtualFile(normalized)) {
        logger.info(
          "[LSH:readFile] Returning virtual file content -> " + fileName
        );
        return this.virtualFiles[normalized];
      }
      const result = this.oldReadFile(fileName, encoding);
      logger.info("[LSH:readFile] Default for: " + fileName);
      return result;
    };
    // Patch readDirectory
    this.info.languageServiceHost.readDirectory = (
      path: string,
      extensions?: readonly string[],
      exclude?: readonly string[],
      include?: readonly string[],
      depth?: number
    ) => {
      const normalized = this.normalizePath(path);
      let result = this.oldHostReadDirectory(
        path,
        extensions,
        exclude,
        include,
        depth
      );
      if (this.isVirtualDirectory(normalized)) {
        result = [
          ...result,
          ...Object.keys(this.virtualFiles).filter((f) =>
            f.startsWith(normalized)
          ),
        ];
      }
      logger.info(
        "[LSH:readDirectory] For: " + path + " -> " + result.join(", ")
      );
      return [...result, this.normalizePath(path + "tsconfig.json")];
    };

    // Patch directoryExists
    this.info.languageServiceHost.directoryExists = (
      directoryName: string
    ): boolean => {
      const normalized = this.normalizePath(directoryName);
      if (this.isVirtualDirectory(normalized)) {
        logger.info(
          "[LSH:directoryExists] Virtual directory exists -> " + directoryName
        );
        return true;
      }
      const result = this.oldDirectoryExists(directoryName);
      logger.info(
        "[LSH:directoryExists] Default for: " + directoryName + " -> " + result
      );
      return result;
    };
    // Patch getDirectories
    this.info.languageServiceHost.getDirectories = (
      directoryName: string
    ): string[] => {
      const normalized = this.normalizePath(directoryName);
      let result = this.oldGetDirectories(directoryName);
      const baseDir = this.virtualTsconfigPath.substring(
        0,
        this.virtualTsconfigPath.lastIndexOf("/")
      );
      if (normalized === baseDir && !result.includes("Libs")) {
        result = [...result, "Libs"];
        logger.info(
          "[LSH:getDirectories] Virtual for: " +
            directoryName +
            " -> " +
            result.join(", ")
        );
        return result;
      }
      logger.info(
        "[LSH:getDirectories] Default for: " +
          directoryName +
          " -> " +
          result.join(", ")
      );
      return result;
    };
    // Patch getScriptFileNames
    this.info.languageServiceHost.getScriptFileNames = (): string[] => {
      this.updateVirtualHosts();
      let files = this.oldGetScriptFileNames();
      if (
        !files.some((f) => this.normalizePath(f) === this.virtualTsconfigPath)
      ) {
        files.push(this.virtualTsconfigPath);
        logger.info(
          "[LSH:getScriptFileNames] Added virtual tsconfig at: " +
            this.virtualTsconfigPath
        );
      }
      for (const key of Object.keys(this.virtualFiles)) {
        if (key === this.virtualTsconfigPath) continue;
        if (!files.some((f) => this.normalizePath(f) === key)) {
          files.push(key);
          logger.info("[LSH:getScriptFileNames] Added virtual file: " + key);
        }
      }
      logger.info("[LSH:getScriptFileNames] Final files: " + files.join(", "));
      return files;
    };
    // Patch getCurrentDirectory if missing
    if (!this.info.languageServiceHost.getCurrentDirectory) {
      this.info.languageServiceHost.getCurrentDirectory = () => {
        logger.info("[LSH:getCurrentDirectory] Patched to return '/'");
        return "/";
      };
    }
  }

  // Patching serverHost methods
  private patchServerHost() {
    const logger = this.info.project.projectService.logger;
    // Patch fileExists
    this.serverHost.fileExists = (path: string): boolean => {
      const normalized = this.normalizePath(path);
      if (this.isVirtualFile(normalized)) {
        logger.info("[SH:fileExists] Virtual file exists -> " + normalized);
        return true;
      }
      const result = this.oldHostFileExists(path);
      logger.info("[SH:fileExists] Default for: " + path + " -> " + result);
      return result;
    };
    // Patch readFile
    this.serverHost.readFile = (
      path: string,
      encoding?: string
    ): string | undefined => {
      const normalized = this.normalizePath(path);
      if (this.isVirtualFile(normalized)) {
        logger.info(
          "[SH:readFile] Returning virtual file content -> " + normalized
        );
        return this.virtualFiles[normalized];
      }
      const result = this.oldHostReadFile(path, encoding);
      logger.info(
        "[SH:readFile] Default for: " +
          path +
          " -> " +
          (result ? "Success" : "Not Found")
      );
      return result;
    };
    // Patch directoryExists
    this.serverHost.directoryExists = (path: string): boolean => {
      const normalized = this.normalizePath(path);
      if (this.isVirtualDirectory(normalized)) {
        logger.info(
          "[SH:directoryExists] Virtual directory exists -> " + normalized
        );
        return true;
      }
      const result = this.oldHostDirectoryExists(path);
      logger.info(
        "[SH:directoryExists] Default for: " + path + " -> " + result
      );
      return result;
    };
    // Patch getDirectories
    this.serverHost.getDirectories = (path: string): string[] => {
      const normalized = this.normalizePath(path);
      let result = this.oldHostGetDirectories(path);
      const baseDir = this.virtualTsconfigPath.substring(
        0,
        this.virtualTsconfigPath.lastIndexOf("/")
      );
      if (normalized === baseDir && !result.includes("Libs")) {
        result = [...result, "Libs"];
      }
      logger.info(
        "[SH:getDirectories] For: " + path + " -> " + result.join(", ")
      );
      return result;
    };
    // Patch readDirectory
    this.serverHost.readDirectory = (
      path: string,
      extensions?: readonly string[],
      exclude?: readonly string[],
      include?: readonly string[],
      depth?: number
    ): string[] => {
      const normalized = this.normalizePath(path);
      let result = this.oldHostReadDirectory(
        path,
        extensions,
        exclude,
        include,
        depth
      );
      if (this.isVirtualDirectory(normalized)) {
        result = [
          ...result,
          ...Object.keys(this.virtualFiles).filter((f) =>
            f.startsWith(normalized)
          ),
        ];
      }
      logger.info(
        "[SH:readDirectory] For: " + path + " -> " + result.join(", ")
      );
      return [...result, this.normalizePath(path + "tsconfig.json")];
    };
    // Patch realpath
    this.serverHost.realpath = (path: string): string => {
      const normalized = this.normalizePath(path);
      if (
        this.isVirtualFile(normalized) ||
        this.isVirtualDirectory(normalized)
      ) {
        logger.info("[SH:realpath] Virtual path -> " + normalized);
        return normalized;
      }
      const result = this.oldHostRealpath(path);
      logger.info("[SH:realpath] For: " + path + " -> " + result);
      return result;
    };
    // Patch watchFile if available
    this.serverHost.watchFile = (
      fileName: string,
      callback: ts.FileWatcherCallback,
      pollingInterval?: number,
      options?: ts.WatchOptions
    ): ts.FileWatcher => {
      const normalized = this.normalizePath(fileName);
      const logger = this.info.project.projectService.logger;
      if (this.isVirtualFile(normalized)) {
        logger.info(
          `[SH:watchFile] Virtual file detected (${fileName}). Returning dummy watcher.`
        );
        // Return a dummy watcher that does nothing.
        return {
          close: () => {
            /* nothing to dispose for virtual files */
          },
        };
      }
      logger.info(`[SH:watchFile] Watching real file ${fileName}`);
      return this.oldHostWatchFile!(
        fileName,
        callback,
        pollingInterval,
        options
      );
    };
    // Patch watchDirectory if available
    this.serverHost.watchDirectory = (
      directory: string,
      callback: ts.DirectoryWatcherCallback,
      recursive?: boolean,
      options?: ts.WatchOptions
    ): ts.FileWatcher => {
      const normalized = this.normalizePath(directory);
      const logger = this.info.project.projectService.logger;
      if (this.isVirtualDirectory(normalized)) {
        logger.info(
          `[SH:watchDirectory] Virtual directory detected (${directory}). Returning dummy watcher.`
        );
        return {
          close: () => {
            /* nothing to dispose for virtual directories */
          },
        };
      }
      logger.info(`[SH:watchDirectory] Watching real directory ${directory}`);
      return this.oldHostWatchDirectory!(
        directory,
        callback,
        recursive,
        options
      );
    };
  }
}

// Exported init function
function init(mod: { typescript: typeof ts }): ts.server.PluginModule {
  function create(info: ts.server.PluginCreateInfo) {
    // Instantiate our class-based plugin.
    new VFSPlugin(info);
    return info.languageService;
  }
  return { create };
}

export = init;
