import "@codingame/monaco-vscode-typescript-basics-default-extension";
import "@codingame/monaco-vscode-typescript-language-features-default-extension";
import "@codingame/monaco-vscode-theme-defaults-default-extension";
import "@codingame/monaco-vscode-json-default-extension";
import "@codingame/monaco-vscode-json-language-features-default-extension";
import "@codingame/monaco-vscode-extension-editing-default-extension";
import "@codingame/monaco-vscode-search-result-default-extension";
import "./vsix/open-collaboration-tools.vsix";
import "./vsix/civet.vsix";

import { Uri } from "monaco-editor";
import {
  MonacoEditorLanguageClientWrapper,
  type WrapperConfig,
} from "monaco-editor-wrapper";
import { initLocaleLoader } from "monaco-editor-wrapper/vscode/locale";
import {
  defaultHtmlAugmentationInstructions,
  defaultViewsInit,
} from "monaco-editor-wrapper/vscode/services";
import { configureDefaultWorkerFactory } from "monaco-editor-wrapper/workers/workerLoaders";
import { createDefaultLocaleConfiguration } from "monaco-languageclient/vscode/services";
import * as vscode from "vscode";

import { LogLevel } from "@codingame/monaco-vscode-api";
import { type RegisterLocalProcessExtensionResult } from "@codingame/monaco-vscode-api/extensions";
import getDebugServiceOverride from "@codingame/monaco-vscode-debug-service-override";
import getEnvironmentServiceOverride from "@codingame/monaco-vscode-environment-service-override";
import getExplorerServiceOverride from "@codingame/monaco-vscode-explorer-service-override";
import getExtensionGalleryService from "@codingame/monaco-vscode-extension-gallery-service-override";
import getExtensionsServiceOverride from "@codingame/monaco-vscode-extensions-service-override";
import {
  RegisteredFileSystemProvider,
  RegisteredMemoryFile,
  registerFileSystemOverlay,
} from "@codingame/monaco-vscode-files-service-override";
import getKeybindingsServiceOverride from "@codingame/monaco-vscode-keybindings-service-override";
import getLanguagesServiceOverride from "@codingame/monaco-vscode-languages-service-override";
import getLifecycleServiceOverride from "@codingame/monaco-vscode-lifecycle-service-override";
import getLocalizationServiceOverride from "@codingame/monaco-vscode-localization-service-override";
import getLogServiceOverride from "@codingame/monaco-vscode-log-service-override";
import getOutputServiceOverride from "@codingame/monaco-vscode-output-service-override";
import getRemoteAgentServiceOverride from "@codingame/monaco-vscode-remote-agent-service-override";
import getSearchServiceOverride from "@codingame/monaco-vscode-search-service-override";
import getSecretStorageServiceOverride from "@codingame/monaco-vscode-secret-storage-service-override";
import getStorageServiceOverride from "@codingame/monaco-vscode-storage-service-override";
import getTextMateServiceOverride from "@codingame/monaco-vscode-textmate-service-override";
import getThemeServiceOverride from "@codingame/monaco-vscode-theme-service-override";
import getBannerServiceOverride from "@codingame/monaco-vscode-view-banner-service-override";
import getStatusBarServiceOverride from "@codingame/monaco-vscode-view-status-bar-service-override";
import getTitleBarServiceOverride from "@codingame/monaco-vscode-view-title-bar-service-override";

import { createDefaultWorkspaceFile } from "./utils.js";

