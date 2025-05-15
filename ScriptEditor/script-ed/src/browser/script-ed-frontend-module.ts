import "../style/styles.css";

import { FrontendApplicationContribution } from "@theia/core/lib/browser";
import { CommandContribution, MenuContribution } from "@theia/core/lib/common";
import { ContainerModule } from "@theia/core/shared/inversify";

import { CustomCssContribution } from "./custom-css-contribution";
import {
  ScriptEdCommandContribution,
  ScriptEdMenuContribution,
} from "./script-ed-contribution";

//import { VirtualDiskFileSystemProvider } from './virtual-project-fs';

export default new ContainerModule((bind) => {
  bind(FrontendApplicationContribution).to(CustomCssContribution);
  bind(CommandContribution).to(ScriptEdCommandContribution);
  bind(MenuContribution).to(ScriptEdMenuContribution);
});
