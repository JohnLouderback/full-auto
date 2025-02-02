import {Tasks} from '@library/Tasks';

console.log('hello world');
await Tasks.Launch('C:\\Program Files\\WindowsApps\\Microsoft.WindowsNotepad_11.2410.21.0_x64__8wekyb3d8bbwe\\Notepad\\Notpad.exe');
console.log('goodbye world');