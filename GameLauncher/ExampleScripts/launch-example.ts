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
    setTimeout(() => {
        console.log('Killing HxD...');
        app.process.kill();
    }, 5000);

    // count down...
    for (let i = 5; i > 0; i--) {
        console.log(i);
        await new Promise(resolve => setTimeout(resolve, 1000));
    }

    await app.exitSignal;
    console.log('HxD exited.');
}