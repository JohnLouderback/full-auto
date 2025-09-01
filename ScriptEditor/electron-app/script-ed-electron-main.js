// @ts-check
"use strict";

const os = require("os");
const path = require("path");
const { app } = require("electron");

// If we are within the app.asar file, this is a published build.
if (app.isPackaged) {
  const pluginsPath = path.join(process.resourcesPath, "plugins");
  // Set the default plugins path.
  process.env.THEIA_DEFAULT_PLUGINS = `local-dir:${pluginsPath}`;

  // `plugins` folder inside the `~/.script-editor` folder. This is for manually installed VS Code extensions. For example, custom themes.
  process.env.THEIA_PLUGINS = [
    process.env.THEIA_PLUGINS,
    `local-dir:${path.resolve(os.homedir(), ".script-editor", "plugins")}`
  ]
    .filter(Boolean)
    .join(",");
}

require("./lib/backend/electron-main");
