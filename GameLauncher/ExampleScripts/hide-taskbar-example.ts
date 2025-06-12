import {getTaskbar} from "@library/Tasks";
import {wait} from "@library/Utils";

const taskbar = getTaskbar();

// console.log(`Taskbar auto-hide is currently ${taskbar.isAutoHideEnabled() ? "enabled" : "disabled"}.`);
//
// taskbar.toggleAutoHide();
//
// console.log(`Taskbar auto-hide is now ${taskbar.isAutoHideEnabled() ? "enabled" : "disabled"}.`);
//
// await wait(5000);

console.log(`Is taskbar showing? ${taskbar.isShowing}`);

taskbar.hide();

console.log(`Is taskbar showing? ${taskbar.isShowing}`);

await wait(5000);

taskbar.show();