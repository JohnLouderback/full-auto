import { ContainerModule } from '@theia/core/shared/inversify';
import { FileSystemProvider } from '@theia/filesystem/lib/common/files';

import { VirtualDiskFileSystemProvider } from './virtual-project-fs';

export default new ContainerModule((bind, unbind, isBound, rebind) => {
  bind(VirtualDiskFileSystemProvider).toSelf().inSingletonScope();
  rebind(FileSystemProvider).toService(VirtualDiskFileSystemProvider);
  // bind(VirtualDiskFileSystemContribution).toSelf().inSingletonScope();
  // bind(FileServiceContribution).toService(VirtualDiskFileSystemContribution);
});
