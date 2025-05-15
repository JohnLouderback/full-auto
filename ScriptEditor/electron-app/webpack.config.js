/**
 * This file can be edited to customize webpack configuration.
 * To reset delete this file and rerun theia build again.
 */
// @ts-check
const configs = require("./gen-webpack.config.js");
const nodeConfig = require("./gen-webpack.node.config.js");
const CopyPlugin = require("copy-webpack-plugin");
const path = require("path");
const webpack = require("webpack");

/**
 * Expose bundled modules on window.theia.moduleName namespace, e.g.
 * window['theia']['@theia/core/lib/common/uri'].
 * Such syntax can be used by external code, for instance, for testing.
configs[0].module.rules.push({
    test: /\.js$/,
    loader: require.resolve('@theia/application-manager/lib/expose-loader')
}); */

nodeConfig.config.plugins.push(
  new CopyPlugin({
    patterns: [
      {
        from: path.resolve(__dirname, "../script-ed/lib/utils"),
        to: path.resolve(__dirname, "./lib/backend")
      },
      {
        from: path.resolve(__dirname, "../script-ed/lib/utils"),
        to: path.resolve(__dirname, "./lib/backend/worker")
      }
    ]
  })
);

nodeConfig.config.plugins.push(
  new webpack.BannerPlugin({
    banner: 'require("./install-all-hooks");\n',
    test: /\.[tj]s$/,
    raw: true
  })
);

module.exports = [...configs, nodeConfig.config];
