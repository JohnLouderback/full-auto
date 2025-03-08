import {
  ElectronMainApplicationContribution,
} from '@theia/core/lib/electron-main/electron-main-application';
import { ContainerModule } from '@theia/core/shared/inversify';
import { app } from '@theia/electron/shared/electron';

import { CustomElectronMainContribution } from './electron-main-contribution';

//electron.app.commandLine.appendSwitch("disable-lcd-text");
// app.commandLine.appendSwitch("disable-font-subpixel-positioning");
app.commandLine.appendSwitch("disable-gpu");
app.commandLine.appendSwitch("disable-gpu-compositing");
// app.commandLine.appendSwitch("enable-use-zoom-for-dsf");
// app.commandLine.appendSwitch("disable-direct-write");
// app.commandLine.appendSwitch("disable-font-antialiasing");
app.commandLine.appendSwitch("force-color-profile", "srgb");
app.commandLine.appendSwitch("disable-directwrite-for-ui");
app.commandLine.appendSwitch("use-angle", "d3d9");
app.commandLine.appendSwitch("enable-fontations-backend", "false");
app.commandLine.appendSwitch("disable-gpu-rasterization");
app.commandLine.appendSwitch("force-gdi");
app.commandLine.appendSwitch("disable-lcd-text");
app.commandLine.appendSwitch("disable-browser-font-smoothing-win");
//app.disableHardwareAcceleration();

export default new ContainerModule((bind) => {
  bind(ElectronMainApplicationContribution)
    .to(CustomElectronMainContribution)
    .inSingletonScope();
});
