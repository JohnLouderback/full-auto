import {awaitWindow, launch} from "@library/Tasks";
import {wait} from "@library/Utils";

console.log("Launching Fork...");

console.log("Waiting for 1 second...");
await wait(1000);
console.log("Waited for 1 second.");

const app = launch(
  "C:\\Users\\John\\AppData\\Local\\Fork\\app-2.5.0\\Fork.exe"
);

if (app === null) {
  console.error("Failed to launch Fork.");
} else {
  console.log("Fork launched.");
  console.log(
    `Process Name: ${app.process.name},
Process Path: ${app.process.fullPath},
Process ID: 0x${app.process.pid.toString(16)}`
  );

  const window = await awaitWindow(
    (window) =>
      window.title === "Fork" &&
      window.className.startsWith("HwndWrapper[Fork.exe;;"),
    10000
  );

  if (window === null) {
    console.error("Failed to find Fork window.");
  } else {
    console.log("Fork window found.");
    console.log(`Window Title: ${window.title}\nWindow Class: ${window.className}`);
    console.log('Waiting for window to be shown...');
    // await window.shownSignal;
    // console.log(window.getBoundingBox());
    window.on('shown', async () => {
      console.log('Window shown.');
      // Log the window's bounding box every 100 milliseconds for 1 second.
      await wait(1000);
      console.log(window.getBoundingBox());
      window.on('boundsChanged', () => {
        console.log(window.getBoundingBox());
      });
    });
    window.on('maximized', () => {
      console.log('Window maximized.');
    });
    window.on('minimized', () => {
      console.log('Window minimized.');
    });
    window.on('restored', () => {
      console.log('Window restored.');
    });
    window.on('closed', () => {
      console.log('Window closed.');
    });
    window.on('focused', () => {
      console.log('Window focused.');
    });
  }

  await app.exitSignal;
  console.log("Fork exited.");
}
