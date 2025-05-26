import {awaitWindow, launch} from "@library/Tasks";
import {wait} from "@library/Utils";

console.log("Launching Fork...");

console.log("Waiting for 1 second...");
await wait(1000);
console.log("Waited for 1 second.");

const app = launch(
  "C:\\Users\\John\\AppData\\Local\\Fork\\current\\Fork.exe"
);

if (app === null) {
  console.error("Failed to launch Fork.");
} else {
  console.log(app);
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
    console.log(window); // Log the window object for debugging
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
      window.makeBorderless();
      window.makeFullscreen("alt enter");
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
      window.constrainCursor();
    });
  }

  console.log(`Is process protected? ${app.process.isProtected}`);

  // Set the affinity of the process to use only the first 4 CPU cores.
  app.process.setAffinity(4);
  // Set the priority of the process to below normal.
  app.process.setPriority('below normal');

  if (!window.isShowing) await window.shown;
  console.log('Downscaling Fork...');
  const downscaleWindow = await window.downscale({
    x: 0,
    y: 0,
    width: 640,
    height: 360,
  });
  if (!downscaleWindow.isShowing) await downscaleWindow.shown;
  downscaleWindow.move(1, 1);
  window.move(1, 1);


  await app.exitSignal;
  console.log(app);
  console.log("Fork exited.");
}
