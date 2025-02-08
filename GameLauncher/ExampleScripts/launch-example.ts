import {awaitWindow, launch} from "@library/Tasks";

console.log("Launching Fork...");

const wait = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

const app = launch(
  "C:\\Users\\John\\AppData\\Local\\Fork\\app-2.4.3\\Fork.exe"
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
    console.log(window.getBoundingBox());
  }

  await app.exitSignal;
  console.log("Fork exited.");
}
