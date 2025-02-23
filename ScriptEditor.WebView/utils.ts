import type { Uri } from "vscode";

import type { IStoredWorkspace } from "@codingame/monaco-vscode-configuration-service-override";
import { RegisteredMemoryFile } from "@codingame/monaco-vscode-files-service-override";

export const disableElement = (id: string, disabled: boolean) => {
  const button = document.getElementById(id) as
    | HTMLButtonElement
    | HTMLInputElement
    | null;
  if (button !== null) {
    button.disabled = disabled;
  }
};

export const createDefaultWorkspaceFile = (
  workspaceFile: Uri,
  workspacePath: string
) => {
  return new RegisteredMemoryFile(
    workspaceFile,
    JSON.stringify(
      <IStoredWorkspace>{
        folders: [
          {
            path: workspacePath,
          },
        ],
      },
      null,
      2
    )
  );
};
