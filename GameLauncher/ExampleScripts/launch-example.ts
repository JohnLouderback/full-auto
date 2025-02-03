import {Tasks} from '@library/Tasks';

console.log('hello world');
await Tasks.launch('"C:\\Program Files (x86)\\HxD\\HxD.exe"');
console.log('goodbye world');