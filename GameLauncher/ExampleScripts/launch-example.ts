import {Tasks} from '@library/Tasks';

console.log('Launching Fork...');

const wait = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

Tasks.awaitWindow({
    title: 'Fork',
    className: 'HwndWrapper[Fork.exe;;6577e698-1421-4227-805b-5a7a96b9a05f]'
}, 10000).then(window => {
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