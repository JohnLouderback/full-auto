import {getFile} from '@library/Tasks';
import {Directory} from "@library/Directory";

const file = getFile("H:\\bp-readings-and-notes.csv");

// console.log(await file.readAllText());

// await file.readLines(console.log);

// const bytes = await file.readAllBytes();
//
// let str = "";
//
// for (let i = 0; i < bytes.length; i++) {
//   str += String.fromCharCode(bytes[i])
// }
//
// console.log(str);

let str = "";

await file!.readBytes((byte) => str += String.fromCharCode(byte));


console.log(str);