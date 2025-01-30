'use strict';

import * as ts from "typescript";

console.log('Hello world');

function compileTypeScript(source) {
    return ts.transpileModule(source, {
            compilerOptions: {
                module: ts.ModuleKind.ES2022,
                target: ts.ScriptTarget.ES2022
            }
        }
    ).outputText;
}