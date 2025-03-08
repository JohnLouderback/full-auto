import { injectable } from 'inversify';

import {
  ElectronMainApplication,
  ElectronMainApplicationContribution,
} from '@theia/core/lib/electron-main/electron-main-application';

//import * as electron from '@theia/electron/shared/electron';

@injectable()
export class CustomElectronMainContribution
  implements ElectronMainApplicationContribution
{
  constructor() {
    console.log("CustomElectronMainContribution created");
    //electron.app.commandLine.appendSwitch("disable-lcd-text");
    // electron.app.commandLine.appendSwitch("disable-gpu");
    // electron.app.commandLine.appendSwitch("enable-use-zoom-for-dsf");
    // electron.app.commandLine.appendSwitch("disable-direct-write");
  }

  onStart(application: ElectronMainApplication): void {
    console.log("CustomElectronMainContribution started");
    //electron.app.commandLine.appendSwitch("disable-lcd-text");
    // electron.app.commandLine.appendSwitch("disable-lcd-text");
    // electron.app.commandLine.appendSwitch("disable-direct-write");
    // electron.app.commandLine.appendSwitch("force-gdi");
    // electron.app.commandLine.appendSwitch("force-gdi", "true");
    //    electron.app.disableHardwareAcceleration();
    // Add other command-line switches or initialization code here
  }
}
