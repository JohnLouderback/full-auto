import {Tasks} from '@library/Tasks';

console.log('Launching HxD...');

const app = Tasks.launch('C:\\Program Files (x86)\\HxD\\HxD.exe');

if (app === null) {
    console.error('Failed to launch HxD.');
} else {
    console.log('HxD launched.');
    console.log(
        `Process Name: ${app.process.name},
Process Path: ${app.process.fullPath},
Process ID: 0x${app.process.pid.toString(16)}`
    );

    const windows = app.listWindows();

    console.log(typeof windows.forEach);

    console.log(`Number of windows: ${windows.length}`);

    console.log('Running for loop...');
    for (let i = 0; i < windows.length; i++) {
        const window = windows[i];
        console.log(
            `Window ${i} Title: ${window.title}`
        );
    }

    console.log('Running forEach method...');
    windows.forEach((window) => {
        console.log(
            `Window Title: ${window.title}`
        );
    });

    await app.exitSignal;
    console.log('HxD exited.');
}