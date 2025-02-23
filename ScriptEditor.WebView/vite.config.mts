import type { UserConfig } from "vite";

import importMetaUrlPlugin from "@codingame/esbuild-import-meta-url-plugin";
import vsixPlugin from "@codingame/monaco-vscode-rollup-vsix-plugin";

export default {
  optimizeDeps: {
    esbuildOptions: {
      plugins: [importMetaUrlPlugin],
    },
    include: [
      "vscode-textmate",
      "vscode-oniguruma",
      //'@vscode/vscode-languagedetection'
    ],
  },
  build: {
    target: "ES2022",
    assetsInlineLimit: 0,
  },
  root: "./",
  plugins: [vsixPlugin()],
  cacheDir: "./vite-cache",
  server: {
    watch: {
      usePolling: true,
    },
  },
  resolve: {
    dedupe: ["vscode"],
  },
  worker: {
    format: "es",
  },
  esbuild: {
    minifySyntax: false,
  },
  preview: {
    cors: {
      origin: "*",
    },
    headers: {
      "Cross-Origin-Opener-Policy": "same-origin",
      "Cross-Origin-Embedder-Policy": "require-corp",
    },
  },
  assetsInclude: ["node_modules/**/*.html"],
} satisfies UserConfig;
