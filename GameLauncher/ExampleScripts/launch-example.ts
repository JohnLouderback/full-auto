import {Tasks} from '@library/Tasks';

console.log('Launching Fork...');

const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

Tasks.awaitWindow('Fork', 10000).then(window => {
    if (window === null) {
        console.error('Failed to find Fork window.');
    } else {
        console.log('Fork window found.');
        console.log(window.getBoundingBox());
    }
});

const app = Tasks.launch('C:\\Users\\John\\AppData\\Local\\Fork\\app-2.4.3\\Fork.exe');

if (app === null) {
    console.error('Failed to launch Fork.');
} else {
    console.log('Fork launched.');
    console.log(
        `Process Name: ${app.process.name},
Process Path: ${app.process.fullPath},
Process ID: 0x${app.process.pid.toString(16)}`
    );


    await app.exitSignal;
    console.log('Fork exited.');
}