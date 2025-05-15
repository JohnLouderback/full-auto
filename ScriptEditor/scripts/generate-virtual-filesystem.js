// @ts-check
/**
 * @typedef {import('../script-ed/src/utils/vfs/schema.ts').Directory} Directory
 * @typedef {import('fs').Dirent} Dirent
 */
const fs = require("fs");
const path = require("path");
const ROOT = path.resolve(__dirname, "..", "..");
const LIBS_PATH = path.join(ROOT, "GameLauncher", "Libs");
const TSCONFIG_PATH = path.join(ROOT, "GameLauncher", "tsconfig.json");
const VFS_PATH = path.join(
  __dirname,
  "..",
  "script-ed",
  "src",
  "utils",
  "vfs",
  "vfs.ts"
);

/**
 * Reads the file "tsconfig.json" from "../../GameLauncher/tsconfig.json" and directory
 * "Libs" from "../../GameLauncher/Libs" and generates a virtual file system in the form
 * of {@link Directory} at the path "../script-ed/src/utils/vfs/vfs.ts".
 *
 * @function generateVirtualFileSystem
 * @returns {Directory} The generated virtual file system.
 */
function generateVirtualFileSystem() {
  const path = require("path");
  const { readdir } = require("fs").promises;

  const tsconfig = fs.readFileSync(TSCONFIG_PATH, "utf8");
  const libs = fs.readdirSync(LIBS_PATH, { withFileTypes: true });

  const libsDirectoryStructure = dirEntsToDirectory(libs);

  return {
    "tsconfig.json": tsconfig,
    Libs: libsDirectoryStructure,
  };
}

/**
 * Converts an array of {@link Dirent} to a {@link Directory} object.
 * @param {Dirent[]} dirEnts The array of {@link Dirent} to convert.
 * @returns {Directory} The converted {@link Directory} object
 */
function dirEntsToDirectory(dirEnts) {
  return dirEnts.reduce((acc, dirEnt) => {
    acc[dirEnt.name] = dirEnt.isDirectory()
      ? dirEntsToDirectory(
          fs.readdirSync(
            path.join(dirEnt.parentPath ?? LIBS_PATH, dirEnt.name),
            {
              withFileTypes: true,
            }
          )
        )
      : fs.readFileSync(
          path.join(dirEnt.parentPath ?? LIBS_PATH, dirEnt.name),
          "utf8"
        );
    return acc;
  }, {});
}

/**
 * Writes the virtual file system to the file "../script-ed/src/utils/vfs/vfs.ts".
 */
function writeVirtualFileSystem() {
  const vfs = generateVirtualFileSystem();
  console.log("Writing virtual file system to:", VFS_PATH);
  fs.writeFileSync(
    VFS_PATH,
    `// This file is auto-generated. Do not modify manually.\n\nimport { Directory } from "./schema";\n\nexport const virtualFileSystem: Directory = ${JSON.stringify(
      vfs,
      null,
      2
    )};`
  );
  console.log("Virtual file system successfully written.");
}

writeVirtualFileSystem();
