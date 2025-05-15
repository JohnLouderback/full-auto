import { MaybePromise } from '@theia/core';
import { EnvVariablesServer } from '@theia/core/lib/common/env-variables';
import {
  inject,
  injectable,
} from '@theia/core/shared/inversify';
import {
  FileService,
  FileServiceContribution,
} from '@theia/filesystem/lib/browser/file-service';
import { FileSystemProvider } from '@theia/filesystem/lib/common/files';

@injectable()
export class VirtualDiskFileSystemContribution
  implements FileServiceContribution
{
  @inject(FileSystemProvider)
  protected readonly fileSystemProvider: FileSystemProvider;

  @inject(EnvVariablesServer)
  protected readonly environments: EnvVariablesServer;

  registerFileSystemProviders(service: FileService): void {
    service.onWillActivateFileSystemProvider((event) => {
      if (event.scheme === "file") {
        event.waitUntil(
          (async () => {
            service.registerProvider("file", this.fileSystemProvider);
          })()
        );
      }
    });
  }

  protected getDelegate(
    service: FileService
  ): MaybePromise<FileSystemProvider> {
    return service.activateProvider("file");
  }
}
