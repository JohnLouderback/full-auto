import { URI } from '@theia/core/lib/common/uri';
import { injectable } from '@theia/core/shared/inversify';
import {
  createFileSystemProviderError,
  FileOpenOptions,
  FileSystemProviderErrorCode,
  FileType,
  Stat,
} from '@theia/filesystem/lib/common/files';
import {
  DiskFileSystemProvider,
} from '@theia/filesystem/lib/node/disk-file-system-provider';

@injectable()
export class VirtualDiskFileSystemProvider extends DiskFileSystemProvider {
  // --- Your virtual data: ---
  protected readonly DEFAULT_TSCONFIG = `{
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

  protected readonly VIRTUAL_LIBS: { [fileName: string]: string } = {
    "Application.ts": `// Example Application.ts
export class Application {
    public run(): void {
        console.log('Running...');
    }
}
`,
    "BoundingBox.ts": `// Example BoundingBox.ts
export interface IBoundingBox {
    x: number;
    y: number;
    width: number;
    height: number;
}
`,
    "Test.ts": `// Example Test.ts
console.log('Test file');
`,
  };

  private readonly VIRTUAL_FILE_INDEX = new Map<number, string>([
    [1, "tsconfig.json"],
    [2, "Application.ts"],
    [3, "BoundingBox.ts"],
    [4, "Test.ts"],
  ]);

  private lookUpFileInIndex(fileName: string): number | undefined {
    for (const [index, name] of this.VIRTUAL_FILE_INDEX) {
      if (name === fileName) {
        return index;
      }
    }
    return undefined;
  }

  override async open(resource: URI, opts: FileOpenOptions): Promise<number> {
    // Check if the file is virtual.
    const pathStr = resource.path.toString();
    if (pathStr.includes("/Libs") || resource.path.base === "tsconfig.json") {
      const fileName = resource.path.base;
      const index = this.lookUpFileInIndex(fileName);
      if (index !== undefined) {
        return index;
      }
    }
    return super.open(resource, opts);
  }

  override async close(fd: number): Promise<void> {
    if (this.VIRTUAL_FILE_INDEX.has(fd)) {
      return;
    }
    return super.close(fd);
  }

  override async read(
    fd: number,
    pos: number,
    data: Uint8Array,
    offset: number,
    length: number
  ): Promise<number> {
    if (this.VIRTUAL_FILE_INDEX.has(fd)) {
      const fileName = this.VIRTUAL_FILE_INDEX.get(fd)!;
      const content =
        fileName === "tsconfig.json"
          ? this.DEFAULT_TSCONFIG
          : this.VIRTUAL_LIBS[fileName];
      const buffer = new TextEncoder().encode(content);
      const slice = buffer.slice(pos, pos + length);
      data.set(slice, offset);
      return slice.length;
    }
    return super.read(fd, pos, data, offset, length);
  }

  /**
   * Intercepts requests for `tsconfig.json` or `/Libs/*`.
   * If no real file is found, returns virtual content.
   * Otherwise, delegates to the base DiskFileSystemProvider.
   */
  override async stat(resource: URI): Promise<Stat> {
    const pathStr = resource.path.toString();
    const base = resource.path.base;

    // 1) If requesting `tsconfig.json` and the file does not physically exist, return a virtual file.
    if (base === "tsconfig.json") {
      try {
        // If a real tsconfig exists, just let the base class handle it:
        return await super.stat(resource);
      } catch {
        // Return a virtual file stat if the real file is missing.
        const size = Buffer.byteLength(this.DEFAULT_TSCONFIG);
        return {
          type: FileType.File,
          ctime: Date.now(),
          mtime: Date.now(),
          size,
        };
      }
    }

    // 2) If requesting something under "/Libs":
    if (pathStr.includes("/Libs")) {
      const segments = pathStr.split("/");
      const libsIndex = segments.indexOf("Libs");
      const afterLibs = segments.slice(libsIndex + 1);

      // If exactly `/Libs` directory
      if (afterLibs.length === 0 || (afterLibs.length === 1 && !afterLibs[0])) {
        return {
          type: FileType.Directory,
          ctime: Date.now(),
          mtime: Date.now(),
          size: 0,
        };
      }
      // If a specific file in `/Libs`
      else if (afterLibs.length === 1) {
        const fileName = afterLibs[0];
        if (this.VIRTUAL_LIBS[fileName] !== undefined) {
          const size = Buffer.byteLength(this.VIRTUAL_LIBS[fileName]);
          return {
            type: FileType.File,
            ctime: Date.now(),
            mtime: Date.now(),
            size,
          };
        }
      }
    }

    // Otherwise, fall back to the real disk provider
    return super.stat(resource);
  }

  override async readFile(resource: URI): Promise<Uint8Array> {
    const pathStr = resource.path.toString();
    const base = resource.path.base;

    // 1) Virtual tsconfig.json
    if (base === "tsconfig.json") {
      // If physically exists, read from disk
      try {
        return await super.readFile(resource);
      } catch {
        // If no real file, return the default content
        return new TextEncoder().encode(this.DEFAULT_TSCONFIG);
      }
    }

    // 2) Virtual /Libs
    if (pathStr.includes("/Libs")) {
      const segments = pathStr.split("/");
      const libsIndex = segments.indexOf("Libs");
      const afterLibs = segments.slice(libsIndex + 1);
      if (afterLibs.length === 1) {
        const fileName = afterLibs[0];
        if (this.VIRTUAL_LIBS[fileName] !== undefined) {
          return new TextEncoder().encode(this.VIRTUAL_LIBS[fileName]);
        }
      }
    }

    // Otherwise, real file
    return super.readFile(resource);
  }

  override async readdir(resource: URI): Promise<[string, FileType][]> {
    console.log(
      `VirtualDiskFileSystemProvider.readdir(${resource.path.toString()})`
    );
    const pathStr = resource.path.toString();

    // If enumerating the Libs folder
    if (pathStr.endsWith("/Libs") || resource.path.base === "Libs") {
      const entries: [string, FileType][] = [];
      for (const fileName of Object.keys(this.VIRTUAL_LIBS)) {
        entries.push([fileName, FileType.File]);
      }
      return entries;
    } else {
      // Otherwise, normal disk read
      const results = await super.readdir(resource);
      // Then add the libs directory and the tsconfig.json file
      results.push(["Libs", FileType.Directory]);
      results.push(["tsconfig.json", FileType.File]);

      return results;
    }

    // Otherwise, normal disk read
    return super.readdir(resource);
  }

  // If you want to block writes to the virtual files or handle them in memory, you can override `writeFile`, `delete`, etc.
  // For instance:
  override async writeFile(
    resource: URI,
    content: Uint8Array,
    opts: any
  ): Promise<void> {
    const pathStr = resource.path.toString();
    if (pathStr.includes("/Libs") || resource.path.base === "tsconfig.json") {
      // Optionally throw an error or ignore
      throw createFileSystemProviderError(
        "Cannot modify virtual file",
        FileSystemProviderErrorCode.NoPermissions
      );
    }
    return super.writeFile(resource, content, opts);
  }
}
