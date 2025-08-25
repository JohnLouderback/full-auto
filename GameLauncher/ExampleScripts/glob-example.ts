import {glob} from "@library/Tasks";

//const files = await glob("C:\\Windows\\Fonts", "*.ttf");
const files = await glob("./", "*");

console.log(files.map(f => f.path));