(async () => {
  await initLocaleLoader();

  const workspaceFile = vscode.Uri.file(
    "/workspace/.vscode/workspace.code-workspace"
  );

  const initialFile = {
    uri: "/workspace/test.ts",
    text: 'export const x: number = "hello";',
  };

  const file2 = {
    uri: "/workspace/test2.ts",
    text: 'export const y: number = "hello";',
  };

  const wrapper = new MonacoEditorLanguageClientWrapper();
  const wrapperConfig: WrapperConfig = {
    $type: "extended",
    id: "AAP",
    htmlContainer: document.body,
    logLevel: LogLevel.Trace,
    vscodeApiConfig: {
      serviceOverrides: {
        ...getKeybindingsServiceOverride(),
        ...getLifecycleServiceOverride(),
        ...getLocalizationServiceOverride(createDefaultLocaleConfiguration()),
        ...getBannerServiceOverride(),
        ...getStatusBarServiceOverride(),
        ...getTitleBarServiceOverride(),
        ...getExplorerServiceOverride(),
        ...getRemoteAgentServiceOverride(),
        ...getEnvironmentServiceOverride(),
        ...getSecretStorageServiceOverride(),
        ...getStorageServiceOverride(),
        ...getSearchServiceOverride(),
        ...getTextMateServiceOverride(),
        ...getThemeServiceOverride(),
        ...getLanguagesServiceOverride(),
        ...getExtensionsServiceOverride(),
        ...getExtensionGalleryService(),
        ...getLogServiceOverride(),
        ...getDebugServiceOverride(),
        ...getOutputServiceOverride(),
      },
      enableExtHostWorker: true,
      viewsConfig: {
        viewServiceType: "ViewsService",
        htmlAugmentationInstructions: defaultHtmlAugmentationInstructions,
        viewsInitFunc: defaultViewsInit,
      },
      userConfiguration: {
        json: JSON.stringify({
          "workbench.colorTheme": "Default Dark Modern",
          "typescript.tsserver.trace": "verbose", // enable verbose tracing
          "typescript.tsserver.log": "verbose", // enable verbose logging
          "typescript.tsserver.web.projectWideIntellisense.enabled": true,
          "typescript.tsserver.web.projectWideIntellisense.suppressSemanticErrors":
            false,
          "typescript.tsserver.web.typeAcquisition.enabled": true,
          "typescript.tsserver.useSyntaxServer": "auto",
          "typescript.tsserver.experimental.enableProjectDiagnostics": true,
          "editor.guides.bracketPairsHorizontal": "active",
          "editor.lightbulb.enabled": "On",
          "editor.wordBasedSuggestions": "off",
          "editor.experimental.asyncTokenization": false,
          "editor.semanticHighlighting.enabled": true,
          "oct.serverUrl": "https://api.open-collab.tools/",
        }),
      },
      workspaceConfig: {
        enableWorkspaceTrust: true,
        windowIndicator: {
          label: "ScriptEditor",
          tooltip: "",
          command: "",
        },
        workspaceProvider: {
          trusted: true,
          workspace: {
            workspaceUri: workspaceFile,
          },
          async open() {
            window.open(window.location.href);
            return true;
          },
        },
        configurationDefaults: {
          "window.title": "ScriptEd${separator}${dirty}${activeEditorShort}",
        },
        productConfiguration: {
          nameShort: "ScriptEd",
          nameLong: "ScriptEditor",
        },
      },
    },
    extensions: [
      {
        config: {
          name: "script-editor",
          publisher: "John Louderback",
          version: "1.0.0",
          engines: {
            vscode: "*",
          },
        },
      },
    ],
    editorAppConfig: {
      // codeResources: {
      //   modified: initialFile,
      // },
      monacoWorkerFactory: configureDefaultWorkerFactory,
    },
  };

  const tsconfigUri = Uri.file("/workspace/tsconfig.json");
  const tsconfigContent = JSON.stringify({
    compilerOptions: {
      target: "es6",
      module: "commonjs",
      strict: true,
    },
    include: ["**/*.ts"],
  });

  // Create a memory-based file system provider
  const fileSystemProvider = new RegisteredFileSystemProvider(false);
  // Register your virtual tsconfig file in that provider
  fileSystemProvider.registerFile(
    new RegisteredMemoryFile(tsconfigUri, tsconfigContent)
  );
  fileSystemProvider.registerFile(
    new RegisteredMemoryFile(Uri.file(initialFile.uri), initialFile.text)
  );
  fileSystemProvider.registerFile(
    new RegisteredMemoryFile(Uri.file(file2.uri), file2.text)
  );
  fileSystemProvider.registerFile(
    createDefaultWorkspaceFile(workspaceFile, "/workspace")
  );

  // Now register the provider as an overlay for the workspace (priority 1 in this example)
  const overlayDisposable = registerFileSystemOverlay(1, fileSystemProvider);
  //await fileSystemProvider.readdir(Uri.file("workspace"));
  await wrapper.init(wrapperConfig);

  const result = wrapper.getExtensionRegisterResult(
    "script-editor"
  ) as RegisterLocalProcessExtensionResult;
  result.setAsDefaultApi();

  const testFileUri = vscode.Uri.file("/workspace/test.ts");
  const doc = await vscode.workspace.openTextDocument(testFileUri);

  const testFile2Uri = vscode.Uri.file("/workspace/test2.ts");
  const doc2 = await vscode.workspace.openTextDocument(testFile2Uri);

  await vscode.window.showTextDocument(doc);

  // Set-up the global objects for debugging.
  (window as any).vscode = vscode;
  (window as any).wrapper = wrapper;
  (window as any).fileSystemProvider = fileSystemProvider;
  (window as any).Uri = Uri;
})();
