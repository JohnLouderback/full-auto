import {findOrAwaitWindow, sendKeys} from "@library/Tasks";

// Invoke the Windows Run dialog.
sendKeys("#r");

// Wait for the Run dialog to appear.
const runWindow = await findOrAwaitWindow(
  (window) => window.title.toLowerCase().trim() === "run" &&
    window.className === "#32770",
  10000
);
console.log(`\`runWindow\` length: ${runWindow.length}. Instance of Array? ${Array.isArray(runWindow)}`);
if (runWindow.length === 0) {
  console.error("Failed to find Run window.");
} else if (runWindow.length === 1) {
  const window = runWindow[0];
  console.log(window); // Log the window object for debugging
  console.log("Run dialog found.");
  console.log(`Window Title: ${window.title}\nWindow Class: ${window.className}`);

  sendKeys("notepad");
  sendKeys("{enter}");
} else {
  console.log(runWindow.length);
  console.log(runWindow); // Log the window objects for debugging
  throw new Error("Multiple Run windows found.");
}