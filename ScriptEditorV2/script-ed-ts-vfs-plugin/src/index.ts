import { resolve as pathResolve } from "node:path";

import type * as ts from "typescript/lib/tsserverlibrary";

/**
 * A map representing the virtual file system.
 */
const virtualFiles = new Map<string, string>([
  [
    "tsconfig.json",
    `{
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
  ],
  [
    "Libs/Application.ts",
    `export class Application {
    constructor() {}
}`,
  ],
]);

class VFSPlugin {
  /**
   * Determines if a file is potentially virtual. If it is, it returns the virtual file
   * name. Otherwise, it returns `undefined`.
   */
  private isFilePotentiallyVirtual(fileName: string): string | undefined {
    // Any files under /Libs/ are considered virtual
    if (fileName.indexOf("Libs/") !== -1) {
      // Get the "Libs/*" path including "Libs/"
      return fileName.substring(fileName.indexOf("Libs/"));
    }
    // Otherwise, any tsconfig.json file is considered virtual as we wish to force it to
    // be such.
    else if (fileName.endsWith("tsconfig.json")) {
      return "tsconfig.json";
    }
    return undefined;
  }

  private readonly logger: ts.server.Logger;

  constructor(
    private readonly info: ts.server.PluginCreateInfo,
    private readonly typescript: typeof ts
  ) {
    this.logger = this.info.project.projectService.logger;

    const oldFileExists = typescript.sys.fileExists;
    typescript.sys.fileExists = (fileName: string) => {
      this.logger.info(`Intercepted fileExists for "${fileName}"`);
      // Check if the requested file is potentially virtual.
      const requestedFile = this.isFilePotentiallyVirtual(fileName);

      // If the requested file appears to be a candidate for the virtual file system
      // and it exists in the virtual file system, return true.
      if (requestedFile && virtualFiles.has(requestedFile)) {
        this.logger.info(`File "${fileName}" is virtual.`);
        return true;
      }

      // Otherwise, return the result of the original fileExists function.
      return oldFileExists(fileName);
    };

    const oldReadFile = typescript.sys.readFile;
    typescript.sys.readFile = (fileName: string) => {
      this.logger.info(`Intercepted readFile for "${fileName}"`);
      // Check if the requested file is potentially virtual.
      const requestedFile = this.isFilePotentiallyVirtual(fileName);

      // If the requested file appears to be a candidate for the virtual file system
      // and it exists in the virtual file system, return the virtual file contents.
      if (requestedFile && virtualFiles.has(requestedFile)) {
        this.logger.info(`Reading virtual file "${fileName}"`);
        return virtualFiles.get(requestedFile);
      }

      // Otherwise, return the result of the original readFile function.
      return oldReadFile(fileName);
    };

    const oldWatchFile = typescript.sys.watchFile;

    if (oldWatchFile) {
      typescript.sys.watchFile = (
        fileName: string,
        callback: ts.FileWatcherCallback,
        pollingInterval?: number,
        options?: ts.WatchOptions
      ) => {
        this.logger.info(`Intercepted watchFile for "${fileName}"`);
        // Check if the requested file is potentially virtual.
        const requestedFile = this.isFilePotentiallyVirtual(fileName);

        // If the requested file appears to be a candidate for the virtual file system
        // and it exists in the virtual file system, return a no-op function.
        if (requestedFile && virtualFiles.has(requestedFile)) {
          this.logger.info(`Watching virtual file "${fileName}"`);
          return { close() {} } as ts.FileWatcher;
        }

        // Otherwise, return the result of the original watchFile function.
        return oldWatchFile(fileName, callback, pollingInterval, options);
      };
    }
  }
}

// Exported init function
function init(mod: { typescript: typeof ts }): ts.server.PluginModule {
  function create(info: ts.server.PluginCreateInfo) {
    // Instantiate our class-based plugin.
    const plugin = new VFSPlugin(info, mod.typescript);
    return info.languageService;
  }

  function getExternalFiles(project: ts.server.ConfiguredProject) {
    const basePath = project.getCurrentDirectory();
    const externalFiles = Array.from(virtualFiles.keys()).map((fileName) =>
      pathResolve(basePath, fileName)
    );

    project.projectService.logger.info(
      `External files provided: ${externalFiles.join(", ")}`
    );
    return externalFiles;
  }

  return { create, getExternalFiles };
}

export = init;
