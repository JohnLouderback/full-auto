import * as child_process from "child_process";
import * as path from "path";

console.log("installing child process hooks");

type Writeable<T extends { [x: string]: any }, K extends string> = {
  [P in K]: T[P];
};

const childProcessProxy = child_process as Writeable<
  typeof child_process,
  keyof typeof child_process
>;

// Store the old versions of the process spawning functions
const oldSpawn = childProcessProxy.spawn;
const oldFork = childProcessProxy.fork;

// Next, we'll intercept the spawn and fork functions, log the command and args, then if
// the original executable is "node", we'll prepend a require argument and load in
// "./install-fs-hooks.js", relative to this file's directory. Additionally, we'll also
// pass this child process hook installer as well.
// This will allow us to intercept all file system calls made by the child process and
// log them to the console.
function isNodeExecutable(executable: string | URL) {
  if (typeof executable === "string") {
    return executable.endsWith("node") || executable.endsWith("node.exe");
  }
  if (executable instanceof URL) {
    return (
      executable.pathname.endsWith("node") ||
      executable.pathname.endsWith("node.exe")
    );
  }
}
function isStringArray(arg: any): arg is readonly string[] {
  return arg instanceof Array && arg.every((item) => typeof item === "string");
}
function isForkOptions(
  options: child_process.ForkOptions | string | readonly string[] | unknown
): options is child_process.ForkOptions {
  return typeof options === "object";
}
// @ts-expect-error
childProcessProxy.spawn = (...args) => {
  console.log("spawn was called with args:", args);
  const [executable, ...rest] = args;
  if (isStringArray(rest[0])) {
    const [commandArgs, ...restArgs] = rest;
    console.log(
      "spawn was called with executable:",
      executable,
      "args:",
      commandArgs
    );
    if (isNodeExecutable(executable)) {
      console.log(
        "Intercepted node spawn, prepending require for install-fs-hooks"
      );
      return oldSpawn(
        executable,
        [
          "-r",
          path.resolve(__dirname, "./install-fs-hooks.js"),
          "-r",
          path.resolve(__dirname, "./install-child-proc-hooks.js"),
          ...commandArgs,
        ],
        // @ts-expect-error
        ...restArgs
      );
    }
    // @ts-expect-error
    return oldSpawn(executable, ...rest);
  }
  // @ts-expect-error
  return oldSpawn(...args);
};

childProcessProxy.fork = (...args) => {
  // Fork is a little different than spawn. Fork always runs a new node process, so we
  // don't need to check if the executable is "node". We can just prepend the require
  // argument in the options object via the execArgv property.

  let options: child_process.ForkOptions | undefined;
  let modArgs: readonly string[] = [];

  // First we'll determine if there are any arguments being passed to the module itself.
  if (isStringArray(args[1])) {
    modArgs = args[1];
    if (isForkOptions(args[2])) {
      options = args[2];
    }
  } else if (isForkOptions(args[1])) {
    options = args[1];
  }

  // If no options object was passed, create one
  if (!options) {
    options = {
      execArgv: [],
    };
  }

  // If an options object was passed, but no execArgv property exists, create one
  if (!options.execArgv) {
    options.execArgv = [];
  }

  // Add the require argument to the execArgv array
  options.execArgv.push(
    "-r",
    path.resolve(__dirname, "./install-fs-hooks.js"),
    "-r",
    path.resolve(__dirname, "./install-child-proc-hooks.js")
  );

  console.log(
    `fork was called with args: ${options.execArgv.join(" ")} ${
      args[0]
    } -- ${modArgs.join(" ")}`
  );

  // Finally, call the original fork function with the modified arguments
  return oldFork(args[0], modArgs, options);
};
