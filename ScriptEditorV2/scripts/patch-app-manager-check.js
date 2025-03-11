/**
 * This script is used to patch the
 * `node_modules/@theia/application-manager/lib/application-package-manager.js` file. It
 * comments out the check for electron versions matching Theia's expectations. This way,
 * we can use the Supermium-Electron binaries. It comments out `throw new
 * AbortError('Updated dependencies, please run "install" again');` and `throw new
 * AbortError('Dependencies are out of sync, please run "install" again');`.
 */

const fs = require('fs');
const path = require('path');

const APP_MANAGER_PATH = path.resolve(__dirname, '../node_modules/@theia/application-manager/lib/application-package-manager.js');

const SEARCH_STRING_1 = 'throw new AbortError(\'Updated dependencies, please run "install" again\');';
const SEARCH_STRING_2 = 'throw new AbortError(\'Dependencies are out of sync, please run "install" again\');';

const fileContents = fs.readFileSync(APP_MANAGER_PATH, 'utf8');
const newContents = fileContents
    .replace(SEARCH_STRING_1, `// ${SEARCH_STRING_1}`)
    .replace(SEARCH_STRING_2, `// ${SEARCH_STRING_2}`);

fs.writeFileSync(APP_MANAGER_PATH, newContents);
console.log('Patched application-package-manager.js');

// Next we'll update `node_modules\@theia\cli\lib\check-dependencies.js` to fix the
// workspaces iteration issue.
const CLI_PATH = path.resolve(__dirname, '../node_modules/@theia/cli/lib/check-dependencies.js');

const SEARCH_STRING_3 = 'const wsGlobs = (_a = options.workspaces) !== null && _a !== void 0 ? _a : readWorkspaceGlobsFromPackageJson();';

const fileContents2 = fs.readFileSync(CLI_PATH, 'utf8');
const newContents2 = fileContents2
    .replace(SEARCH_STRING_3, `let wsGlobs = (_a = options.workspaces) !== null && _a !== void 0 ? _a : readWorkspaceGlobsFromPackageJson();\n    if (!Array.isArray(wsGlobs) && 'packages' in wsGlobs) wsGlobs = wsGlobs.packages;`);

fs.writeFileSync(CLI_PATH, newContents2);

console.log('Patched check-dependencies.js');
