import "../utils/install-child-proc-hooks";

import { ContainerModule } from "@theia/core/shared/inversify";

export default new ContainerModule((bind, unbind, isBound, rebind) => {
  //bind(VirtualDiskFileSystemProvider).toSelf().inSingletonScope();
  //rebind(FileSystemProvider).toService(VirtualDiskFileSystemProvider);
  // bind(VirtualDiskFileSystemContribution).toSelf().inSingletonScope();
  // bind(FileServiceContribution).toService(VirtualDiskFileSystemContribution);
});
