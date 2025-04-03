import {awaitWindow, sendKeys} from "@library/Tasks";

// Wait for the Run dialog to appear.
const runWindow = awaitWindow(
  (window) => window.title.toLowerCase().trim() === "run" &&
    window.className === "#32770",
  10000
).then((window) => {
  if (window === null) {
    console.error("Failed to find Run dialog.");
    return null;
  } else {
    console.log(window); // Log the window object for debugging
    console.log("Run dialog found.");
    console.log(`Window Title: ${window.title}\nWindow Class: ${window.className}`);

    sendKeys("notepad");
    sendKeys("{enter}");

    return window;
  }
});

// Invoke the Windows Run dialog.
sendKeys("#r");

// Ensures the script does not exit before the Run dialog is shown (or not as the case may be).
await runWindow;