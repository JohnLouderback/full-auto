import {getAllMonitors} from "@library/Tasks";
import {wait} from "@library/Utils";

console.log("Getting all monitors...");
const allMonitors = getAllMonitors();
console.log(allMonitors);
//console.log(allMonitors[0].screen.listDisplayModes());
// allMonitors[0].screen.setDisplayMode(1920, 1080, 60);
allMonitors[1].screen.setDisplayMode({
  width: 1920,
  height: 1080,
  colorDepth: 32,
  shouldPersist: false
});
allMonitors[1].makePrimary();
await wait(5000);